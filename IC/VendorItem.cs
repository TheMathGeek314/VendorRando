using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Tags;

namespace VendorRando {
    public class VendorItem: VoidItem {
        public string container;
        public VendorItem(string container) {
            this.container = container;
            AddTag<PersistentItemTag>().Persistence = Persistence.Persistent;
        }
        public override string GetPreferredContainer() => container;
    }
}
