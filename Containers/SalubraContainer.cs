using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class SalubraContainer: VendorContainer<SalubraContainer> {
        public override string Name => "Salubra";

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Charm Slug"];
            npcOffset = new Vector3(0, 1.93f - 0.4081f, 0.13f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "shop_0000_a", 0.29f, 0.112f - 0.4081f, 0.03f);
            addObject(preObjs, "Shop Region", 17.65f - 19.33f, 1.22f - 0.4081f, 0.009f);
        }
    }
}
