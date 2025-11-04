namespace VendorRando {
    public class GlobalSettings {
        public bool Sly = false;
        public bool Salubra = false;
        public bool Iselda = false;
        public bool LegEater = false;
        public bool Lemm = false;

        public bool Any => Sly
                        || Salubra
                        || Iselda
                        || LegEater
                        || Lemm;
    }

    public class LocalSettings {
        public bool Sly = false;
        public bool Salubra = false;
        public bool Iselda = false;
        public bool LegEater = false;
        public bool Lemm = false;

        public bool OverrideIsRando = false;
    }
}
