namespace VendorRando {
    public static class Consts {

        public const string SlyShop = "VR-SlyShop";
        public const string SalubraShop = "VR-SalubraShop";
        public const string IseldaShop = "VR-IseldaShop";
        public const string LeggyShop = "VR-LegEaterShop";
        public const string LemmShop = "VR-LemmShop";

        public const string VendorPoolGroup = "Vendors";

        public const float LOGICPRIORITY = 50f;// I have no idea what this should be, mushroom is 50, lever is 0.3

        public static string[] ToArray() {
            return new string[] { SlyShop, SalubraShop, IseldaShop, LeggyShop, LemmShop };
        }
    }
}
