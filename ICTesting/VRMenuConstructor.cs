using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using ItemChanger.Placements;
using MenuChanger;
using MenuChanger.MenuElements;

namespace VendorRando {
    public class VRMenuConstructor: ModeMenuConstructor {

        public static StartDef startDef => new StartDef {
            SceneName = SceneNames.Tutorial_01,
            MapZone = 2,
            X = 35.5f,
            Y = 11.4f
        };
        
        public override void OnEnterMainMenu(MenuPage modeMenu) {}
        public override void OnExitMainMenu() {}

        public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button) {
            button = new BigButton(modeMenu, "VR-IC Test");
            button.OnClick += () => Start();
            return true;
        }

        private static void Start() {
            ItemChangerMod.CreateSettingsProfile(overwrite: false);
            ItemChangerMod.ChangeStartGame(startDef);
            ItemChangerMod.AddPlacements(GetPlacements(Container.DefineContainer<SlyContainer>().Name));
            MenuChangerMod.HideAllMenuPages();
            UIManager.instance.StartNewGame();
        }

        private static IEnumerable<AbstractPlacement> GetPlacements(string container) {
            AbstractLocation a = Finder.GetLocation(LocationNames.Geo_Rock_Kings_Pass_Left);
            AbstractPlacement ap = a.Wrap();
            ap.Items.Add(Finder.GetItem(ItemNames.Rancid_Egg));
            MutablePlacement mp = (MutablePlacement)ap;
            mp.containerType = container;
            
            return new List<AbstractPlacement>() { mp };
        }
    }
}
