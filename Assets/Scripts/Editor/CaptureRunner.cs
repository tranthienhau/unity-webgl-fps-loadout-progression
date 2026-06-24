#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.App;
using Breachpoint.Data;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.EditorTools
{
    // Headless screenshot harness. Builds the UI in edit mode, renders each screen through a
    // camera -> RenderTexture -> PNG. No display required (runs in -batchmode with graphics).
    // Same construction code path as the WebGL runtime, so captures match the shipped app.
    public static class CaptureRunner
    {
        const int W = 1600, H = 900;

        struct Step { public string screen; public string named; public Action mutate; }

        public static void Run()
        {
            string outDir = ArgOr("-captureOut", Path.GetFullPath(Path.Combine(Application.dataPath, "..", "screenshots")));
            string framesDir = Path.Combine(outDir, "frames");
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(framesDir);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var app = GameBootstrap.Construct(out var cam, out var canvasRect);

            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            rt.Create();
            cam.targetTexture = rt;
            cam.pixelRect = new Rect(0, 0, W, H);

            var w = MockDatabase.Weapons.Find(x => x.Id == "ar_phantom");

            var steps = new List<Step>
            {
                new() { screen = "Play",        named = "01-play" },
                new() { screen = "Loadout",     named = "02-loadout" },
                new() { screen = "Armory",      named = "03-armory" },
                new() { screen = "WeaponDetail",named = "04-weapon-detail", mutate = () => app.SelectedWeaponId = "ar_phantom" },
                // action: equip a different owned skin (skin swap)
                new() { screen = "WeaponDetail",named = null, mutate = () => w.EquippedSkinId = "sk_phantom_carbon" },
                new() { screen = "Store",       named = "05-store" },
                // action: spend gems
                new() { screen = "Store",       named = null, mutate = () => { MockDatabase.Player.Gems -= 1800; app.RefreshCurrencies(); } },
                new() { screen = "BattlePass",  named = "06-battlepass" },
                // action: unlock premium track
                new() { screen = "BattlePass",  named = null, mutate = () => MockDatabase.Player.PremiumPass = true },
            };

            int frame = 0;
            foreach (var s in steps)
            {
                s.mutate?.Invoke();
                app.Navigate((Screen)Enum.Parse(typeof(Screen), s.screen));
                RenderToFile(cam, rt, canvasRect, Path.Combine(framesDir, $"frame_{frame:D2}.png"));
                if (!string.IsNullOrEmpty(s.named))
                    File.Copy(Path.Combine(framesDir, $"frame_{frame:D2}.png"), Path.Combine(outDir, s.named + ".png"), true);
                Debug.Log($"[Capture] {s.screen} -> frame {frame} {(s.named ?? "")}");
                frame++;
            }

            cam.targetTexture = null;
            rt.Release();
            Debug.Log($"[Capture] wrote {frame} frames + named screenshots to {outDir}");
            EditorApplication.Exit(0);
        }

        static void RenderToFile(Camera cam, RenderTexture rt, RectTransform canvasRect, string path)
        {
            // Pre-warm dynamic font glyphs so the first render isn't blank.
            Canvas.ForceUpdateCanvases();
            foreach (var t in canvasRect.GetComponentsInChildren<Text>(true))
            {
                if (t.font != null && !string.IsNullOrEmpty(t.text))
                    t.font.RequestCharactersInTexture(t.text, t.fontSize, t.fontStyle);
                t.SetAllDirty();
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRect);
            Canvas.ForceUpdateCanvases();

            // Render twice: first pass triggers font-texture rebuild, second pass draws final glyphs.
            cam.Render();
            Canvas.ForceUpdateCanvases();
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static string ArgOr(string flag, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == flag) return args[i + 1];
            return fallback;
        }
    }
}
#endif
