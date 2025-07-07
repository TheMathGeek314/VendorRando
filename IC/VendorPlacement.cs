using System;
using UnityEngine;
using ItemChanger;
using ItemChanger.Placements;
using System.Collections.Generic;
using System.Linq;

namespace VendorRando {
    public class VendorPlacement: ShopPlacement, IContainerPlacement, IPrimaryLocationPlacement, IMultiCostPlacement {
        public new AbstractLocation Location;
        AbstractLocation IPrimaryLocationPlacement.Location => Location;
        public string containerType;

        public new string requiredPlayerDataBool;
        public new bool dungDiscount;
        public new DefaultShopItems defaultShopItems;

        public VendorPlacement(string Name): base(Name) { }

        protected override void OnLoad() {
            Location.Placement = this;
            Location.Load();
        }

        protected override void OnUnload() {
            Location.Unload();
        }

        public void GetContainer(AbstractLocation location, out GameObject obj, out string containerType) {
            if(this.containerType == Container.Unknown) {
                this.containerType = Consts.Sly;
            }
            containerType = this.containerType;
            var container = Container.GetContainer(containerType);
            if(container == null || !container.SupportsInstantiate) {
                this.containerType = containerType = Consts.Sly;
                container = Container.GetContainer(containerType);
                if(container == null)
                    throw new InvalidOperationException($"You did an oopsie and we couldn't resolve container type {containerType} for placement {Name}");
            }

            ContainerInfo ci = new(this.containerType, this, FlingType.DirectDeposit);
            if(container is SlyContainer vContainer) {
                obj = vContainer.GetNewContainer(ci, requiredPlayerDataBool);
            }
            else {
                obj = container.GetNewContainer(ci);
            }
        }

        public override IEnumerable<Tag> GetPlacementAndLocationTags() {
            try {
                return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
            }
            catch(NullReferenceException) {
                return Location.tags ?? Enumerable.Empty<Tag>();
            }
        }
    }
}