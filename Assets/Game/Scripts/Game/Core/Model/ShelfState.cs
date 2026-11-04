using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
[Serializable]
public sealed class ShelfState
{
    public int id, height, key = -1, frozen;
    public bool locked, boarded;
    public List<ItemState> slots = new List<ItemState>();
    public bool Open => !locked && !boarded && frozen == 0;
}
}
