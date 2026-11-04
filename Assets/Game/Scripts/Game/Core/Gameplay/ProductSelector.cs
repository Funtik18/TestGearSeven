using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public static class ProductSelector
    {
        public static List<int> Pick(LevelDefinition level, GameProgress progress)
        {
            var random = new SeededRandom(unchecked((uint)(level.seed * 31 + 7)));
            var families = ProductCatalog.All.GroupBy(p => p.Family).ToDictionary(g => g.Key,g => g.ToList());
            foreach (var list in families.Values) random.Shuffle(list);
            var order = new List<string> { "dairy", "bakery", "drink", "pantry", "sweet", "fresh", "home" }; random.Shuffle(order);
            var result = new List<int>(); var shapes = new HashSet<string>(); var colors = new HashSet<string>();
            bool Allowed(ProductDefinition p) => !shapes.Contains(p.Shape) && !colors.Contains(Bucket(p.Color));
            void Take(ProductDefinition p) { result.Add(p.Id); shapes.Add(p.Shape); colors.Add(Bucket(p.Color)); families[p.Family].Remove(p); }
            if (!string.IsNullOrEmpty(level.family)) foreach (var p in families[level.family].ToArray())
                if (Allowed(p)) { Take(p); if (result.Count >= 3) break; }
            for (int i = 0; result.Count < level.types && i < 5000; i++)
            {
                var list = families[order[i % order.Count]];
                var product = list.FirstOrDefault(p => Allowed(p) && !progress.found[p.Id]) ?? list.FirstOrDefault(Allowed);
                if (product != null) Take(product);
            }
            foreach (var product in ProductCatalog.All)
            { if (result.Count >= level.types) break; if (shapes.Add(product.Shape)) result.Add(product.Id); }
            return result;
        }
        static string Bucket(Color c)
        {
            float max = Mathf.Max(c.r,c.g,c.b), min = Mathf.Min(c.r,c.g,c.b), light = (max + min) / 2, d = max - min;
            float saturation = d == 0 ? 0 : d / (1 - Mathf.Abs(2 * light - 1));
            if (saturation < .22f || light > .9f) return light > .6f ? "white" : light < .3f ? "black" : "grey";
            Color.RGBToHSV(c,out float hue,out _,out _); hue *= 360;
            if (hue >= 15 && hue < 45 && light < .42f) return "brown";
            return hue < 15 || hue >= 345 ? "red" : hue < 45 ? "orange" : hue < 70 ? "yellow" : hue < 160 ? "green"
                : hue < 200 ? "teal" : hue < 250 ? "blue" : hue < 290 ? "purple" : "pink";
        }
    }
}
