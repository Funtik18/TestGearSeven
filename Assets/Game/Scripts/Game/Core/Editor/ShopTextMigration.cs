using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core.Editor
{
    /// <summary>Converts existing prefabs in place, retaining layouts, overrides and component file IDs.</summary>
    public static class ShopTextMigration
    {
        const string Prefabs = "Assets/Game/Assets/Prefabs";
        [MenuItem("Tools/Rosie's Corner Shop/Migrate text to TMP")]
        public static void Migrate()
        {
            TMP_FontAsset font = ShopFontUtility.GetFont(); int count = 0;
            var remaining = new HashSet<string>(AssetDatabase.FindAssets("t:Prefab",new[] { Prefabs }).Select(AssetDatabase.GUIDToAssetPath));
            while (remaining.Count > 0)
            {
                string path = remaining.FirstOrDefault(p => !AssetDatabase.GetDependencies(p,false).Any(d => d != p && remaining.Contains(d)));
                if (path == null) throw new InvalidOperationException("Cyclic prefab dependencies during TMP migration.");
                count += Convert(path,font); remaining.Remove(path);
            }
            AssetDatabase.SaveAssets(); Validate(); ShopPrefabBuilder.ValidateReferences();
            Debug.Log("TMP migration: PASS; converted " + count + " text components in existing prefabs.");
        }
        static int Convert(string path, TMP_FontAsset font)
        {
            string originalYaml = File.ReadAllText(path);
            var persistent = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var originalIds = new Dictionary<string,long>();
            foreach (Text text in persistent.GetComponentsInChildren<Text>(true))
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(text,out _,out long id);
                originalIds[AnimationUtility.CalculateTransformPath(text.transform,persistent.transform)] = id;
            }
            if (originalIds.Count == 0)
            {
                string fixedYaml = FixStrippedTypes(originalYaml);
                if (fixedYaml != originalYaml) { File.WriteAllText(path,fixedYaml); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate); }
                return 0;
            }
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (Text old in root.GetComponentsInChildren<Text>(true))
                {
                    GameObject obj = old.gameObject; string value = old.text; Color color = old.color; int size = old.fontSize;
                    FontStyle style = old.fontStyle; TextAnchor alignment = old.alignment;
                    bool raycast = old.raycastTarget, rich = old.supportRichText, auto = old.resizeTextForBestFit;
                    int min = old.resizeTextMinSize, max = old.resizeTextMaxSize;
                    HorizontalWrapMode wrap = old.horizontalOverflow; VerticalWrapMode overflow = old.verticalOverflow; bool enabled = old.enabled;
                    UnityEngine.Object.DestroyImmediate(old);
                    var tmp = obj.AddComponent<TextMeshProUGUI>(); tmp.font = font; tmp.text = value;
                    tmp.fontSize = size; tmp.color = color; tmp.raycastTarget = raycast; tmp.richText = rich; tmp.enabled = enabled;
                    tmp.fontStyle = style == FontStyle.Bold ? FontStyles.Bold : style == FontStyle.Italic ? FontStyles.Italic
                        : style == FontStyle.BoldAndItalic ? FontStyles.Bold | FontStyles.Italic : FontStyles.Normal;
                    TextAlignmentOptions[] alignments = { TextAlignmentOptions.TopLeft,TextAlignmentOptions.Top,TextAlignmentOptions.TopRight,
                        TextAlignmentOptions.Left,TextAlignmentOptions.Center,TextAlignmentOptions.Right,
                        TextAlignmentOptions.BottomLeft,TextAlignmentOptions.Bottom,TextAlignmentOptions.BottomRight };
                    tmp.alignment = alignments[(int)alignment]; tmp.margin = Vector4.zero;
                    tmp.enableAutoSizing = auto; tmp.fontSizeMin = min; tmp.fontSizeMax = max;
                    tmp.textWrappingMode = wrap == HorizontalWrapMode.Wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
                    tmp.overflowMode = overflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            persistent = AssetDatabase.LoadAssetAtPath<GameObject>(path); string convertedYaml = File.ReadAllText(path);
            // Retain the original document and replace only the text component bodies. This preserves
            // serialized references even when Unity has already deserialized an incompatible field as null.
            foreach (var tmp in persistent.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string hierarchy = AnimationUtility.CalculateTransformPath(tmp.transform,persistent.transform);
                if (!originalIds.TryGetValue(hierarchy,out long originalId)) continue;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tmp,out _,out long newId);
                string Pattern(long id) => @"^--- !u!114 &" + id + @"\r?\n.*?(?=^---|\z)";
                var block = Regex.Match(convertedYaml,Pattern(newId),RegexOptions.Multiline | RegexOptions.Singleline);
                if (!block.Success) throw new Exception("TMP component body not found in " + path);
                string replacement = Regex.Replace(block.Value,@"\A--- !u!114 &" + newId,"--- !u!114 &" + originalId);
                originalYaml = Regex.Replace(originalYaml,Pattern(originalId),_ => replacement,RegexOptions.Multiline | RegexOptions.Singleline);
            }
            File.WriteAllText(path,FixStrippedTypes(originalYaml)); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            return originalIds.Count;
        }
        static string FixStrippedTypes(string yaml) => yaml.Replace("guid: 5f7201a12d95ffc409449d95f23cf332", "guid: f4688fdb7df04437aeb418b961361dc5");
        [MenuItem("Tools/Rosie's Corner Shop/Validate TMP text")]
        public static void Validate()
        {
            int total = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab",new[] { Prefabs }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab.GetComponentsInChildren<Text>(true).Length > 0) throw new Exception("Legacy UI.Text remains in " + path);
                foreach (var text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    total++; if (text.font == null) throw new Exception("TMP font missing in " + path);
                    text.ForceMeshUpdate(true,true);
                    if (!text.font.HasCharacters("АБВабвЁё",out uint[] missing,true,true)) throw new Exception("Font lacks Cyrillic: " + string.Join(",",missing));
                }
            }
            Debug.Log("TMP validation: PASS; " + total + " TMP labels (including nested instances), no legacy UI.Text.");
        }
        public static void MigrateAndRun()
        {
            try { Migrate(); ShopPlayModeValidation.Run(); }
            catch (Exception exception) { Debug.LogException(exception); if (Application.isBatchMode) EditorApplication.Exit(1); else throw; }
        }
    }
}
