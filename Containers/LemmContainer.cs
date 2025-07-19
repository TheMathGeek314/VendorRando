using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker;
using Modding;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class LemmContainer: VendorContainer<LemmContainer> {
        public override string Name => Consts.Lemm;
        public override string VanillaPlacement => LocationNames.Lemm;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Relic Dealer"];
            npcOffset = new Vector3(53.5077f, 24.99f, 0.03f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(52.8284f, 23.4081f);
            addObject(preObjs, "antique_shop/antique_r_0007_a", 51.7226f, 24.3305f, 0.0223f);
            addObject(preObjs, "antique_shop/antique_r_0006_a", 52.5207f, 22.916f, 0.0211f);
            //ruins_clutter_0010_a to be deleted in city but not added at check location
            addObject(preObjs, "Shop Region", 51.15f, 24.25f, 0.009f);
        }

        public override GameObject GetNewContainer(ContainerInfo info) {
            GameObject npc = GetNewContainer(info);
            VendorRando.vlog("FSMs found: " + npc.GetComponentsInChildren<PlayMakerFSM>(true).Length);
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>(true)) {
                VendorRando.vlog($"Checking fsm {fsm.name}");
                if(fsm.FsmName == "npc_control") {
                    VendorRando.vlog("found mpc_control, adding QolSellAll");
                    npc.GetComponentInChildren<ContainerEnableConfig>(true).doMoreStuffs += QolSellAll;
                }
                break;
            }
            return npc;
        }

        protected override void setupShopRegion(GameObject npc, GameObject shopRegion, GameObject shopMenu, ContainerInfo info, TrackProgression tpAction) {
            base.setupShopRegion(npc, shopRegion, shopMenu, info, tpAction);
            foreach(PlayMakerFSM fsm in npc.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.FsmName == "Relic Discussions") {
                    setTargetToGameObject(fsm.GetValidState("End"), 1, shopRegion);//SHOP OPEN AUTO
                }
            }
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu, TrackProgression tpAction) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu, tpAction);
            setTargetToGameObject(convCtrl.GetValidState("Box Down"), 1, npc);//NPC TITLE DOWN
        }

        private void QolSellAll(PlayMakerFSM npcCtrl) {
            VendorRando.vlog("QolSellAll");
            if(ModHooks.GetMod("QoL") is Mod) {
                VendorRando.vlog("QoL is Mod");
                if(QoL.Modules.NPCSellAll.LemmSellAll) {
                    VendorRando.vlog("LemmSellAll");
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
