using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
[Serializable]
public sealed class GameProgress
{
    public int coins = 150;
    public int[] stars = new int[20];
    public bool[] found = new bool[ProductCatalog.All.Length];
    public int[] boosters = new int[3];
    public bool[] gifts = new bool[4];
    public bool sound = true;
}
}
