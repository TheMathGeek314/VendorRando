using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class SalubraContainer: VendorContainer<SalubraContainer> {
        public override string Name => Consts.Salubra;
        public override string VanillaPlacement => LocationNames.Salubra;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Charm Slug"];
            npcOffset = new Vector3(19.33f, 7.93f, 0.13f);//0, 1.93f - 0.4081f, 0.13f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(19.302f, 6.4081f);
            addObject(preObjs, "shop_0000_a", 19.52f, 6.112f, 0.03f);//0.29f, 0.112f - 0.4081f, 0.03f);
            addObject(preObjs, "Shop Region", 17.65f, 7.22f, 0.009f);//17.65f - 19.33f, 1.22f - 0.4081f, 0.009f);
            addObject(preObjs, "Scene Blanker", 18.779f, 8.3673f, -0.005f);//18.779f - 19.33f, 8.3673f - 0.4081f, -0.005f);
            addObject(preObjs, "Cinematic Player", 0, 0, -5.12f);
        }

        protected override void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info) {
            base.setupShopRegion(npc, shopRegion, shopMenu, info);
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "Give Blessing") {
                    setTargetToGameObject(fsm.GetValidState("End"), 4, shopRegion);//SHOP REGION ACTIVE
                }
            }
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu);
            convCtrl.FsmVariables.GetFsmGameObject("Shop Region").Value = shopRegion;
            convCtrl.GetValidState("Met?").RemoveAction(2);//Find Shop Region
            foreach((string state, int index, GameObject go) in new (string, int, GameObject)[] {
                ("To Shop", 3, npc),//NPC TITLE DOWN
                ("To Shop", 4, shopRegion),//DO SHOP OPEN
                ("Box Down", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(convCtrl.GetValidState(state), index, go);
            }
        }
    }
}
