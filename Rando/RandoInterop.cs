using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;

namespace VendorRando {
    internal static class RandoInterop {
        public static void HookRandomizer() {
            RandoMenuPage.Hook();
            RequestModifier.HookRequestBuilder();
            LogicAdder.Hook();

            Container.DefineContainer<SlyContainer>();
            Container.DefineContainer<SalubraContainer>();
            Container.DefineContainer<IseldaContainer>();
            Container.DefineContainer<LeggyContainer>();
            Container.DefineContainer<LemmContainer>();
            DefineLocations();
        }

        public static void DefineLocations() {
            static void DefineLoc(string name, string sceneName, string objectName, string sprite, float x, float y, string altSceneName = "", bool keyDontDoStuff = false) {
                ObjectLocation objLocation = new() {
                    name = name,
                    objectName = objectName,
                    sceneName = sceneName,
                    forceShiny = false,
                    flingType = FlingType.DirectDeposit
                };

                InteropTag tag = objLocation.AddTag<InteropTag>();
                tag.Message = ConnectionMetadataInjector.SupplementalMetadata.InteropTagMessage;
                tag.Properties["ModSource"] = VendorRando.instance.GetName();
                if(!keyDontDoStuff) {
                    tag.Properties["WorldMapLocations"] = new (string, float, float)[] {
                        (string.IsNullOrEmpty(altSceneName) ? sceneName : altSceneName, x, y)
                    };
                    tag.Properties["PinSprite"] = new EmbeddedSprite(sprite);
                }

                Finder.DefineCustomLocation(objLocation);
            }

            DefineLoc(Consts.Sly, SceneNames.Room_shop, "Sly Shop", "pin_shop_sly", 118f, 35f, SceneNames.Town);
            DefineLoc(Consts.Salubra, SceneNames.Room_Charm_Shop, "Charm Slug", "pin_charm_slug", 140f, 17f, SceneNames.Crossroads_04);
            DefineLoc(Consts.Iselda, SceneNames.Room_mapper, "Iselda", "pin_shop_mapper", 155f, 35f, SceneNames.Town);
            DefineLoc(Consts.LegEater, SceneNames.Fungus2_26, "Leg Eater", "pin_shop_leg_eater", 55f, 8f);
            DefineLoc(Consts.Lemm, SceneNames.Ruins1_05b, "Relic Dealer", "pin_shop_relic_dealer", 53.3077f, 24.99f);
            DefineLoc(Consts.SlyKey, SceneNames.Town, Consts.Sly, "", 0, 0, "", true);
        }
    }
}
