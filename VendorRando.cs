using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using RandomizerMod.IC;

namespace VendorRando {
    public class VendorRando: Mod, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "VendorRando";
        public override string GetVersion() => "1.0.0.0";

        public static GlobalSettings Settings { get; set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => Settings = s;
        public GlobalSettings OnSaveGlobal() => Settings;

        internal static VendorRando instance;

        public VendorRando() : base(null) {
            instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            definePrefabs(preloadedObjects);
            On.GameManager.OnNextLevelReady += onSceneChange;
            On.PlayMakerFSM.OnEnable += editFsm;

            RandoInterop.HookRandomizer();
        }

        public override List<(string, string)> GetPreloadNames() {
            return [
                ("Room_shop", "Basement Closed"),
                ("Room_shop", "_Scenery/Shop Counter"),
                ("Room_shop", "Shop Menu"),
                ("Room_Charm_Shop", "Charm Slug"),
                ("Room_Charm_Shop", "shop_0000_a"),
                ("Room_Charm_Shop", "Shop Region"),
                ("Room_Charm_Shop", "Shop Menu"),
                ("Room_Charm_Shop", "Scene Blanker"),
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
            ];
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
            if(IsRandoSave()) {
                switch(self.sceneName) {
                    case "Room_shop":
                        if(Settings.Sly) {
                            foreach(string go in new string[] { "Basement Closed", "_Scenery/Shop Counter" }) {
                                try {
                                    GameObject.Find(go).SetActive(false);
                                }
                                catch(Exception) { }
                            }
                        }
                        break;
                    case "Room_Charm_Shop":
                        if(Settings.Salubra) {
                            foreach(string go in new string[] { "Charm Slug", "shop_0000_a", "Shop Region" }) {
                                try {
                                    GameObject.Find(go).SetActive(false);
                                }
                                catch(Exception) { }
                            }
                        }
                        break;
                    case "Room_mapper":
                        if(Settings.Iselda) {
                            foreach(string go in new string[] { "Iselda", "_Scenery/Mapper_home_0001_a", "Shop Region" }) {
                                try {
                                    GameObject.Find(go).SetActive(false);
                                }
                                catch(Exception) { }
                            }
                        }
                        break;
                    case "Fungus2_26":
                        if(Settings.LegEater) {
                            foreach(string go in new string[] { "Leg Eater", "leg_eater_scenery_0004_a", "Shop Region" }) {
                                try {
                                    GameObject.Find(go).SetActive(false);
                                }
                                catch(Exception) { }
                            }
                        }
                        break;
                    case "Ruins1_05b":
                        if(Settings.Lemm) {
                            foreach(string go in new string[] { "Relic Dealer", "antique_shop/antique_r_0007_a", "antique_shop/antique_r_0006_a", "antique_shop/ruins_clutter_0010_a", "Shop Region" }) {
                                try {
                                    GameObject.Find(go).SetActive(false);
                                }
                                catch(Exception) { }
                            }
                        }
                        break;
                    case "Crossroads_04":
                        if(Settings.Salubra) {
                            GameObject.Find("Audio Salubra").SetActive(false);
                        }
                        break;
                }
            }
        }

        private void editFsm(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(self.FsmName == "npc_control" && self.gameObject.TryGetComponent(out ContainerEnableConfig config)) {
                config.run();
            }
        }

        public static bool IsRandoSave() {
            try {
                RandomizerModule module = ItemChangerMod.Modules.Get<RandomizerModule>();
                return module is not null;
            }
            catch(NullReferenceException) {
                return false;
            }
        }

        public static void vlog(string msg) {
            Modding.Logger.Log($"[VendorRando] - {msg}");
        }
    }
}
// TODO
//      Lemm should not be selling items
//      Resolve shopkey
//      Actually fix salubra requires charms
//      Look at the pinned guide for other integrations and features (RandoSettingsManager, major items, spoiler logging, etc)
//      Place vanilla items in rando logic
//      Check the BuiltInRequests about ApplyLongLocationPreviewSettings
//      Vanilla Salubra's Blessing blackscreens and does not return control
//      Make sure MoreLocations Lemm shop and LoreRando cursed listening either work right or deny access
//      Make sure heights are properly corrected (see Geo_Rock-Deepnest_Below_Spike_Grub and *_Dupe)
//
//  SWITCHING TO ACCESS-ITEM-CENTRIC DESIGN
//      RequestModifier is replaced by RequestModifier2 (subject to renaming later), and VendorPlacement is no longer in use
//      Figure out how to access vanilla ShopPlacements in VendorUtils from GetNewContainer
//      Lemm is probably broken too
//      Just test everything tbh, especially shopkey
//      Also make sure to grant logical access upon interacting with any shop in any way
//      Also update pin locations at runtime