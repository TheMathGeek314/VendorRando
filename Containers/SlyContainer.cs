﻿using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class SlyContainer: VendorContainer<SlyContainer> {
        public override string Name => Consts.Sly;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Basement Closed"];
            npcOffset = new Vector3(-17.2f, -6.4081f, 0.029f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "_Scenery/Shop Counter", -0.03f, -0.7281f, 0.023f);
        }

        public override GameObject GetNewContainer(ContainerInfo info) {
            return GetNewContainer(info, "");
        }

        public GameObject GetNewContainer(ContainerInfo info, string requiredBool) {
            GameObject sly = base.GetNewContainer(info, true, requiredBool);
            foreach(PlayMakerFSM fsm in sly.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.gameObject.name.StartsWith("Basement Closed") && fsm.FsmName.StartsWith("Control")) {
                    sly.GetComponent<PlayMakerFSM>().GetValidState("Check").RemoveAction(0);
                }
            }
            return sly;
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu);
            convCtrl.FsmVariables.GetFsmGameObject("Shop Menu").Value = shopMenu;
            convCtrl.GetValidState("Store Key").RemoveAction(0);//Find Shop Menu
            foreach((string state, int index, GameObject go) in new (string, int, GameObject)[] {
                ("To Shop", 2, npc),//NPC TITLE DOWN
                ("To Shop", 3, shopRegion),//DO SHOP OPEN
                ("Box Down", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(convCtrl.GetValidState(state), index, go);
            }
        }
    }
}
