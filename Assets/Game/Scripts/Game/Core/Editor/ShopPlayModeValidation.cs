using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core.Editor
{
    /// <summary>Opt-in batch smoke test; exercises real prefab instances and running DOTween sequences.</summary>
    public static class ShopPlayModeValidation
    {
        static double next, timeout;
        static int stage;
        static ShopApplication app;
        static string output;
        static bool failed;
        public static void RebuildAndRun() { ShopPrefabBuilder.Rebuild(); Run(); }
        public static void Run()
        {
            output = Path.GetFullPath(Path.Combine(Application.dataPath,"../../RosieSmoke")); Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output,"errors.txt"),"");
            // Survives the domain reload caused by entering Play mode.
            SessionState.SetString("Rosie.SmokeOutput",output); SessionState.SetBool("Rosie.Smoke",true);
            EditorApplication.isPlaying = true;
        }
        [InitializeOnLoadMethod]
        static void Resume()
        {
            if (!SessionState.GetBool("Rosie.Smoke",false)) return;
            output = SessionState.GetString("Rosie.SmokeOutput","");
            stage = 0; failed = false; timeout = EditorApplication.timeSinceStartup + 70;
            Application.logMessageReceived += Log;
            EditorApplication.update += Step;
        }
        static void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            // Unity 6's initial Search index occasionally throws before the game is started.
            if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            failed = true; File.AppendAllText(Path.Combine(output,"errors.txt"),message + "\n" + trace + "\n");
        }
        static void Step()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > timeout) throw new Exception("Play mode smoke test timed out at stage " + stage);
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
                app = UnityEngine.Object.FindAnyObjectByType<ShopApplication>(); if (app == null) return;
                switch (stage++)
                {
                    case 0:
                        Capture("01-home.png"); Wait(.8); break;
                    case 1:
                        FindButton("Play").onClick.Invoke(); Wait(.6); break;
                    case 2:
                        FindButton("Action0").onClick.Invoke(); Wait(.6); break;
                    case 3:
                        Capture("02-game.png"); TakeOne(); Wait(.8); break;
                    case 4:
                        TakeOne(); Wait(.8); break;
                    case 5:
                        TakeOne(); Wait(1.4); break;
                    case 6:
                        if (app.Session.State.collected < 3) throw new Exception("The completed basket was not shipped");
                        Capture("03-packed.png");
                        // Restart during active flights, then build the full collection.
                        app.StartLevel(17); Wait(.15); break;
                    case 7:
                        TakeOne(); app.StartLevel(12); TakeOne(); app.ShowCollection(); Wait(.7); break;
                    case 8:
                        Capture("04-collection.png"); app.ShowHome(); app.StartLevel(10); Wait(.7); break;
                    case 9:
                        Capture("05-mechanic.png");
                        if (failed) throw new Exception("Runtime errors were logged; see errors.txt");
                        File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: real prefabs, home/play/tutorial input, item flights, basket packing, restarts during tweens, collection and mechanic window.");
                        Finish(0); break;
                }
            }
            catch (Exception exception)
            { File.WriteAllText(Path.Combine(output,"result.txt"),"FAIL: " + exception); Finish(1); }
        }
        static void Wait(double duration) { next = EditorApplication.timeSinceStartup + duration; }
        static Button FindButton(string name)
        { return app.GetComponentsInChildren<Button>().First(b => b.name == name && b.interactable); }
        static void TakeOne()
        {
            var state = app.Session.State;
            var choices = state.columns.SelectMany(c => c.shelves).Where(b => b.Open)
                .SelectMany(b => b.slots.Select((j,i) => new { b,j,i })).Where(p => p.j != null).ToList();
            var choice = choices.Where(p => state.baskets.Any(b => b != null && b.count < 3 && b.type == p.j.type))
                .OrderByDescending(p => state.baskets.First(b => b != null && b.count < 3 && b.type == p.j.type).count).FirstOrDefault() ?? choices.First();
            app.Take(choice.b,choice.i);
        }
        static void Capture(string name)
        {
            // Batch editors do not present a Game View; render the real Canvas through a temporary camera.
            var cameraObject = new GameObject("Smoke capture camera",typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 450;
            camera.transform.position = new Vector3(0,0,-10); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = ShopColors.Dark;
            var target = new RenderTexture(430,900,24); camera.targetTexture = target;
            Canvas canvas = app.GetComponent<Canvas>(); RenderMode originalMode = canvas.renderMode; Camera originalCamera = canvas.worldCamera;
            Transform safe = app.transform.Find("SafeArea"); Vector3 originalScale = safe.localScale;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            safe.localScale = Vector3.one; Canvas.ForceUpdateCanvases(); camera.Render();
            RenderTexture previous = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(430,900,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,430,900),0,0); image.Apply();
            File.WriteAllBytes(Path.Combine(output,name),image.EncodeToPNG()); RenderTexture.active = previous;
            canvas.renderMode = originalMode; canvas.worldCamera = originalCamera; safe.localScale = originalScale;
            camera.targetTexture = null; UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(cameraObject);
        }
        static void Finish(int exitCode)
        {
            SessionState.SetBool("Rosie.Smoke",false); Application.logMessageReceived -= Log; EditorApplication.update -= Step;
            EditorApplication.Exit(exitCode);
        }
    }
}
