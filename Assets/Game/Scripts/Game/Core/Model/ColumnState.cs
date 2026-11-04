using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
[Serializable]
public sealed class ColumnState
{
    public List<ShelfState> shelves = new List<ShelfState>();
    public List<ItemState> queue = new List<ItemState>();
}
}
