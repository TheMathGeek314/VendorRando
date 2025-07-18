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
                ("Room_Charm_Shop", "Cinematic Player"),
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
//      Vanilla Salubra's Blessing kinda works but the video is black
//      Make sure heights are properly corrected (see Geo_Rock-Deepnest_Below_Spike_Grub and *_Dupe)
//          Take a look at dropping into place
//      Lantern/e-key/quill don't appear if vendor stock contains no rando'd items
//      Sly's dirtmouth room isn't working (except for other vendors?)
//
//  SWITCHING TO ACCESS-ITEM-CENTRIC DESIGN
//      Also make sure to grant logical access upon interacting with any shop in any way
//      Also update pin locations at runtime
//
//  DEPENDENCIES
//      ConnectionMetadataInjector
//      ItemChanger
//      MenuChanger
//      RandomizerCore
//      RandomizerCore.Json
//      RandomizerMod
//      Satchel
//  INTEGRATIONS
//      rando?
//      CondensedSpoilerLogger
//      QoL
//      RandoSettingsManager
//      map mod?