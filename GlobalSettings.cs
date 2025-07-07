namespace VendorRando {
    public class GlobalSettings {
        public bool Sly = true;
        public bool Salubra = true;
        public bool Iselda = true;
        public bool LegEater = true;
        public bool Lemm = true;

        public bool Any => Sly
                        || Salubra
                        || Iselda
                        || LegEater
                        || Lemm;
    }
}
