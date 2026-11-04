using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    [Serializable] public sealed class GameState
    {
        public int level, jps, visible, total, collected, moves, peak, nextId, capacity = 5, unlocked = 2, freezes;
        public uint random;
        public List<ColumnState> columns = new List<ColumnState>();
        public List<int> orders = new List<int>();
        public List<ItemState> counter = new List<ItemState>();
        public BasketState[] baskets = new BasketState[3];
    }

}
