using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.IO;

namespace VendorRando {
    public static class LogicAdder {
        
        public static void Hook() {
            RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            if(!VendorRando.Settings.Any)
                return;
            JsonLogicFormat fmt = new();
            using Stream s = typeof(LogicAdder).Assembly.GetManifestResourceStream("VendorRando.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);
        }
    }
}
