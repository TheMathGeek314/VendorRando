using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VendorRando {
    public class VendorLocation: ContainerLocation {
        public string vendorName;

        protected override void OnLoad() {
            Modding.Logger.Log("[VendorLocation] - VendorLocation.OnLoad() called");
        }

        protected override void OnUnload() {
            Modding.Logger.Log("[VendorLocation] - VendorLocation.OnUnload() called");
        }
    }
}
