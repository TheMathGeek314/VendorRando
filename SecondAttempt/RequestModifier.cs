using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ItemChanger;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace VendorRando {
    public static class RequestModifier {
        public static void Hook() {
            RequestBuilder.OnUpdate.Subscribe(-499, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(0.4f, AddVendors);
        }

        private static void SetupRefs(RequestBuilder rb) {
            foreach(string vendor in Consts.ToArray()) {
                rb.EditItemRequest(vendor, info => {
                    info.getItemDef = () => new() {
                        Name = vendor,
                        Pool = Consts.VendorPoolGroup,
                        MajorItem = false,
                        PriceCap = 500//should this be -1?
                    };
                });
                rb.EditLocationRequest(vendor, info => {
                    info.getLocationDef = () => new() {
                        Name = vendor,
                        SceneName = "Town",//Finder.GetLocation(vendor).sceneName,
                        FlexibleCount = false,
                        AdditionalProgressionPenalty = false
                    };
                });
            }
            if(rb.gs.SplitGroupSettings.RandomizeOnStart && VendorRando.Settings.VendorGroup >= 0 && VendorRando.Settings.VendorGroup <= 2) {
                VendorRando.Settings.VendorGroup = rb.rng.Next(3);
            }
            if(VendorRando.Settings.VendorGroup > 0) {
                ItemGroupBuilder vendorGroup = null;
                string label = RBConsts.SplitGroupPrefix + VendorRando.Settings.VendorGroup;
                foreach(ItemGroupBuilder igb in rb.EnumerateItemGroups()) {
                    if(igb.label == label) {
                        vendorGroup = igb;
                        break;
                    }
                }
                vendorGroup ??= rb.MainItemStage.AddItemGroup(label);

                rb.OnGetGroupFor.Subscribe(0.01f, ResolveVendorGroup);

                bool ResolveVendorGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb) {
                    if(type == RequestBuilder.ElementType.Transition) {
                        gb = default;
                        return false;
                    }
                    if(!Consts.ToArray().Contains(item)) {
                        gb = default;
                        return false;
                    }
                    gb = vendorGroup;
                    return true;
                }
            }
        }

        private static void AddVendors(RequestBuilder rb) {
            foreach((bool property, string name) in new (bool, string)[] {
                (VendorRando.Settings.EnableSly, Consts.SlyShop),
                (VendorRando.Settings.EnableSalubra, Consts.SalubraShop),
                (VendorRando.Settings.EnableIselda, Consts.IseldaShop),
                (VendorRando.Settings.EnableLeggy, Consts.LeggyShop),
                (VendorRando.Settings.EnableLemm, Consts.LemmShop)
            }) {
                if(property) {
                    rb.AddItemByName(name, 1);
                    rb.AddLocationByName(name);
                }
            }
        }
    }
}
