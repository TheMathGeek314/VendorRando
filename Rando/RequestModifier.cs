﻿using System;
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
            foreach((bool setting, string shop, string vanillaLocation) in new (bool, string, string)[] {
                (VendorRando.Settings.Sly, Consts.Sly, LocationNames.Sly),
                (VendorRando.Settings.Salubra, Consts.Salubra, LocationNames.Salubra),
                (VendorRando.Settings.Iselda, Consts.Iselda, LocationNames.Iselda),
                (VendorRando.Settings.LegEater, Consts.LegEater, LocationNames.Leg_Eater),
                (VendorRando.Settings.Lemm, Consts.Lemm, LocationNames.Lemm)
            }) {
                if(setting) {
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
                                MutablePlacement mp = al.Wrap() as MutablePlacement;
                                factory.AddPlacement(mp);
                                return mp;
                            }
                        };
                        info.getLocationDef = () => new() {
                            Name = shop,
                            FlexibleCount = false,
                            AdditionalProgressionPenalty = false,
                            SceneName = Finder.GetLocation(vanillaLocation).sceneName
                        };
                    });
                }
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
            foreach((bool setting, string vloc) in new (bool, string)[] {
                (VendorRando.Settings.Sly, Consts.Sly),
                (VendorRando.Settings.Salubra, Consts.Salubra),
                (VendorRando.Settings.Iselda, Consts.Iselda),
                (VendorRando.Settings.LegEater, Consts.LegEater),
                (VendorRando.Settings.Lemm, Consts.Lemm)
            }) {
                if(setting)
                    mutables.Add(vloc);
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
                var mp = loc.Wrap();
                if(mp is not MutablePlacement)
                    return false;
                if(loc is DualLocation or AutoLocation or StagLocation or ShopLocation or ExistingFsmContainerLocation)
                    return false;
                if(type == typeof(CoordinateLocation)) {
                    if(loc.sceneName == SceneNames.RestingGrounds_07 || loc.sceneName == SceneNames.Crossroads_38 || location == LocationNames.Grimmchild || location == LocationNames.Stag_Nest_Stag)
                        return false;
                    return true;
                }
                if(type == typeof(ObjectLocation)) {
                    if(location == LocationNames.Resting_Grounds_Map)
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
            foreach((bool setting, string vanillaShop) in new (bool, string)[] {
                (VendorRando.Settings.Sly, LocationNames.Sly),
                (VendorRando.Settings.Sly, LocationNames.Sly_Key),
                (VendorRando.Settings.Salubra, LocationNames.Salubra),
                (VendorRando.Settings.Iselda, LocationNames.Iselda),
                (VendorRando.Settings.LegEater, LocationNames.Leg_Eater),
                (VendorRando.Settings.Lemm, LocationNames.Lemm)
            }) {
                if(setting) {
                    if(vanillaShop != LocationNames.Sly_Key)
                        horizontal++;
                    int copy = horizontal;
                    rb.EditLocationRequest(vanillaShop, info => {
                        info.onPlacementFetch += (factory, randoPlacement, placement) => {
                            ShopPlacement shop = placement as ShopPlacement;
                            InteropTag tag = RandoInterop.AddTag(shop.Location);
                            (string, float, float) wmLocation = (SceneNames.Ruins1_28, copy * pinSize + 15, 115 + (vanillaShop == LocationNames.Sly_Key ? pinSize : 0));
                            tag.Properties["WorldMapLocation"] = wmLocation;
                        };
                    });
                }
            }
        }
    }
}
