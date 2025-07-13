using System.IO;
using ItemChanger;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace VendorRando {
    public static class LogicAdder {
        
        public static void Hook() {
            RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(50, PatchLogic);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            if(!VendorRando.Settings.Any)
                return;
            JsonLogicFormat fmt = new();
            using Stream s = typeof(LogicAdder).Assembly.GetManifestResourceStream("VendorRando.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);

            DefineTermsAndItems(lmb, fmt);
        }

        private static void DefineTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt) {
            using Stream t = typeof(LogicAdder).Assembly.GetManifestResourceStream("VendorRando.Resources.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            foreach(string accessName in new string[] { Consts.AccessSly, Consts.AccessSalubra, Consts.AccessIselda, Consts.AccessLeggy, Consts.AccessLemm }) {
                lmb.AddItem(new SingleItem(accessName, new TermValue(lmb.GetTerm(accessName), 1)));
            }
        }

        private static void PatchLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            foreach((bool setting, string location, string access) in new (bool, string, string)[] {
                (VendorRando.Settings.Sly, LocationNames.Sly, Consts.AccessSly),
                (VendorRando.Settings.Sly, LocationNames.Sly_Key, Consts.AccessSly + " + SHOPKEY"),
                (VendorRando.Settings.Salubra, LocationNames.Salubra, Consts.AccessSalubra),
                (VendorRando.Settings.Iselda, LocationNames.Iselda, Consts.AccessIselda),
                (VendorRando.Settings.LegEater, LocationNames.Leg_Eater, Consts.AccessLeggy),
                (VendorRando.Settings.Lemm, "Can_Visit_Lemm", Consts.AccessLemm)
            }) {
                if(setting) {
                    lmb.DoLogicEdit(new(location, access));
                }
            }
        }
    }
}
