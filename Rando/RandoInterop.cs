using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;

namespace VendorRando {
    internal static class RandoInterop {
        public static void HookRandomizer() {
            RandoMenuPage.Hook();
            RequestModifier2.HookRequestBuilder();
            LogicAdder.Hook();

            Container.DefineContainer<SlyContainer>();
            Container.DefineContainer<SalubraContainer>();
            Container.DefineContainer<IseldaContainer>();
            Container.DefineContainer<LeggyContainer>();
            Container.DefineContainer<LemmContainer>();
            DefineLocations();
            DefineItems();
        }

        public static void DefineLocations() {
            static void DefineLoc(string name, string sceneName, string objectName, string sprite, float x, float y, string altSceneName = "") {
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
                tag.Properties["WorldMapLocations"] = new (string, float, float)[] {
                    (string.IsNullOrEmpty(altSceneName) ? sceneName : altSceneName, x, y)
                };
                tag.Properties["PinSprite"] = new EmbeddedSprite(sprite);

                Finder.DefineCustomLocation(objLocation);
            }

            DefineLoc(Consts.Sly, SceneNames.Room_shop, "Sly Shop", "pin_shop_sly", 118f, 35f, SceneNames.Town);
            DefineLoc(Consts.Salubra, SceneNames.Room_Charm_Shop, "Charm Slug", "pin_charm_slug", 140f, 17f, SceneNames.Crossroads_04);
            DefineLoc(Consts.Iselda, SceneNames.Room_mapper, "Iselda", "pin_shop_mapper", 155f, 35f, SceneNames.Town);
            DefineLoc(Consts.LegEater, SceneNames.Fungus2_26, "Leg Eater", "pin_shop_leg_eater", 55f, 8f);
            DefineLoc(Consts.Lemm, SceneNames.Ruins1_05b, "Relic Dealer", "pin_shop_relic_dealer", 53.3077f, 24.99f);
        }

        public static void DefineItems() {
            foreach((string name, string accessName, string sprite) in new (string, string, string)[] {
                (Consts.Sly, Consts.AccessSly, "pin_shop_sly"),
                (Consts.Salubra, Consts.AccessSalubra, "pin_charm_slug"),
                (Consts.Iselda, Consts.AccessIselda, "pin_shop_mapper"),
                (Consts.LegEater, Consts.AccessLeggy, "pin_shop_leg_eater"),
                (Consts.Lemm, Consts.AccessLemm, "pin_shop_relic_dealer")
            }) {
                VendorItem vendorItem = new(name) { name = accessName };
                InteropTag tag = vendorItem.AddTag<InteropTag>();
                tag.Message = ConnectionMetadataInjector.SupplementalMetadata.InteropTagMessage;
                tag.Properties["ModSource"] = VendorRando.instance.GetName();
                vendorItem.UIDef = new MsgUIDef {
                    name = new BoxedString(name),
                    shopDesc = new BoxedString("You should never see this"),
                    sprite = new EmbeddedSprite(sprite)
                };
                Finder.DefineCustomItem(vendorItem);
            }
        }
    }
}
