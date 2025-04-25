using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemChanger;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace VendorRando {
    public static class LogicAdder {
        public static void Hook() {
            RCData.RuntimeLogicOverride.Subscribe(300, DefineLogic);//idk what good values would be here
            RCData.RuntimeLogicOverride.Subscribe(300, DefineTerms);

            RCData.RuntimeLogicOverride.Subscribe(100000, EditVanillaVendors);
        }

        private static void DefineLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            lmb.LogicLookup[Consts.SlyShop] = lmb.LogicLookup[LocationNames.Sly];
            lmb.LogicLookup[Consts.SalubraShop] = lmb.LogicLookup[LocationNames.Salubra];
            lmb.LogicLookup[Consts.IseldaShop] = lmb.LogicLookup[LocationNames.Iselda];
            lmb.LogicLookup[Consts.LeggyShop] = lmb.LogicLookup[LocationNames.Leg_Eater];
            lmb.LogicLookup[Consts.LemmShop] = lmb.LogicLookup[LocationNames.Geo_Rock_City_of_Tears_Lemm];//lmb.LogicLookup[LocationNames.Lemm];
        }

        private static void DefineTerms(GenerationSettings gs, LogicManagerBuilder lmb) {
            foreach(string v in Consts.ToArray()) {
                Term vendorTerm = lmb.GetOrAddTerm(v);
                lmb.AddItem(new SingleItem(v, new TermValue(vendorTerm, 1)));
            }
        }

        private static void EditVanillaVendors(GenerationSettings gs, LogicManagerBuilder lmb) {
            foreach((string vanilla, string constant) in new (string,string)[] {
                ("Sly", Consts.SlyShop),
                ("Sly_(Key)", Consts.SlyShop),
                ("Salubra", Consts.SalubraShop),
                ("Iselda", Consts.IseldaShop),
                ("Leg_Eater", Consts.LeggyShop),
                //("Lemm", Consts.LemmNPC) //Lemm doesn't hold vanilla items, maybe this is useful for other connections
            }) {
                if(lmb.LogicLookup.ContainsKey(vanilla)) {
                    lmb.DoLogicEdit(new(vanilla, constant));//I have no idea if this is how this works
                }
            }
        }
    }
}
