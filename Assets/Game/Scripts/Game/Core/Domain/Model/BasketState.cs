using System;
using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    [ Serializable ]
    public sealed class BasketState
    {
        public int id, type;
        public List< ItemState > items = new List< ItemState >();
        public int count => items.Count;
        public float readyAt;

        public void Add( ItemState item )
        {
            items.Add( new ItemState { id = item.id, type = item.type } );
        }
    }
}
