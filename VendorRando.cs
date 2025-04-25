using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Satchel;
using ItemChanger;
using MenuChanger;

namespace VendorRando {
    public class VendorRando: Mod, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "VendorRando";
        public override string GetVersion() => "1.0.0.0";

        public static GlobalSettings Settings { get; set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => Settings = s;
        public GlobalSettings OnSaveGlobal() => Settings;

        //static npcObjects slyObj, salubraObj;//, iseldaObj, leggyObj, lemmObj;

        internal static VendorRando instance;

        public VendorRando() : base(null) {
            instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            //TemporaryHookThingy();
            definePrefabs(preloadedObjects);
            On.GameManager.OnNextLevelReady += onSceneChange;
            ModeMenu.AddMode(new VRMenuConstructor());
        }

        /*public static void TemporaryHookThingy() {
            LogicAdder.Hook();
            RequestModifier.Hook();
        }*/

        public override List<(string, string)> GetPreloadNames() {
            return new List<(string, string)> {
                ("Room_shop", "Basement Closed"),
                ("Room_shop", "_Scenery/Shop Counter"),
                ("Room_shop", "Shop Menu"),
                ("Room_Charm_Shop", "Charm Slug"),
                ("Room_Charm_Shop", "shop_0000_a"),
                ("Room_Charm_Shop", "Shop Region"),
                ("Room_Charm_Shop", "Shop Menu"),
                ("Room_mapper", "Iselda"),
                ("Room_mapper", "_Scenery/Mapper_home_0001_a"),
                ("Room_mapper", "Shop Region"),
                ("Room_mapper", "Shop Menu"),
                ("Fungus2_26", "Leg Eater"),
                ("Fungus2_26", "leg_eater_scenery_0004_a"),
                ("Fungus2_26", "Shop Region"),
                ("Fungus2_26", "Shop Menu"),
                ("Ruins1_05b", "Relic Dealer"),
                ("Ruins1_05b", "antique_shop/antique_r_0007_a"),
                ("Ruins1_05b", "antique_shop/antique_r_0006_a"),
                ("Ruins1_05b", "Shop Region"),
                ("Ruins1_05b", "Shop Menu")
            };
        }

        private void definePrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            SlyContainer.definePrefabs(preloadedObjects["Room_shop"]);
            SalubraContainer.definePrefabs(preloadedObjects["Room_Charm_Shop"]);
            IseldaContainer.definePrefabs(preloadedObjects["Room_mapper"]);
            LeggyContainer.definePrefabs(preloadedObjects["Fungus2_26"]);
            LemmContainer.definePrefabs(preloadedObjects["Ruins1_05b"]);
        }

        private void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self) {
            orig(self);
            //only run this if running a rando aka not vanilla
            switch(self.sceneName) {
                case "Room_shop":
                    if(Settings.EnableSly) {
                        foreach(string go in new string[] { "Basement Closed", "_Scenery/Shop Counter" }) {
                            GameObject.Find(go).SetActive(false);
                        }
                    }
                    break;
                case "Room_Charm_Shop":
                    if(Settings.EnableSalubra) {
                        foreach(string go in new string[] { "Charm Slug", "shop_0000_a", "Shop Region" }) {
                            GameObject.Find(go).SetActive(false);
                        }
                    }
                    break;
                case "Room_mapper":
                    if(Settings.EnableIselda) {
                        foreach(string go in new string[] { "Iselda", "_Scenery/Mapper_home_0001_a", "Shop Region" }) {
                            GameObject.Find(go).SetActive(false);
                        }
                    }
                    break;
                case "Fungus2_26":
                    if(Settings.EnableLeggy) {
                        foreach(string go in new string[] { "Leg Eater", "leg_eater_scenery_0004_a", "Shop Region" }) {
                            GameObject.Find(go).SetActive(false);
                        }
                    }
                    break;
                case "Ruins1_05b":
                    if(Settings.EnableLemm) {
                        foreach(string go in new string[] { "Relic Dealer", "antique_shop/antique_r_0007_a", "antique_shop/antique_r_0006_a", "antique_shop/ruins_clutter_0010_a", "Shop Region" }) {
                            GameObject.Find(go).SetActive(false);
                        }
                    }
                    break;
            }
        }
    }
}