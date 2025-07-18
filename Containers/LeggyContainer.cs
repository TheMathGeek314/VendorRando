using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class LeggyContainer: VendorContainer<LeggyContainer> {
        public override string Name => Consts.LegEater;
        public override string VanillaPlacement => LocationNames.Leg_Eater;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Leg Eater"];
            npcOffset = new Vector3(45.677f/* - 45.61f*/, 6.035f/* - 5.4081f*/, 0.01f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(45.9084f, 5.4081f);
            addObject(preObjs, "leg_eater_scenery_0004_a", 46.82f/* - 45.61f*/, 4.36f/* - 5.4081f*/, 0.03f);
            addObject(preObjs, "Shop Region", 44.24f/* - 45.61f*/, 6.28f/* - 5.4081f*/, 0.009f);
        }

        protected override void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info) {
            base.setupShopRegion(npc, shopRegion, shopMenu, info);
            shopRegion.GetComponent<BoxCollider2D>().enabled = PlayerData.instance.paidLegEater;
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu);
            convCtrl.FsmVariables.GetFsmGameObject("Shop Region").Value = shopRegion;
            convCtrl.GetValidState("Init").RemoveAction(3);
            foreach((string state, int index, GameObject go) in new (string, int, GameObject)[] {
                ("Dial Box Down 2", 3, npc),//NPC TITLE DOWN
                ("Box Down", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(convCtrl.GetValidState(state), index, go);
            }
        }
    }
}
