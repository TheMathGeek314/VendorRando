using System.Collections.Generic;

namespace VendorRando {
    public static class Consts {
        public const string Sly = "Vr_Sly";
        public const string Salubra = "Vr_Salubra";
        public const string Iselda = "Vr_Iselda";
        public const string LegEater = "Vr_Leg_Eater";
        public const string Lemm = "Vr_Lemm";

        public const string AccessSalubra = "Vr_Salubra_Access";
        public const string AccessIselda = "Vr_Iselda_Access";
        public const string AccessLeggy = "Vr_Leggy_Access";
        public const string AccessLemm = "Vr_Lemm_Access";
        public const string AccessSly = "Vr_Sly_Access";

        public static List<string> Names = [Sly, Salubra, Iselda, LegEater, Lemm];
        public static List<string> AccessNames = [AccessSly, AccessSalubra, AccessIselda, AccessLeggy, AccessLemm];
    }
}
