using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace VendorRando {
    public class RequestModifier {
        private static FieldInfo _placements;
        private static string salubraLocation;

        public static void HookRequestBuilder() {
            _placements = typeof(ICFactory).GetField("_placements", BindingFlags.Instance | BindingFlags.NonPublic);

            RequestBuilder.OnUpdate.Unsubscribe(-100, BuiltinRequests.ApplyShopDefs);
            RequestBuilder.OnUpdate.Subscribe(-100, ApplyNewShopDefs);
            RequestBuilder.OnUpdate.Unsubscribe(-100, BuiltinRequests.ApplySalubraCharmDef);
            RequestBuilder.OnUpdate.Subscribe(-99, ApplyNewSalubraCharmDef);
            RequestBuilder.OnUpdate.Subscribe(314, RemoveOldShops);
        }

        public static void ApplyNewShopDefs(RequestBuilder rb) {
            Dictionary<string, string> vendorMap = new();
            List<string> mutables = new();
            foreach(var pool in Data.Pools) {
                if(pool.IsIncluded(rb.gs)) {
                    foreach(string location in pool.IncludeLocations) {
                        if(IsValidLocation(location)) {
                            mutables.Add(location);
                        }
                    }
                }
            }
            (bool, string, string)[] settingsMap = new (bool, string, string)[] {
                (VendorRando.Settings.Sly, Consts.Sly, LocationNames.Sly),
                (VendorRando.Settings.Salubra, Consts.Salubra, LocationNames.Salubra),
                (VendorRando.Settings.Iselda, Consts.Iselda, LocationNames.Iselda),
                (VendorRando.Settings.LegEater, Consts.LegEater, LocationNames.Leg_Eater),
                (VendorRando.Settings.Lemm, Consts.Lemm, LocationNames.Lemm)
            };
            //include rando'd shops as possible locations
            foreach((bool setting, string shop, string vanillaShop) in settingsMap) {
                if(setting) {
                    mutables.Add(vanillaShop);//todo this doesn't work yet
                }
            }
            //pre-decide shop locations
            foreach((bool setting, string shop, string vanillaShop) in settingsMap) {
                if(setting) {
                    bool safeFromCircularLogic = false;
                    string chosenLocation;
                    do {
                        chosenLocation = mutables[rb.rng.Next(mutables.Count)];
                        safeFromCircularLogic = CheckCircularLogic(shop, chosenLocation);
                    } while(!safeFromCircularLogic);
                    vendorMap.Add(shop, chosenLocation);
                    if(shop == Consts.Salubra)
                        salubraLocation = Finder.GetLocation(chosenLocation).sceneName;
                    VendorRando.vlog($"Chosen {chosenLocation} for {shop}");
                    mutables.Remove(chosenLocation);
                }
            }

            foreach((bool setting, string shop, string vanillaShop) in settingsMap) {
                if(setting) {
                    EditLocation_newVendor(shop, vanillaShop);
                    if(mutables.Contains(vanillaShop)) {
                        EditLocation_noLongerShopLocation(shop, vanillaShop);
                    }
                    if(shop == Consts.Sly) {
                        AddLocation_newVendor_slyKey();
                    }
                }
                else {
                    if(shop != Consts.Lemm) {
                        EditLocation_originalBehavior(vanillaShop);
                    }
                    if(shop == Consts.Sly) {
                        EditLocation_originalBehavior(LocationNames.Sly_Key);
                    }
                }
            }

            static int GetShopCost(Random rng, RandoModItem item) {
                double pow = 1.2;
                int cap = item.ItemDef is not null ? item.ItemDef.PriceCap : 500;
                if(cap <= 100)
                    return cap;
                if(item.Required)
                    return rng.PowerLaw(pow, 100, Math.Min(cap, 500)).ClampToMultipleOf(5);
                return rng.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
            }

            bool IsValidLocation(string location) {
                AbstractLocation loc = Finder.GetLocation(location);
                var mp = loc.Wrap();
                if(mp is MutablePlacement) {
                    Type type = loc.GetType();
                    if(type == typeof(CoordinateLocation)) {
                        if(loc.sceneName == "RestingGrounds_07" || loc.sceneName == "Crossroads_38" || location == LocationNames.Grimmchild) {
                            return false;
                        }
                        return true;
                    }
                    if(type == typeof(ObjectLocation))
                        return true;
                    if(type == typeof(ItemChanger.Locations.SpecialLocations.StagLocation))
                        return true;
                }
                return false;
            }

            bool CheckCircularLogic(string shop, string location) {
                switch(shop) {
                    case Consts.Sly:
                        if(!rb.gs.PoolSettings.Keys && ContainsLogic(location, ["SIMPLE4", "LANTERN", "DARKROOMS", "ELEGANT"]))
                            return false;
                        break;
                    case Consts.Salubra:
                        if(!rb.gs.PoolSettings.Charms && ContainsLogic(location, ["LONGNAIL", "LONGMARK"]))
                            return false;
                        break;
                    case Consts.LegEater:
                        if(!rb.gs.PoolSettings.Charms && ContainsLogic(location, ["FRAGILEHEART", "FRAGILEGREED", "FRAGILESTRENGTH"]))
                            return false;
                        break;
                    case Consts.Lemm:
                        if(ContainsLogic(location, ["Can_Visit_Lemm"]))
                            return false;
                        break;
                }
                return true;
            }

            bool ContainsLogic(string location, string[] clauses) {
                List<string> newList = new();
                return ContainsLogic2(location, clauses, ref newList);
            }

            bool ContainsLogic2(string location, string[] clauses, ref List<string> alreadyChecked) {
                if(alreadyChecked.Contains(location))
                    return false;
                alreadyChecked.Add(location);
                if(rb.lm.LogicLookup.TryGetValue(location, out LogicDef def)) {
                    foreach(Term term in def.GetTerms()) {
                        foreach(string clause in clauses) {
                            if(term.ToString() == clause) {
                                return true;
                            }
                        }
                        if(ContainsLogic2(term.ToString(), clauses, ref alreadyChecked)) {
                            return true;
                        }
                    }
                }
                return false;
            }
            
            //setting is turned off, normal Rando4 ApplyShopDefs
            void EditLocation_originalBehavior(string s) {
                rb.EditLocationRequest(s, info => {
                    info.customPlacementFetch = (factory, placement) => {
                        if(factory.TryFetchPlacement(s, out AbstractPlacement ap)) {
                            return ap;
                        }
                        ShopPlacement sp = (ShopPlacement)factory.FetchOrMakePlacement(s);
                        sp.defaultShopItems = RandomizerMod.IC.Shops.GetDefaultShopItems(factory.gs);
                        return sp;
                    };
                    info.onRandomizerFinish += placement => {
                        if(placement.Location is not RandoModLocation rl || placement.Item is not RandoModItem ri || rl.costs == null)
                            return;
                        foreach(LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>()) {
                            if(gc.GeoAmount < 0)
                                gc.GeoAmount = GetShopCost(rb.rng, ri);
                        }
                    };
                });
            }

            //random check location, my vendor container
            void EditLocation_newVendor(string shop, string vanillaShop) {
                string myLocation = vendorMap[shop];
                rb.EditLocationRequest(myLocation, info => {
                    info.customPlacementFetch = (factory, placement) => {
                        if(factory.TryFetchPlacement(myLocation, out AbstractPlacement ap)) {
                            return ap;
                        }
                        var p = _placements.GetValue(factory) as Dictionary<string, AbstractPlacement>;
                        if(p.TryGetValue(myLocation, out AbstractPlacement placement1)) {
                            return placement1;
                        }
                        else {
                            if(p.ContainsKey(myLocation))
                                throw new ArgumentException($"Placement {myLocation} already exists!");
                            return PlaceShopWithDefaults(shop, myLocation, RandomizerMod.IC.Shops.GetDefaultShopItems(rb.gs), factory);
                        }
                    };
                    if(shop != Consts.Lemm) {
                        info.AddGetLocationDefModifier(myLocation, def => def with { FlexibleCount = true, AdditionalProgressionPenalty = true });
                        info.onRandoLocationCreation += (factory, rl) => {
                            LogicManager lm = factory.lm;
                            rl.logic = lm.GetLogicDefStrict(myLocation);
                            if(Data.TryGetCost(vanillaShop, out CostDef def)) {
                                rl.AddCost(new LogicGeoCost(lm, def.Amount));
                            }
                        };
                        info.onRandomizerFinish += placement => {
                            if(placement.Location is not RandoModLocation rl || placement.Item is not RandoModItem ri || rl.costs == null)
                                return;
                            foreach(LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>()) {
                                if(gc.GeoAmount < 0)
                                    gc.GeoAmount = GetShopCost(rb.rng, ri);
                            }
                        };
                    }
                    else {
                        rb.RemoveLocationByName(myLocation);//not doing anything??
                    }
                });
            }

            void AddLocation_newVendor_slyKey() {
                string baseLocation = vendorMap[Consts.Sly];
                //string myLocation = $"{baseLocation}_(Key)";
                string myLocation = Consts.SlyKey;
                /*ObjectLocation objLocation = new() {
                    name = myLocation,
                    objectName = Consts.Sly,
                    sceneName = Finder.GetLocation(baseLocation).sceneName,
                    forceShiny = false,
                    flingType = FlingType.DirectDeposit
                };
                InteropTag tag = objLocation.AddTag<InteropTag>();
                tag.Message = ConnectionMetadataInjector.SupplementalMetadata.InteropTagMessage;
                tag.Properties["ModSource"] = VendorRando.instance.GetName();
                //InteropTag oldTag = Finder.GetLocation(baseLocation).GetTag<InteropTag>();
                //tag.Properties["WorldMapLocations"] = oldTag.Properties["WorldMapLocations"];
                //tag.Properties["PinSprite"] = oldTag.Properties["PinSprite"];
                Finder.DefineCustomLocation(objLocation);
                /*LogicAdder.slyKeyLocation = (baseLocation, myLocation);
                LogicAdder.slyAssigned = true;*/

                //Finder.GetLocation(Consts.SlyKey).sceneName = Finder.GetLocation(Consts.Sly).sceneName;
                /*VendorRando.vlog($"SlyKey current scene: {Finder.GetLocation(Consts.SlyKey).sceneName}");
                Dictionary<string, AbstractLocation> customLocations = typeof(Finder).GetField("CustomLocations", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<string, AbstractLocation>;
                if(customLocations.TryGetValue(Consts.SlyKey, out AbstractLocation loc)) {
                    loc.sceneName = Finder.GetLocation(vendorMap[Consts.Sly]).sceneName;
                }
                VendorRando.vlog($"SlyKey new scene: {Finder.GetLocation(Consts.SlyKey).sceneName}");*/

                rb.EditLocationRequest(myLocation, info => {
                    info.customPlacementFetch = (factory, placement) => {
                        if(factory.TryFetchPlacement(myLocation, out AbstractPlacement ap)) {
                            return ap;
                        }
                        var p = _placements.GetValue(factory) as Dictionary<string, AbstractPlacement>;
                        if(p.TryGetValue(myLocation, out AbstractPlacement placement1)) {
                            return placement1;
                        }
                        else {
                            if(p.ContainsKey(myLocation))
                                throw new ArgumentException($"Placement {myLocation} already exists!");
                            VendorPlacement vp = PlaceShopWithDefaults(Consts.Sly, myLocation, RandomizerMod.IC.Shops.GetDefaultShopItems(rb.gs), factory, "gaveSlykey");
                            return vp;
                        }
                    };
                    info.AddGetLocationDefModifier(myLocation, def => def with { FlexibleCount = true, AdditionalProgressionPenalty = true, SceneName = Finder.GetLocation(vendorMap[Consts.Sly]).sceneName });
                    info.onRandoLocationCreation += (factory, rl) => {
                        LogicManager lm = factory.lm;
                        rl.logic = lm.GetLogicDefStrict(baseLocation);
                        if(Data.TryGetCost(LocationNames.Sly_Key, out CostDef def)) {
                            rl.AddCost(new LogicGeoCost(lm, def.Amount));
                        }
                    };
                    info.onRandomizerFinish += placement => {
                        if(placement.Location is not RandoModLocation rl || placement.Item is not RandoModItem ri || rl.costs == null)
                            return;
                        foreach(LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>()) {
                            if(gc.GeoAmount < 0)
                                gc.GeoAmount = GetShopCost(rb.rng, ri);
                        }
                    };
                });
            }

            //original vendor location, non-vendor basic check
            void EditLocation_noLongerShopLocation(string shop, string vanillaShop)
            {
                LocationDef backupDef = new();
                if(rb.TryGetLocationDef(vanillaShop, out LocationDef tempDef))
                {
                    backupDef = tempDef;
                }
                rb.AddLocationByName(shop);
                rb.EditLocationRequest(shop, info =>
                {
                    info.customPlacementFetch = (factory, placement) =>
                    {
                        if(factory.TryFetchPlacement(shop, out AbstractPlacement ap))
                        {
                            return ap;
                        }
                        var p = _placements.GetValue(factory) as Dictionary<string, AbstractPlacement>;
                        if(p.TryGetValue(shop, out AbstractPlacement placement2))
                            return placement2;
                        else
                        {
                            if(p.ContainsKey(shop))
                                throw new ArgumentException($"Placement {shop} already exists!");
                            return ReplaceOldShops(shop, factory);
                        }
                    };
                    info.AddGetLocationDefModifier(shop, def => backupDef with { FlexibleCount = false, AdditionalProgressionPenalty = false });
                });
            }
        }

        public static void RemoveOldShops(RequestBuilder rb) {
            foreach((bool setting, string shop) in new (bool, string)[] {
                (VendorRando.Settings.Sly, LocationNames.Sly),
                (VendorRando.Settings.Salubra, LocationNames.Salubra),
                (VendorRando.Settings.Iselda, LocationNames.Iselda),
                (VendorRando.Settings.LegEater, LocationNames.Leg_Eater),
                (VendorRando.Settings.Lemm, LocationNames.Lemm)
            }) {
                if(setting) {
                    rb.RemoveLocationByName(shop);
                    if(shop == LocationNames.Salubra) {
                        rb.RemoveLocationByName("Salubra_(Requires_Charms)");
                    }
                    else if(shop == LocationNames.Sly) {
                        rb.RemoveLocationByName(LocationNames.Sly_Key);
                    }
                }
            }
        }

        public static void ApplyNewSalubraCharmDef(RequestBuilder rb) {
            (string normal, string withCharms) = VendorRando.Settings.Salubra ? (Consts.Salubra, Consts.SalubraCharms) : (LocationNames.Salubra, "Salubra_(Requires_Charms)");
            rb.EditLocationRequest(withCharms, info => {
                info.randoLocationCreator += factory => factory.MakeLocation(normal);
                info.onRandoLocationCreation += (factory, rl) => {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTermStrict("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                };
                info.customPlacementFetch = (factory, placement) => {
                    return factory.FetchOrMakePlacementWithEvents(normal, placement);
                };
                info.AddGetLocationDefModifier(withCharms, def => def with { FlexibleCount = true, AdditionalProgressionPenalty = true, SceneName = salubraLocation });
            });
        }

        private static VendorPlacement PlaceShopWithDefaults(string container, string location, DefaultShopItems allShopItems, ICFactory factory, string requiredBool = "") {
            string vanillaScene = container switch {
                Consts.Sly => SceneNames.Room_shop,
                Consts.Salubra => SceneNames.Room_Charm_Shop,
                Consts.Iselda => SceneNames.Room_mapper,
                Consts.LegEater => SceneNames.Fungus2_26,
                Consts.Lemm => SceneNames.Ruins1_05b,
                _ => null
            };
            DefaultShopItems? myItems = DefaultShopItems.None;
            for(int i = 0; i < 18; i++) {
                ShopItemStats stats = new() { specialType = i };//this is almost certainly incorrect
                if(!string.IsNullOrEmpty(requiredBool))
                    stats.requiredPlayerDataBool = requiredBool;
                DefaultShopItems? tempItem = ItemChanger.Util.ShopUtil.GetVanillaShopItemType(vanillaScene, stats);
                if(tempItem != null) {
                    myItems |= tempItem;
                }
            }
            myItems &= allShopItems;
            VendorPlacement vp = new(location) {
                Location = Finder.GetLocation(location),
                containerType = container
            };
            vp.AddTag<VendorTag>().defaultShopItems = myItems.GetValueOrDefault();
            if(!string.IsNullOrEmpty(requiredBool))
                vp.requiredPlayerDataBool = requiredBool;
            factory.AddPlacement(vp);
            return vp;
        }

        private static MutablePlacement ReplaceOldShops(string name, ICFactory factory) {
            AbstractPlacement ap = Finder.GetLocation(name).Wrap();
            factory.AddPlacement(ap);
            return (MutablePlacement)ap;
        }
    }
}
