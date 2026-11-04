using UnityEngine;

namespace RosiesCornerShop.Core
{
    public static class ShopColors
    {
        public static readonly Color Cream = Hex("FFF8EA"), Ink = Hex("3B2A1B"), Wood = Hex("CD955A"),
            Dark = Hex("6B4320"), Berry = Hex("C63D4E"), Sage = Hex("A9BC96"), Green = Hex("5F9A46"), Gold = Hex("EDB53F");
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var result); return result; }
    }
}
