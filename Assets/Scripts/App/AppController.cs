using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.UI;

namespace Breachpoint.App
{
    public enum Screen { Play, Loadout, Armory, Store, BattlePass, WeaponDetail }

    // Builds the persistent shell (nav rail + top bar) and routes between screens by
    // rebuilding the content area. No scene wiring - everything is constructed in code.
    public class AppController : MonoBehaviour
    {
        RectTransform _screenArea;
        Text _credits, _gems, _levelBadge;
        readonly Dictionary<Screen, (Image bg, Text icon, Text label)> _nav = new();

        public Screen Current { get; private set; }
        public string SelectedWeaponId = "ar_phantom";   // context for WeaponDetail

        public void Build(RectTransform canvas)
        {
            // Background
            var bg = UIFactory.Panel(canvas, Theme.Surface, "Background");
            UIFactory.FillParent(bg);

            BuildNavRail(canvas);

            // Content root (right of nav rail)
            var content = UIFactory.NewRect("Content", canvas);
            content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(1, 1);
            content.offsetMin = new Vector2(256, 0); content.offsetMax = Vector2.zero;

            BuildTopBar(content);

            _screenArea = UIFactory.NewRect("ScreenArea", content);
            _screenArea.anchorMin = new Vector2(0, 0); _screenArea.anchorMax = new Vector2(1, 1);
            _screenArea.offsetMin = new Vector2(0, 0); _screenArea.offsetMax = new Vector2(0, -64);

            BuildScanlines(canvas);

            Navigate(Screen.Play);
        }

        void BuildNavRail(RectTransform canvas)
        {
            var rail = UIFactory.Panel(canvas, Theme.PanelLow, "NavRail");
            var rt = rail.rectTransform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f); rt.sizeDelta = new Vector2(256, 0);
            UIFactory.AddVLayout(rt, 6, new RectOffset(0, 0, 40, 32));
            var border = rail.gameObject.AddComponent<Outline>();
            border.effectColor = new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.15f);
            border.effectDistance = new Vector2(2, 0);

            // Logo
            var logoBox = UIFactory.NewRect("Logo", rt);
            UIFactory.AddVLayout(logoBox, 2, new RectOffset(24, 24, 0, 24));
            UIFactory.SetHeight(logoBox.gameObject, 64);
            UIFactory.Label(logoBox.transform, "BREACHPOINT", Theme.Display, 22, Theme.Primary, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Label(logoBox.transform, "SQUADRON_OS_V4", Theme.Mono, 10, Theme.TextDim, TextAnchor.MiddleLeft);

            AddNavItem(rt, Screen.Play, "▶", "PLAY");
            AddNavItem(rt, Screen.Loadout, "❖", "LOADOUT");
            AddNavItem(rt, Screen.Armory, "✦", "ARMORY");
            AddNavItem(rt, Screen.Store, "◈", "STORE");
            AddNavItem(rt, Screen.BattlePass, "✷", "BATTLE PASS");

            // spacer + deploy button
            var spacer = UIFactory.NewRect("spacer", rt); UIFactory.Flexible(spacer.gameObject, 1, 1);
            var deployWrap = UIFactory.NewRect("deployWrap", rt);
            UIFactory.AddHLayout(deployWrap, 0, new RectOffset(20, 20, 0, 0), true, false, true);
            UIFactory.SetHeight(deployWrap.gameObject, 52);
            var deploy = UIFactory.Button(deployWrap.transform, "DEPLOY", Theme.Primary, Color.black, Theme.Display, 18,
                () => Navigate(Screen.Play));
            UIFactory.Flexible(deploy.gameObject, 1);
        }

        void AddNavItem(RectTransform parent, Screen screen, string icon, string label)
        {
            var item = UIFactory.Panel(parent, new Color(0, 0, 0, 0), "Nav_" + screen);
            var btn = item.gameObject.AddComponent<Button>(); btn.targetGraphic = item;
            UIFactory.SetHeight(item.gameObject, 48);
            UIFactory.AddHLayout(item.rectTransform, 16, new RectOffset(24, 24, 0, 0), false, false, true);
            var ic = UIFactory.Label(item.transform, icon, Theme.Body, 18, Theme.TextDim, TextAnchor.MiddleCenter);
            UIFactory.SetWidth(ic.gameObject, 22);
            var lb = UIFactory.Label(item.transform, label, Theme.Mono, 13, Theme.TextDim, TextAnchor.MiddleLeft);
            UIFactory.Flexible(lb.gameObject, 1);
            btn.onClick.AddListener(() => Navigate(screen));
            _nav[screen] = (item, ic, lb);
        }

