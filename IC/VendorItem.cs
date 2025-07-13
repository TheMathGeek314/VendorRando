using ItemChanger.Items;

namespace VendorRando {
    public class VendorItem: VoidItem {
        private string container;
        public VendorItem(string container) {
            this.container = container;
        }
        public override string GetPreferredContainer() => container;
    }
}
