using System.Collections.Generic;
using UnityEngine;
using ItemChanger;

namespace VendorRando {
    public class IseldaContainer: VendorContainer<IseldaContainer> {
        public override string Name => "Iselda";

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Iselda"];
            npcOffset = new Vector3(19.84f - 18.8f, 7.25f - 6.4081f, 0.195f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "_Scenery/Mapper_home_0001_a", 18.71f - 18.8f, 8.22f - 6.4081f, 0.16f);
            addObject(preObjs, "Shop Region", 17.33f - 18.8f, 7.04f - 6.4081f, 0.009f);
        }
    }
}
