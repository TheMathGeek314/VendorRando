using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class SlyContainer: VendorContainer<SlyContainer> {
        public override string Name => Consts.Sly;
        public override string VanillaPlacement => LocationNames.Sly;
        protected override float npcInteractOffset => 0.6f;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Basement Closed"];
            npcOffset = new Vector3(0, 0, 0);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(17.136f, 6.4081f);
            addObject(preObjs, "_Scenery/Shop Counter", 17.17f, 5.68f, 0.023f);
        }

        public override GameObject GetNewContainer(ContainerInfo info) {
            GameObject sly = GetNewContainer(info, true);
            GameObject slyChild = sly.FindGameObjectInChildren("Basement Closed(Clone)");
            foreach(PlayMakerFSM fsm in slyChild.GetComponentsInChildren<PlayMakerFSM>()) {
                if(fsm.gameObject.name.StartsWith("Basement Closed") && fsm.FsmName.StartsWith("Control")) {
                    slyChild.GetComponent<PlayMakerFSM>().GetValidState("Check").RemoveAction(0);
                }
            }
            foreach(string childName in new string[] { "Shop Region", "Sly Shop" }) {
                slyChild.FindGameObjectInChildren(childName).SetActive(true);
            }
            return sly;
        }

        protected override void doInteractOffset(GameObject go, float offset) {
            base.doInteractOffset(go.FindGameObjectInChildren("Sly Shop"), offset);
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu, TrackProgression tpAction) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu, tpAction);
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
