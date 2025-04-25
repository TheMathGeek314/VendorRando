using System.Collections.Generic;
using UnityEngine;
using ItemChanger;

namespace VendorRando{
    public class LemmContainer: VendorContainer<LemmContainer> {
        public override string Name => "Lemm";

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Relic Dealer"];
            npcOffset = new Vector3(53.5077f - 51.893f, 24.99f - 23.4081f, 0.03f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "antique_shop/antique_r_0007_a", 51.7226f - 51.893f, 24.3305f - 23.4081f, 0.0223f);
            addObject(preObjs, "antique_shop/antique_r_0006_a", 52.5207f - 51.893f, 22.916f - 23.4081f, 0.0211f);
            //ruins_clutter_0010_a to be deleted in city but not added at check location
            addObject(preObjs, "Shop Region", 51.15f - 51.893f, 24.25f - 23.4081f, 0.009f);
        }

        //probably do something to ensure qol sell-all still works
    }
}
