using System;

namespace RosiesCornerShop.Core
{
    [ Serializable ]
    public sealed class ItemState
    {
        public int id, type, target;
        public bool wrapped, hammer;
        public ItemState back;
    }
}
