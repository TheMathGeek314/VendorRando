using System.Collections.Generic;
using UnityEngine;
using ItemChanger;

namespace VendorRando {
    public class LeggyContainer: VendorContainer<LeggyContainer> {
        public override string Name => "Leg Eater";

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Leg Eater"];
            npcOffset = new Vector3(45.677f - 45.61f, 6.035f - 5.4081f, 0.01f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "leg_eater_scenery_0004_a", 46.82f - 45.61f, 4.36f - 5.4081f, 0.03f);
            addObject(preObjs, "Shop Region", 44.24f - 45.61f, 6.28f - 5.4081f, 0.009f);
        }
    }
}
