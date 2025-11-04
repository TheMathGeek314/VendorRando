using System.IO;
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
            if(!VendorRando.globalSettings.Any)
                return;
            JsonLogicFormat fmt = new();
            using Stream s = typeof(LogicAdder).Assembly.GetManifestResourceStream("VendorRando.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);

            DefineTermsAndItems(lmb, fmt);
        }

        private static void DefineTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt) {
            using Stream t = typeof(LogicAdder).Assembly.GetManifestResourceStream("VendorRando.Resources.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            foreach(string accessName in Consts.AccessNames) {
                lmb.AddItem(new SingleItem(accessName, new TermValue(lmb.GetTerm(accessName), 1)));
            }
        }

        private static void PatchLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            foreach(VendorData data in Consts.vendorData(true)) {
                if(data.enabled) {
                    lmb.DoLogicEdit(new(data.logic, data.access));
                }
            }

            if(VendorRando.globalSettings.LegEater)
                lmb.DoSubst(new("Can_Repair_Fragile_Charms", "Fungus2_26[left1]", Consts.AccessLeggy));
        }
    }
}
