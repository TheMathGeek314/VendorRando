using Modding;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MWC = MultiWorldMod.Randomizer.MultiWorldController;

namespace VendorRando {
    internal static class CompatibilityChecks {
        private static bool ccWarned = false;

        public static void Run() {
            if(!VendorRando.globalSettings.Any)
                return;
            if(ModHooks.GetMod("RandoPlus") is Mod) {
                CheckRandoPlus();
            }
            if(ModHooks.GetMod("BugPrince") is Mod) {
                CheckBugPrince();
            }
            if(ModHooks.GetMod("ContainerConfig") is Mod) {
                if(!ccWarned) {
                    ccWarned = true;
                    throw new VendorCompatibilityWarning();
                }
            }
        }

        private static void CheckRandoPlus() {
            if(RandoPlus.RandoPlus.GS.FullFlexibleCount) {
                throw new VendorCompatibilityException("Full Flexible Count", "RandoPlus", false);
            }
            if(RandoPlus.RandoPlus.GS.AreaBlitz) {
                throw new VendorCompatibilityException("Area Blitz", "RandoPlus", false);
            }
        }

        private static void CheckBugPrince() {
            if(BugPrince.BugPrinceMod.RS.MapShop && VendorRando.globalSettings.Iselda) {
                throw new VendorCompatibilityException("Map Shop", "BugPrince", true, "Iselda");
            }
        }

        public static void PatchMultiWorld() {
            MethodInfo methodToHook = typeof(MWC).GetMethod("InitialMultiSetup", BindingFlags.Public | BindingFlags.Instance);
            new Hook(methodToHook, (Action<MWC> orig, MWC self) => {
                if(VendorRando.globalSettings.Any)
                    throw new VendorMultiWorldException();
                orig(self);
            });
        }
    }

    public class VendorCompatibilityException: Exception {
        string setting;
        string source;
        bool individual;
        string vendor;

        public VendorCompatibilityException(string setting, string source, bool individual, string vendor = "Iselda") {
            this.setting = setting;
            this.source = source;
            this.individual = individual;
            if(individual)
                this.vendor = vendor;
        }

        public override string Message => $"{setting} from {source} is not compatible with " + (individual ? vendor + " from VendorRando" : "VendorRando");
        public override string ToString() => Message;
    }

    public class VendorCompatibilityWarning: Exception {
        public override string Message => "ContainerConfig will break VendorRando locations! (Trying again will ignore this warning)";
        public override string ToString() => Message;
    }

    public class VendorMultiWorldException: Exception {
        public override string Message => "VendorRando cannot be used in MultiWorld";
        public override string ToString() => Message;
    }
}
