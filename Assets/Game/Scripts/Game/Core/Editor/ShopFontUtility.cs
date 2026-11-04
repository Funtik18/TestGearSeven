using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace RosiesCornerShop.Core.Editor
{
    public static class ShopFontUtility
    {
        public const string FontPath = "Assets/Game/Assets/Fonts/ShopUI SDF.asset";
        public static TMP_FontAsset GetFont()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (asset != null) { EnsureSymbols(asset); return asset; }
            Font source = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            if (source == null) throw new InvalidOperationException("Import TMP Essential Resources before creating the shop font.");
            Directory.CreateDirectory(Path.GetDirectoryName(FontPath)); AssetDatabase.Refresh();
            asset = TMP_FontAsset.CreateFontAsset(source,48,6,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            asset.name = "ShopUI SDF";
            var settings = new SerializedObject(asset); settings.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false; settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(asset,FontPath);
            asset.material.name = "ShopUI SDF Material"; AssetDatabase.AddObjectToAsset(asset.material,asset);
            string characters = string.Concat(Enumerable.Range(32,95).Select(i => (char)i))
                + string.Concat(Enumerable.Range(0x410,64).Select(i => (char)i)) + "Ёё·—–×";
            asset.TryAddCharacters(characters,out string missing);
            if (!string.IsNullOrEmpty(missing)) Debug.LogWarning("Shop font has no glyphs for: " + missing);
            foreach (var atlas in asset.atlasTextures)
            { atlas.name = "ShopUI SDF Atlas"; if (!AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas,asset); EditorUtility.SetDirty(atlas); }
            EnsureSymbols(asset); EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets(); return asset;
        }
        static void EnsureSymbols(TMP_FontAsset asset)
        {
            if (asset.HasCharacters("★",out uint[] _,true,true)) return;
            const string path = "Assets/Game/Assets/Fonts/ShopSymbols SDF.asset";
            var symbols = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (symbols == null)
            {
                symbols = TMP_FontAsset.CreateFontAsset("Segoe UI Symbol","Regular",48);
                if (symbols == null || !symbols.TryAddCharacters("★")) throw new InvalidOperationException("Cannot bake the shop star symbol.");
                symbols.name = "ShopSymbols SDF"; symbols.atlasPopulationMode = AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(symbols,path); AssetDatabase.AddObjectToAsset(symbols.material,symbols);
                foreach (var atlas in symbols.atlasTextures) AssetDatabase.AddObjectToAsset(atlas,symbols);
            }
            if (asset.fallbackFontAssetTable == null) asset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            asset.fallbackFontAssetTable.Add(symbols); EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
        }
    }
}
