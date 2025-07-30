using System;
using System.Collections.Generic;
using ItemChanger;

namespace VendorRando {
    public static class Consts {
        public const string Sly = "Vr_Sly";
        public const string Salubra = "Vr_Salubra";
        public const string Iselda = "Vr_Iselda";
        public const string LegEater = "Vr_Leg_Eater";
        public const string Lemm = "Vr_Lemm";

        public const string AccessSalubra = "Vr_Salubra_Access";
        public const string AccessIselda = "Vr_Iselda_Access";
        public const string AccessLeggy = "Vr_Leggy_Access";
        public const string AccessLemm = "Vr_Lemm_Access";
        public const string AccessSly = "Vr_Sly_Access";

        public static List<string> AccessNames = [AccessSly, AccessSalubra, AccessIselda, AccessLeggy, AccessLemm];

        public static Dictionary<string, string> RoomNames = new Dictionary<string, string> {
            { Consts.Sly, SceneNames.Room_shop },
            { Consts.Salubra, SceneNames.Room_Charm_Shop },
            { Consts.Iselda, SceneNames.Room_mapper },
            { Consts.LegEater, SceneNames.Fungus2_26 },
            { Consts.Lemm, SceneNames.Ruins1_05b }
        };

        public static readonly Func<bool, List<VendorData>> vendorData = includeKey => {
            List<VendorData> data = [
                new VendorData() { shop = Consts.Sly, vanillaShop = LocationNames.Sly, enabled = VendorRando.Settings.Sly, logic = LocationNames.Sly, access = Consts.AccessSly },
                new VendorData() { shop = Consts.Salubra, vanillaShop = LocationNames.Salubra, enabled = VendorRando.Settings.Salubra, logic = LocationNames.Salubra, access = Consts.AccessSalubra },
                new VendorData() { shop = Consts.Iselda, vanillaShop = LocationNames.Iselda, enabled = VendorRando.Settings.Iselda, logic = LocationNames.Iselda, access = Consts.AccessIselda },
                new VendorData() { shop = Consts.LegEater, vanillaShop = LocationNames.Leg_Eater, enabled = VendorRando.Settings.LegEater, logic = LocationNames.Leg_Eater, access = Consts.AccessLeggy },
                new VendorData() { shop = Consts.Lemm, vanillaShop = LocationNames.Lemm, enabled = VendorRando.Settings.Lemm, logic = "Can_Visit_Lemm", access = Consts.AccessLemm }
            ];
            if(includeKey) {
                data.Add(new VendorData() { shop = Consts.Sly, vanillaShop = LocationNames.Sly_Key, enabled = VendorRando.Settings.Sly, logic = LocationNames.Sly_Key, access = Consts.AccessSly + " + SHOPKEY" });
            }
            return data;
        };
    }

    public struct VendorData {
        public string vanillaShop;
        public string shop;
        public string access;
        public string logic;
        public bool enabled;
    }
}
