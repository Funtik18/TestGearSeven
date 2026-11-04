using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
public sealed class ProductDefinition
{
    public readonly int Id;
    public readonly string Name, Family, Shape;
    public readonly Color Color;
    public ProductDefinition(int id, string name, string family, string shape, string hex)
    { Id = id; Name = name; Family = family; Shape = shape; ColorUtility.TryParseHtmlString(hex, out Color); }
}
}
