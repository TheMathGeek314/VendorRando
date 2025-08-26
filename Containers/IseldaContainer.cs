using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class IseldaContainer: VendorContainer<IseldaContainer> {
        public override string Name => Consts.Iselda;
        public override string VanillaPlacement => LocationNames.Iselda;
        protected override float npcInteractOffset => 0;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Iselda"];
            npcOffset = new Vector3(19.84f, 7.25f, 0.195f);
            menuObject = preObjs["Shop Menu"];
            knightPosition = new Vector3(18.92f, 6.4081f);
            addObject(preObjs, "_Scenery/Mapper_home_0001_a", 18.71f, 8.22f, 0.16f);
            addObject(preObjs, "Shop Region", 17.33f, 7.04f, 0.009f);
        }

        protected override void editConvCtrl(PlayMakerFSM convCtrl, GameObject npc, GameObject shopRegion, GameObject shopMenu, TrackProgression tpAction) {
            base.editConvCtrl(convCtrl, npc, shopRegion, shopMenu, tpAction);
            convCtrl.FsmVariables.GetFsmGameObject("Shop Region").Value = shopRegion;
            foreach((string state, int index, GameObject go) in new (string, int, GameObject)[] {
                ("To Shop", 1, npc),//NPC TITLE DOWN
                ("To Shop", 2, shopRegion),//DO SHOP OPEN
                ("Box Down", 1, npc)//NPC TITLE DOWN
            }) {
                setTargetToGameObject(convCtrl.GetValidState(state), index, go);
            }
            if(RandoInterop.loreExists) {
                doLoreIntegration(convCtrl);
            }
        }
    }
}
