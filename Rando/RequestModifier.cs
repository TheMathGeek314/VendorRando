using System;
using System.Collections.Generic;
using System.Reflection;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Locations.SpecialLocations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using RandomizerCore.Exceptions;
using RandomizerCore.Randomization;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace VendorRando {
    public class RequestModifier {
        private static FieldInfo _placements;

        public static void HookRequestBuilder() {
            _placements = typeof(ICFactory).GetField("_placements", BindingFlags.Instance | BindingFlags.NonPublic);

            RequestBuilder.OnUpdate.Subscribe(-100, ApplyHutDefs);
            RequestBuilder.OnUpdate.Subscribe(-499, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(101, RestrictPlacements);
            RequestBuilder.OnUpdate.Subscribe(-99, EditShopPins);
        }

        public static void ApplyHutDefs(RequestBuilder rb) {
            foreach(VendorData data in Consts.vendorData(false)) {
                if(data.enabled) {
                    rb.AddLocationByName(data.shop);
                    rb.EditLocationRequest(data.shop, info => {
                        info.customPlacementFetch = (factory, placement) => {
                            if(factory.TryFetchPlacement(data.shop, out AbstractPlacement ap))
                                return ap;
                            var p = _placements.GetValue(factory) as Dictionary<string, AbstractPlacement>;
                            if(p.TryGetValue(data.shop, out AbstractPlacement placement2))
                                return placement2;
                            else {
                                if(p.ContainsKey(data.shop))
                                    throw new ArgumentException($"Placement {data.shop} already exists!");
                                AbstractLocation al = Finder.GetLocation(data.shop);
                                al.flingType = FlingType.StraightUp;
                                MutablePlacement mp = al.Wrap() as MutablePlacement;
                                factory.AddPlacement(mp);
                                return mp;
                            }
                        };
                        info.getLocationDef = () => new() {
                            Name = data.shop,
                            FlexibleCount = false,
                            AdditionalProgressionPenalty = false,
                            SceneName = Finder.GetLocation(data.vanillaShop).sceneName
                        };
                    });
                }
            }
        }

        private static void SetupItems(RequestBuilder rb) {
            if(!VendorRando.Settings.Any)
                return;
            foreach(VendorData data in Consts.vendorData(false)) {
                rb.EditItemRequest(data.access, info => {
                    info.getItemDef = () => new ItemDef() {
                        Name = data.access,
                        MajorItem = false,
                        PriceCap = 1
                    };
                });
                if(data.enabled) {
                    rb.AddItemByName(data.access);
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
            foreach(VendorData data in Consts.vendorData(false)) {
                if(data.enabled)
                    mutables.Add(data.shop);
            }
            foreach(ItemGroupBuilder igb in rb.EnumerateItemGroups()) {
                if(igb.strategy is DefaultGroupPlacementStrategy dgps) {
                    dgps.ConstraintList.Add(new DefaultGroupPlacementStrategy.Constraint(
                        (item, location) => !(Consts.AccessNames.Contains(item.Name) && !mutables.Contains(location.Name)),
                        Label: "Vendor Placement",
                        Fail: (item, location) => {
                            throw new OutOfLocationsException();
                        }
                    ));
                }
            }

            bool IsValidLocation(string location) {
                AbstractLocation loc = Finder.GetLocation(location);
                Type type = loc.GetType();
                if(type == typeof(CoordinateLocation)) {
                    if(loc.sceneName == SceneNames.RestingGrounds_07 || loc.sceneName == SceneNames.Crossroads_38 || location == LocationNames.Grimmchild || location == LocationNames.Stag_Nest_Stag)
                        return false;
                    return true;
                }
                if(type == typeof(ObjectLocation)) {
                    if(location == LocationNames.Resting_Grounds_Map || location == LocationNames.Elevator_Pass)
                        return false;
                    return true;
                }
                if(loc is ExistingFsmContainerLocation efcLoc && loc is not DivineLocation) {
                    if(efcLoc.containerType != "Shiny")
                        return false;
                    if(location == LocationNames.Flukenest || location == LocationNames.City_Crest || location == LocationNames.Lifeblood_Core || location.StartsWith("Charm_Notch-") || location.StartsWith("Rancid_Egg-"))
                        return false;
                    return true;
                }
                return false;
            }
        }

        private static void EditShopPins(RequestBuilder rb) {
            if(!VendorRando.Settings.Any)
                return;
            int horizontal = 0;
            int pinSize = 25;
            foreach(VendorData data in Consts.vendorData(true)) {
                if(data.enabled) {
                    if(data.vanillaShop != LocationNames.Sly_Key)
                        horizontal++;
                    int copy = horizontal;
                    rb.EditLocationRequest(data.vanillaShop, info => {
                        info.onPlacementFetch += (factory, randoPlacement, placement) => {
                            ShopPlacement shop = placement as ShopPlacement;
                            InteropTag tag = RandoInterop.AddTag(shop.Location);
                            (string, float, float) wmLocation = (SceneNames.Ruins1_28, copy * pinSize + 15, 115 + (data.vanillaShop == LocationNames.Sly_Key ? pinSize : 0));
                            tag.Properties["WorldMapLocation"] = wmLocation;
                        };
                    });
                }
            }
        }
    }
}
