using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerCore;
using RandomizerMod.RC;
using UnityEngine;

namespace VendorRando {
    public class Workshop {
        public static void ApplyShopDefs(RequestBuilder rb) {
            string[] shops = new string[] { LocationNames.Sly, LocationNames.Sly_Key, LocationNames.Iselda, LocationNames.Salubra, LocationNames.Leg_Eater/*, LocationNames.Lemm*/ };
            foreach(string s in shops) {
                rb.LocationRequests.Remove(s);
            }
        }

        public static void doThings(ICFactory factory, RandoPlacement placement, Action<LocationRequestInfo> info, string s) {
            if(factory.TryFetchPlacement(s, out AbstractPlacement ap)) {
                
            }
        }
    }
}