        void BuildTopBar(RectTransform content)
        {
            var bar = UIFactory.Panel(content, new Color(0, 0, 0, 0.25f), "TopBar");
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 64);
            var ul = bar.gameObject.AddComponent<Outline>();
            ul.effectColor = new Color(Theme.Outline.r, Theme.Outline.g, Theme.Outline.b, 0.4f);
            ul.effectDistance = new Vector2(0, -1);
            UIFactory.AddHLayout(rt, 24, new RectOffset(48, 48, 0, 0), false, false, true);

            UIFactory.Label(bar.transform, "TACTICAL_HUD_V2.0", Theme.Mono, 12, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            var spacer = UIFactory.NewRect("sp", rt); UIFactory.Flexible(spacer.gameObject, 1);

            _credits = Currency(bar.transform, "◉", Theme.Primary, MockDatabase.Player.Credits);
            _gems = Currency(bar.transform, "♦", Theme.Secondary, MockDatabase.Player.Gems);

            var badge = UIFactory.Card(bar.transform, Theme.PanelHigh, Theme.Primary, 1, false, "lvl");
            UIFactory.AddHLayout(badge.rectTransform, 6, new RectOffset(10, 10, 6, 6), false, false, true);
            UIFactory.AddFitter(badge.gameObject, ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.Unconstrained);
            UIFactory.Label(badge.transform, "LV", Theme.Mono, 11, Theme.TextDim, TextAnchor.MiddleCenter);
            _levelBadge = UIFactory.Label(badge.transform, MockDatabase.Player.Level.ToString(), Theme.Display, 16, Theme.Primary, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        Text Currency(Transform parent, string icon, Color color, int value)
        {
            var box = UIFactory.NewRect("cur", parent);
            UIFactory.AddHLayout(box, 6, default, false, false, true);
            UIFactory.AddFitter(box.gameObject, ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.Unconstrained);
            UIFactory.Label(box.transform, icon, Theme.Body, 16, color, TextAnchor.MiddleCenter);
            var t = UIFactory.Label(box.transform, $"{value:N0}", Theme.Mono, 14, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.SetWidth(t.gameObject, 64);
            return t;
        }

        void BuildScanlines(RectTransform canvas)
        {
            var s = UIFactory.NewRect("Scanlines", canvas);
            UIFactory.FillParent(s);
            var img = s.gameObject.AddComponent<RawImage>();
            img.texture = ScanlineTexture();
            img.color = new Color(1, 1, 1, 0.05f);
            img.uvRect = new Rect(0, 0, 1, 450);   // tile vertically
            img.raycastTarget = false;
        }

        static Texture2D _scan;
        static Texture2D ScanlineTexture()
        {
            if (_scan != null) return _scan;
            _scan = new Texture2D(1, 4, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            _scan.SetPixels(new[] { new Color(0, 0, 0, 0), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0.7f), new Color(0, 0, 0, 0.7f) });
            _scan.Apply();
            return _scan;
        }

        public void Navigate(Screen screen)
        {
            Current = screen;
            for (int i = _screenArea.childCount - 1; i >= 0; i--)
            {
                var go = _screenArea.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
            }

            // re-tint nav
            foreach (var kv in _nav)
            {
                bool active = kv.Key == screen;
                kv.Value.bg.color = active ? Theme.PrimaryDim : new Color(0, 0, 0, 0);
                kv.Value.icon.color = active ? Theme.Primary : Theme.TextDim;
                kv.Value.label.color = active ? Theme.Primary : Theme.TextDim;
            }

            switch (screen)
            {
                case Screen.Play: PlayMenuScreen.Build(_screenArea, this); break;
                case Screen.Loadout: LoadoutScreen.Build(_screenArea, this); break;
                case Screen.Armory: ArmoryScreen.Build(_screenArea, this); break;
                case Screen.Store: StoreScreen.Build(_screenArea, this); break;
                case Screen.BattlePass: BattlePassScreen.Build(_screenArea, this); break;
                case Screen.WeaponDetail: WeaponDetailScreen.Build(_screenArea, this); break;
            }
        }

        public void RefreshCurrencies()
        {
            _credits.text = $"{MockDatabase.Player.Credits:N0}";
            _gems.text = $"{MockDatabase.Player.Gems:N0}";
        }
    }
}
