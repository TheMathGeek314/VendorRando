using ItemChanger.Items;

namespace VendorRando {
    public class VendorItem: VoidItem {
        public string container;
        public VendorItem(string container) {
            this.container = container;
        }
        public override string GetPreferredContainer() => container;
    }
}
