namespace RosiesCornerShop.Core
{
    public sealed class ItemPresentation
    {
        public int Id { get; }
        public int ProductId { get; }
        public int RearProductId { get; }
        public int ShelfId { get; }
        public int Slot { get; }
        public bool Wrapped { get; }
        public bool Hammer { get; }
        public bool CanTake { get; }

        public ItemPresentation( int id, int productId, int rearProductId = -1, bool wrapped = false, bool hammer = false, bool canTake = false, int shelfId = -1, int slot = -1 )
        {
            Id = id;
            ProductId = productId;
            RearProductId = rearProductId;
            Wrapped = wrapped;
            Hammer = hammer;
            CanTake = canTake;
            ShelfId = shelfId;
            Slot = slot;
        }
    }
}
