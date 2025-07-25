﻿using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class LeggyContainer: VendorContainer<LeggyContainer> {
        public override string Name => Consts.LegEater;
        public override string VanillaPlacement => LocationNames.Leg_Eater;
        protected override float npcInteractOffset => 1.4f;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Leg Eater"];
            npcOffset = new Vector3(45.677f, 6.035f, 0.01f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(45.8084f, 5.4081f);
            addObject(preObjs, "leg_eater_scenery_0004_a", 46.82f, 4.36f, 0.03f);
            addObject(preObjs, "Shop Region", 44.24f, 6.28f, 0.009f);
        }

        protected override void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info, TrackProgression tpAction) {
            base.setupShopRegion(npc, shopRegion, shopMenu, info, tpAction);
            shopRegion.GetComponent<BoxCollider2D>().enabled = PlayerData.instance.paidLegEater;
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu, TrackProgression tpAction) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu, tpAction);
            convCtrl.FsmVariables.GetFsmGameObject("Shop Region").Value = shopRegion;
            convCtrl.GetValidState("Init").RemoveAction(3);
            foreach((string state, int index, GameObject go) in new (string, int, GameObject)[] {
                ("Dial Box Down 2", 3, npc),//NPC TITLE DOWN
                ("Box Down", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(convCtrl.GetValidState(state), index, go);
            }
            convCtrl.GetValidState("Something Bought").GetFirstActionOfType<SetBoxCollider2DSize>().offsetX = npcInteractOffset;
        }
    }
}
