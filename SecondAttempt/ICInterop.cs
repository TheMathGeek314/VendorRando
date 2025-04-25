using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using ItemChanger.Items;
using ItemChanger.UIDefs;

namespace VendorRando {
    public static class ICInterop {
        public static void DefineItemsAndLocations() {
            void DefineItem(string name) {
                VoidItem vendorItem = new() {
                    name = name,
                    UIDef = new MsgUIDef() {
                        name = new BoxedString(name),
                        shopDesc = new BoxedString("If you see this, something is broken"),
                        sprite = null
                    }
                };
                SupplementalMetadataTagFactory.AddTagToItem(vendorItem, poolGroup: Consts.VendorPoolGroup);
                Finder.DefineCustomItem(vendorItem);
            }
            
            void DefineLocation(string name, string vItem/*, string scene, string objectName, int level*/) {
                VendorLocation vendLocation = new() {
                    vendorName = name
                };
                SupplementalMetadataTagFactory.AddTagToLocation(vendLocation, poolGroup: Consts.VendorPoolGroup, vanillaItem: vItem);
                Finder.DefineCustomLocation(vendLocation);
            }

            DefineItem(Consts.SlyShop);
            DefineItem(Consts.SalubraShop);
            DefineItem(Consts.IseldaShop);
            DefineItem(Consts.LeggyShop);
            DefineItem(Consts.LemmShop);

            DefineLocation("Sly", Consts.SlyShop);
            DefineLocation("Salubra", Consts.SalubraShop);
            DefineLocation("Iselda", Consts.IseldaShop);
            DefineLocation("Leggy", Consts.LeggyShop);
            DefineLocation("Lemm", Consts.LemmShop);
        }
    }
}
