using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    public sealed class BasketPresentation
    {
        public int Id { get; }
        public int ProductId { get; }
        public int Slot { get; }
        public IReadOnlyList< ItemPresentation > Items { get; }
        public int Count => Items.Count;
        public bool Complete { get; }
        public string ProgressLabel { get; }

        public BasketPresentation( int id, int productId, int slot, IReadOnlyList< ItemPresentation > items, bool complete, string progressLabel )
        {
            Id = id;
            ProductId = productId;
            Slot = slot;
            Items = items;
            Complete = complete;
            ProgressLabel = progressLabel;
        }
    }
}
