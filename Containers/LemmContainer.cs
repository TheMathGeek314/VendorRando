using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker;
using Modding;
using ItemChanger;
using Satchel;

namespace VendorRando{
    public class LemmContainer: VendorContainer<LemmContainer> {
        public override string Name => Consts.Lemm;
        public override string VanillaPlacement => LocationNames.Lemm;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Relic Dealer"];
            npcOffset = new Vector3(53.5077f - 51.893f, 24.99f - 23.4081f, 0.03f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(52.8284f, 23.4081f);
            addObject(preObjs, "antique_shop/antique_r_0007_a", 51.7226f/* - 51.893f*/, 24.3305f/* - 23.4081f*/, 0.0223f);
            addObject(preObjs, "antique_shop/antique_r_0006_a", 52.5207f/* - 51.893f*/, 22.916f/* - 23.4081f*/, 0.0211f);
            //ruins_clutter_0010_a to be deleted in city but not added at check location
            addObject(preObjs, "Shop Region", 51.15f/* - 51.893f*/, 24.25f/* - 23.4081f*/, 0.009f);
        }

        public override GameObject GetNewContainer(ContainerInfo info) {
            GameObject npc = GetNewContainer(info);
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "npc_control") {
                    npc.GetComponentInChildren<ContainerEnableConfig>().doMoreStuffs += QolSellAll;
                }
                break;
            }
            return npc;
        }

        protected override void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info) {
            base.setupShopRegion(npc, shopRegion, shopMenu, info);
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "Relic Discussions") {
                    setTargetToGameObject(fsm.GetValidState("End"), 1, shopRegion);//SHOP OPEN AUTO
                }
            }
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu);
            setTargetToGameObject(convCtrl.GetValidState("Box Down"), 1, npc);//NPC TITLE DOWN
        }

        private void QolSellAll(PlayMakerFSM npcCtrl) {
            if(ModHooks.GetMod("QoL") is Mod) {
                if(QoL.Modules.NPCSellAll.LemmSellAll) {
                    npcCtrl.GetValidState("Convo End").AddAction(new SellAllAction());
                }
            }
        }
    }

    public class SellAllAction: FsmStateAction {
        private MethodInfo SellRelics;
        public SellAllAction() {
            SellRelics = typeof(QoL.Modules.NPCSellAll).GetMethod("SellRelics", BindingFlags.NonPublic | BindingFlags.Static);
        }
        public override void OnEnter() {
            SellRelics.Invoke(null, null);
            Finish();
        }
    }
}
