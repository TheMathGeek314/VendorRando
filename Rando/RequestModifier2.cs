using System;
using System.Collections.Generic;
using System.Reflection;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Locations.SpecialLocations;
using ItemChanger.Placements;
using RandomizerCore.Randomization;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace VendorRando {
    public class RequestModifier2 {
        private static FieldInfo _placements;

        public static void HookRequestBuilder() {
            _placements = typeof(ICFactory).GetField("_placements", BindingFlags.Instance | BindingFlags.NonPublic);
            
            RequestBuilder.OnUpdate.Subscribe(-100, ApplyHutDefs);
            RequestBuilder.OnUpdate.Subscribe(-99, NabShopPlacements);
            RequestBuilder.OnUpdate.Subscribe(-499, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(101, RestrictPlacements);
        }

        public static void ApplyHutDefs(RequestBuilder rb) {
            foreach((bool setting, string shop, string vanillaLocation) in new (bool, string, string)[] {
                (VendorRando.Settings.Sly, Consts.Sly, LocationNames.Sly),
                (VendorRando.Settings.Salubra, Consts.Salubra, LocationNames.Salubra),
                (VendorRando.Settings.Iselda, Consts.Iselda, LocationNames.Iselda),
                (VendorRando.Settings.LegEater, Consts.LegEater, LocationNames.Leg_Eater),
                (VendorRando.Settings.Lemm, Consts.Lemm, LocationNames.Lemm)
            }) {
                if(setting) {
                    LocationDef backupDef = new();
                    if(rb.TryGetLocationDef(vanillaLocation, out LocationDef tempDef)) {
                        backupDef = tempDef with { FlexibleCount = false, AdditionalProgressionPenalty = false, Name = shop };
                    }
                    else {
                        VendorRando.vlog($"Could not get LocationDef [{vanillaLocation}]");
                    }
                    rb.AddLocationByName(shop);
                    rb.EditLocationRequest(shop, info => {
                        info.customPlacementFetch = (factory, placement) => {
                            if(factory.TryFetchPlacement(shop, out AbstractPlacement ap))
                                return ap;
                            var p = _placements.GetValue(factory) as Dictionary<string, AbstractPlacement>;
                            if(p.TryGetValue(shop, out AbstractPlacement placement2))
                                return placement2;
                            else {
                                if(p.ContainsKey(shop))
                                    throw new ArgumentException($"Placement {shop} already exists!");
                                AbstractLocation al = Finder.GetLocation(shop);
                                al.flingType = FlingType.StraightUp;
                                AbstractPlacement mp = al.Wrap();
                                factory.AddPlacement(mp);
                                return (MutablePlacement)mp;
                            }
                        };
                        info.AddGetLocationDefModifier(shop, def => backupDef);
                        /*info.getLocationDef = () => new() {
                            Name = shop,
                            FlexibleCount = false,
                            AdditionalProgressionPenalty = false,
                            SceneName = Finder.GetLocation(vanillaLocation).sceneName
                        };*/
                    });
                }
            }
        }

        public static void NabShopPlacements(RequestBuilder rb) {
            foreach(string shop in new string[] { LocationNames.Sly, LocationNames.Salubra, LocationNames.Iselda, LocationNames.Leg_Eater, LocationNames.Lemm }) {
                rb.EditLocationRequest(shop, info => {
                    info.customAddToPlacement += (factory, rp, ap, item) => {
                        string vanillaScene = "Town";
                        switch(shop) {
                            case LocationNames.Sly:
                                SlyContainer.VanillaShopPlacement = ap;
                                vanillaScene = SceneNames.Room_shop;
                                break;
                            case LocationNames.Salubra:
                                SalubraContainer.VanillaShopPlacement = ap;
                                vanillaScene = SceneNames.Room_Charm_Shop;
                                break;
                            case LocationNames.Iselda:
                                IseldaContainer.VanillaShopPlacement = ap;
                                vanillaScene = SceneNames.Room_mapper;
                                break;
                            case LocationNames.Leg_Eater:
                                LeggyContainer.VanillaShopPlacement = ap;
                                vanillaScene = SceneNames.Fungus2_26;
                                break;
                            case LocationNames.Lemm:
                                LemmContainer.VanillaShopPlacement = ap;
                                vanillaScene = SceneNames.Ruins1_05b;
                                break;
                        }
                        if(!ap.HasTag<VendorTag>()) {
                            DefaultShopItems? myItems = DefaultShopItems.None;
                            for(int i = 0; i < 18; i++) {
                                ShopItemStats stats = new() { specialType = i };//maybe adjust this or add requiredBool
                                DefaultShopItems? tempItem = ItemChanger.Util.ShopUtil.GetVanillaShopItemType(vanillaScene, stats);
                                if(tempItem != null)
                                    myItems |= tempItem;
                            }
                            myItems &= RandomizerMod.IC.Shops.GetDefaultShopItems(rb.gs);
                            ap.AddTag<VendorTag>().defaultShopItems = myItems.GetValueOrDefault();
                        }
                    };
                });
            }
        }

        private static void SetupItems(RequestBuilder rb) {
            if(!VendorRando.Settings.Any)
                return;
            foreach((bool setting, string accessItem) in new (bool, string)[] {
                (VendorRando.Settings.Sly, Consts.AccessSly),
                (VendorRando.Settings.Salubra, Consts.AccessSalubra),
                (VendorRando.Settings.Iselda, Consts.AccessIselda),
                (VendorRando.Settings.LegEater, Consts.AccessLeggy),
                (VendorRando.Settings.Lemm, Consts.AccessLemm)
            }) {
                rb.EditItemRequest(accessItem, info => {
                    info.getItemDef = () => new ItemDef() {
                        Name = accessItem,
                        Pool = "GeoRocks",
                        MajorItem = false,
                        PriceCap = 1
                    };
                });
                if(setting) {
                    rb.AddItemByName(accessItem);
                }
            }
        }

        private static void RestrictPlacements(RequestBuilder rb) {
            if(!VendorRando.Settings.Any)
                return;
            List<string> mutables = new();
            foreach(var pool in Data.Pools)
                if(pool.IsIncluded(rb.gs))
                    foreach(string location in pool.IncludeLocations)
                        if(IsValidLocation(location))
                            mutables.Add(location);
            foreach(ItemGroupBuilder igb in rb.EnumerateItemGroups()) {
                if(igb.strategy is DefaultGroupPlacementStrategy dgps) {
                    List<string> accessNames = new List<string>([Consts.AccessSly, Consts.AccessSalubra, Consts.AccessIselda, Consts.AccessLeggy, Consts.AccessLemm]);
                    dgps.ConstraintList.Add(new DefaultGroupPlacementStrategy.Constraint(
                        (item, location) => !(accessNames.Contains(item.Name) && !mutables.Contains(location.Name)),
                        Label: "Vendor Placement"
                    ));
                }
            }

            bool IsValidLocation(string location) {
                AbstractLocation loc = Finder.GetLocation(location);
                var mp = loc.Wrap();
                if(mp is MutablePlacement) {
                    Type type = loc.GetType();
                    if(type == typeof(CoordinateLocation)) {
                        if(loc.sceneName == SceneNames.RestingGrounds_07 || loc.sceneName == SceneNames.Crossroads_38 || location == LocationNames.Grimmchild)
                            return false;
                        return true;
                    }
                    if(type == typeof(ObjectLocation))
                        return true;
                    if(type == typeof(StagLocation))
                        return true;
                }
                return false;
            }
        }
    }
}
