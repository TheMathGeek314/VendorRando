using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace VendorRando {
    public class RandoMenuPage {
        internal MenuPage VendorRandoPage;
        internal MenuElementFactory<GlobalSettings> vrMEF;
        internal VerticalItemPanel vrVIP;

        internal SmallButton JumpToVRButton;

        internal static RandoMenuPage Instance { get; private set; }

        public static void OnExitMenu() {
            Instance = null;
        }

        public static void Hook() {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button) {
            button = Instance.JumpToVRButton;
            return true;
        }

        private void SetTopLevelButtonColor() {
            if(JumpToVRButton != null) {
                JumpToVRButton.Text.color = VendorRando.globalSettings.Any ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private RandoMenuPage(MenuPage landingPage) {
            VendorRandoPage = new MenuPage(Localize("VendorRando"), landingPage);
            vrMEF = new(VendorRandoPage, VendorRando.globalSettings);
            vrVIP = new(VendorRandoPage, new(0, 300), 75f, true, vrMEF.Elements);
            Localize(vrMEF);
            foreach(IValueElement e in vrMEF.Elements) {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            JumpToVRButton = new(landingPage, Localize("VendorRando"));
            JumpToVRButton.AddHideAndShowEvent(landingPage, VendorRandoPage);
            SetTopLevelButtonColor();
        }
    }
}
