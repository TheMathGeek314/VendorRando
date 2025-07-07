using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using Satchel;
using ItemChanger;
using MenuChanger;
using HutongGames.PlayMaker.Actions;
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

            //ModHooks.HeroUpdateHook += heroUpdate;
            //On.HutongGames.PlayMaker.Fsm.Event_FsmEventTarget_FsmEvent += fsmEventTarget;
        }

        /*private void heroUpdate() {
            if(Input.GetKeyDown(KeyCode.O)) {
                vlog("pressed o");
                //Dictionary<string, string> receivedEvents = new();
                List<(string, string)> receivedEvents = new();
                foreach(var go in GameObject.FindObjectsOfType<GameObject>(true)) {
                    foreach(var fsm in go.GetComponentsInChildren<PlayMakerFSM>()) {
                        foreach(var fsmevent in fsm.FsmEvents) {
                            string key = $"{go.name} - {fsm.FsmName}";
                            receivedEvents.Add((key, fsmevent.Name));
                        }
                    }
                }
                foreach(var go in GameObject.FindObjectsOfType<GameObject>()) {
                    foreach(var fsm in go.GetComponentsInChildren<PlayMakerFSM>()) {
                        foreach(string soughtEvent in new string[] { "SCENE BLANKER OFF", "SCENE BLANKER ON", "SHOP REGION ACTIVE" }) {
                            if(new List<FsmEvent>(fsm.FsmEvents).Contains(FsmEvent.GetFsmEvent(soughtEvent))) {
                                //vlog($"Found event {soughtEvent} on {fsm.gameObject.name}-{fsm.FsmName}");
                            }
                        }
                        foreach(FsmState state in fsm.FsmStates) {
                            foreach(FsmStateAction action in state.Actions) {
                                if(action.GetType() == typeof(SendEventByName)) {
                                    if(((SendEventByName)action).eventTarget.target == FsmEventTarget.EventTarget.BroadcastAll) {
                                        if(!(new List<string>() {
                                            "Attack Reminder", "Charm Effects", "Damage Effect", "Dream Dialogue", "Dream Dialogue (2)",
                                            "Inventory", "Jump Reminder", "Knight", "Map Key", "Tut_tablet_top", "Tut_tablet_top (1)",
                                            "Tut_tablet_top (2)", "Soul Orb", "Text", "Charms", "Inspect Region", "Hero Death"
                                        }).Contains(fsm.Fsm.GameObjectName)) {
                                            if(!fsm.Fsm.GameObjectName.Contains("Health")) {
                                                //vlog("BroadcastAll found: \t" + fsm.Fsm.GameObjectName + "\t" + fsm.FsmName + "\t" + state.Name + "\t" + ((SendEventByName)action).sendEvent.Value);
                                                processBroadcast($"{fsm.Fsm.GameObjectName} - {fsm.FsmName} - {state.Name}", ((SendEventByName)action).sendEvent.Value, receivedEvents);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /*private void processBroadcast(string source, string eventName, List<(string, string)> receivedEvents) {
            foreach(var revent in receivedEvents) {
                if(revent.Item2 == eventName) {
                    vlog($"BroadcastAll found: event\t{eventName}\tsent from\t{source}\tto\t{revent.Item1}");
                }
            }
        }*/

        /*private void fsmEventTarget(On.HutongGames.PlayMaker.Fsm.orig_Event_FsmEventTarget_FsmEvent orig, Fsm self, FsmEventTarget eventTarget, FsmEvent fsmEvent) {
            if(fsmEvent != null && (fsmEvent.Name == "SHOP START" || fsmEvent.Name == "SHOP STOP")) {
                vlog($"fsmEventTarget {self.GameObjectName} - {self.ActiveState.Name} - {fsmEvent.Name}");
            }
            orig(self, eventTarget, fsmEvent);
        }*/

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
            /*PlayerData.instance.trinket4 = 5;
            PlayerData.instance.trinket2 = 2;//THESE SHOULD NOT STAY IN
            PlayerData.instance.trinket3 = 12;*/
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
//      Double-check the map pin coords in dirtmouth
//      Check the BuiltInRequests about ApplyLongLocationPreviewSettings
//      Patch can_visit_Lemm logic while maintaining cloned Vr-Lemm location logic
//      Vanilla Salubra's Blessing blackscreens and does not return control
//      Make sure MoreLocations Lemm shop and LoreRando cursed listening either work right or deny access
//      Make sure heights are properly corrected (see Geo_Rock-Deepnest_Below_Spike_Grub and *_Dupe)
//
//      I've created a slykey location but it's not appearing for some reason, and I suspect it has something to do with the lack of immediate logic
//      For testing purposes, try giving it bogus logic at an acceptable time to see if this resolves anything