using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace VendorRando {
    internal static class RSMInterop {
        public static void Hook() {
            RandoSettingsManagerMod.Instance.RegisterConnection(new VendorRandoSettingsProxy());
        }
    }

    internal class VendorRandoSettingsProxy: RandoSettingsProxy<GlobalSettings, string> {
        public override string ModKey => VendorRando.instance.GetName();
        
        public override VersioningPolicy<string> VersioningPolicy { get; } = new EqualityVersioningPolicy<string>(VendorRando.instance.GetVersion());

        public override void ReceiveSettings(GlobalSettings settings) {
            settings ??= new();
            RandoMenuPage.Instance.vrMEF.SetMenuValues(settings);
        }

        public override bool TryProvideSettings(out GlobalSettings settings) {
            settings = VendorRando.Settings;
            return settings.Any;
        }
    }
}
