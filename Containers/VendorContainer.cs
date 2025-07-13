using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Internal;
using Satchel;

namespace VendorRando {
    public abstract class VendorContainer<T>: Container where T : VendorContainer<T> {
        protected static GameObject npcObject;
        protected static Vector3 npcOffset;
        protected static GameObject menuObject;
        protected static List<GameObject> otherObjects;
        protected static List<Vector3> objectOffset;
        protected List<GameObject> myObjects;
        
        //public static string VanillaPlacement;
        public abstract string VanillaPlacement { get; }
        public static AbstractPlacement VanillaShopPlacement;

        public override bool SupportsInstantiate => true;

        public override GameObject GetNewContainer(ContainerInfo info) {
            return GetNewContainer(info, false);
        }

        public GameObject GetNewContainer(ContainerInfo info, bool regionIsChild = false, string requiredBool = "") {
            GameObject npc = GameObject.Instantiate(npcObject);
            GameObject myMenu = GameObject.Instantiate(menuObject, new Vector3(8.53f, 0.54f, -1.8609f), Quaternion.identity);
            myMenu.SetActive(true);
            myObjects = new();
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "npc_control") {
                    fsm.gameObject.AddComponent<ContainerEnableConfig>().setFsm(fsm);
                    break;
                }
            }
            if(regionIsChild) {
                GameObject region = npc.FindGameObjectInChildren("Shop Region");
                setupShopRegion(npc, region, myMenu, info, requiredBool);
            }
            foreach(GameObject obj in otherObjects) {
                GameObject ob2 = GameObject.Instantiate(obj);
                if(ob2.name.StartsWith("Shop Region")) {
                    setupShopRegion(npc, ob2, myMenu, info, requiredBool);
                }
                myObjects.Add(ob2);
            }
            npc.AddComponent<ContainerInfoComponent>().info = info;
            return npc;
        }

        public override void ApplyTargetContext(GameObject obj, float x, float y, float elevation) {
            obj.transform.position = new Vector3(x, y - elevation, 0) + npcOffset;
            obj.SetActive(true);
            for(int i = 0; i < myObjects.Count; i++) {
                myObjects[i].transform.position = new Vector3(x, y - elevation, 0) + objectOffset[i];
                myObjects[i].SetActive(true);
            }
        }

        public override void ApplyTargetContext(GameObject obj, GameObject target, float elevation) {
            ApplyTargetContext(obj, target.transform.position.x, target.transform.position.y, elevation);
        }

        protected static void addObject(Dictionary<string, GameObject> po, string name, float x, float y, float z) {
            otherObjects ??= new();
            objectOffset ??= new();
            otherObjects.Add(po[name]);
            objectOffset.Add(new Vector3(x, y, z));
        }

        protected virtual void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info, string requiredBool) {
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "Conversation Control") {
                    editConvCtrl(fsm, npc, shopRegion, shopMenu);
                }
            }
            PlayMakerFSM srFSM = shopRegion.GetComponent<PlayMakerFSM>();
            srFSM.GetValidState("Move Hero Left").InsertAction(new IgnoreProximity(), 0);
            srFSM.GetValidState("Move Hero Right").InsertAction(new IgnoreProximity(), 0);
            srFSM.FsmVariables.GetFsmGameObject("Shop Object").Value = shopMenu;
            srFSM.GetValidState("Init").RemoveAction(2); //overwrote Shop Object
            srFSM.GetValidState("Re Init").RemoveAction(2); //again
            foreach((string state, int index, GameObject targetGO) in new (string, int, GameObject)[] {
                ("Shop Up Msg", 0, npc),//SHOP START
                ("Regain Control", 1, npc),//SHOP STOP
                ("Box Down Relic", 1, npc),//NPC TITLE DOWN
                ("Box Down", 1, npc),//NPC TITLE DOWN
                ("Box Down 2", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(srFSM.GetValidState(state), index, targetGO);
            }
            VendorUtils.EditShopRegion(srFSM);

            PlayMakerFSM[] menuFSMs = shopMenu.GetComponentsInChildren<PlayMakerFSM>(true);
            foreach(PlayMakerFSM fsm in menuFSMs) {
                if(fsm.FsmName == "shop_control") {
                    foreach((string state, int index, GameObject targetGO) in new (string, int, GameObject)[] {
                        ("Special Close", 14, shopRegion),//SHOP CLOSED QUICK
                        ("Special Close To Inactive", 14, shopRegion),//SHOP REGION INACTIVE
                        ("Send Event", 2, shopRegion),//SHOP CLOSED
                        ("Leg Eater", 0, npc),//SHOP TALK START
                        ("Box Down", 1, npc),//SHOP TALK END
                        ("Box Down", 2, npc),//NPC TITLE DOWN
                        ("Broken Charm", 1, npc),//SHOP TALK START
                        ("Box Down 2", 1, npc),//SHOP TALK END
                        ("Box Down 2", 2, npc),//NPC TITLE DOWN
                        ("Dung Convo", 2, npc),//SHOP TALK START
                        ("Box Down 3", 1, npc),//SHOP TALK END
                        ("Box Down 3", 2, npc),//NPC TITLE DOWN
                        ("Open Window", 1, npc)//SHOP WINDOW UP
                    }) {
                        setTargetToGameObject(fsm.GetValidState(state), index, targetGO);
                    }
                    DefaultShopItems dsItems = info.giveInfo.placement.HasTag<VendorTag>() ? info.giveInfo.placement.GetTag<VendorTag>().defaultShopItems : DefaultShopItems.None;
                    //VendorUtils.EditShopControl(fsm, info.giveInfo.placement, Name, dsItems, requiredBool);
                    //VendorUtils.EditShopControl(fsm, VanillaShopPlacement, Name, dsItems, requiredBool);
                    Dictionary<string, AbstractPlacement> rsp = Ref.Settings.Placements;
                    if(!rsp.ContainsKey(VanillaPlacement)) {
                        VendorRando.vlog($"rps does not contain key for {VanillaPlacement}");
                    }
                    else if(rsp[VanillaPlacement] == null) {
                        VendorRando.vlog($"rsp[{VanillaPlacement}] is null");
                    }
                    else {
                        VendorRando.vlog($"rsp[{VanillaPlacement}] = " + String.Join(", ", rsp[VanillaPlacement].Items.Select(item => item.name)));
                    }
                    if(VanillaShopPlacement == null) {
                        VendorRando.vlog($"VanillaShopPlacement is null for {VanillaPlacement}");
                    }
                    else {
                        VendorRando.vlog($"{VanillaPlacement}.VanillaShopPlacement = " + String.Join(", ", VanillaShopPlacement.Items.Select(item => item.name)));
                    }
                    VendorUtils.EditShopControl(fsm, Ref.Settings.Placements.ContainsKey(VanillaPlacement) ? Ref.Settings.Placements[VanillaPlacement] : null, Name, dsItems, requiredBool);
                }
                if(fsm.FsmName == "Confirm Control") {
                    foreach((string state, int index, GameObject targetGO) in new (string, int, GameObject)[] {
                        ("Close Shop Window", 1, shopMenu),//CLOSE SHOP WINDOW
                        ("Close Shop Window 2", 0, shopMenu),//CLOSE SHOP WINDOW
                        ("Reset", 1, shopMenu),//RESET SHOP WINDOW
                        ("Set Shop Inactive", 1, shopMenu)//CLOSE SHOP WINDOW INACTIVE
                    }) {
                        setTargetToGameObject(fsm.GetValidState(state), index, targetGO);
                    }
                    VendorUtils.EditConfirmControl(fsm);
                    VendorUtils.HastenConfirmControl(fsm);
                }
                if(fsm.FsmName == "Item List Control") {
                    VendorUtils.EditItemListControl(fsm);
                    VendorUtils.HastenItemListControl(fsm);
                }
                if(fsm.FsmName == "ui_list") {
                    VendorUtils.HastenUIList(fsm);
                }
                if(fsm.FsmName == "ui_list_getinput") {
                    VendorUtils.HastenUIListGetInput(fsm);
                }
                if(fsm.FsmName == "ui_list_button_listen") {
                    VendorUtils.HastenUIListButtonListen(fsm);
                }
            }
        }

        public void setTargetToGameObject(FsmState state, int index, GameObject gameObject) {
            FsmEventTarget target = ((SendEventByName)state.Actions[index]).eventTarget;
            target.target = FsmEventTarget.EventTarget.GameObject;
            target.gameObject.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
            target.gameObject.GameObject.Value = gameObject;
        }

        protected virtual void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu) {}
    }

    public class IgnoreProximity: FsmStateAction {
        public override void OnEnter() {
            Fsm.Event(FsmEvent.GetFsmEvent("FINISHED"));
            Finish();//Is this needed? ¯\_(ツ)_/¯
        }
    }

    public class ContainerEnableConfig: MonoBehaviour {
        private PlayMakerFSM fsm;
        public Action<PlayMakerFSM> doMoreStuffs;

        public void setFsm(PlayMakerFSM fsm) {
            this.fsm = fsm;
        }

        public void run() {
            FsmEventTarget target = ((SendEventByName)fsm.GetValidState("Reset Convo").Actions[1]).eventTarget;//NPC TITLE DOWN
            target.target = FsmEventTarget.EventTarget.GameObject;
            target.gameObject.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
            target.gameObject.GameObject.Value = fsm.gameObject;

            fsm.GetValidState("Move Hero Left").InsertAction(new IgnoreProximity(), 0);
            fsm.GetValidState("Move Hero Right").InsertAction(new IgnoreProximity(), 0);

            doMoreStuffs?.Invoke(fsm);

            gameObject.RemoveComponent<ContainerEnableConfig>();
        }
    }
}
