using System;
using UnityEngine;
using UnityEngine.UI;

namespace Breachpoint.UI
{
    // Thin helpers for building uGUI hierarchies in code. Same builders run in the WebGL
    // runtime and in the headless screenshot harness, so captures match the real app.
    public static class UIFactory
    {
        public static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static Image Panel(Transform parent, Color color, string name = "Panel")
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        // Bordered card. border<=0 -> no outline. glow adds a soft cyan edge for "focused" cards.
        public static Image Card(Transform parent, Color fill, Color border, float border_px = 1f,
            bool glow = false, string name = "Card")
        {
            var img = Panel(parent, fill, name);
            if (border_px > 0)
            {
                var ol = img.gameObject.AddComponent<Outline>();
                ol.effectColor = border;
                ol.effectDistance = new Vector2(border_px, -border_px);
            }
            if (glow)
            {
                var sh = img.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.5f);
                sh.effectDistance = new Vector2(0, 0);
                // simulate spread with multiple? keep single soft outline
                var ol2 = img.gameObject.AddComponent<Outline>();
                ol2.effectColor = new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.35f);
                ol2.effectDistance = new Vector2(3, -3);
            }
            return img;
        }

        public static Text Label(Transform parent, string text, Font font, int size, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, FontStyle style = FontStyle.Normal, string name = "Label")
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.text = text; t.font = font; t.fontSize = size; t.color = color;
            t.alignment = anchor; t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            return t;
        }

        // Clickable rectangular button with label.
        public static Button Button(Transform parent, string text, Color bg, Color fg, Font font,
            int size, Action onClick = null, string name = "Button")
        {
            var img = Panel(parent, bg, name);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            // Guarantee a minimum height so the fill never collapses to 0 inside a layout group.
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.minHeight = Mathf.Round(size * 1.7f);
            var colors = btn.colors;
            colors.highlightedColor = new Color(1, 1, 1, 1f);
            colors.normalColor = Color.white;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
            var lbl = Label(img.transform, text, font, size, fg, TextAnchor.MiddleCenter, FontStyle.Bold, "Text");
            FillParent(lbl.rectTransform);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        // Horizontal stat bar: dim track + cyan fill (0..1). Returns the row root.
        public static RectTransform StatBar(Transform parent, string label, int value, Font labelFont, Font numFont)
        {
            var row = NewRect("Stat_" + label, parent);
            AddVLayout(row, 3, default, true, false);
            var head = NewRect("head", row);
            AddHLayout(head, 0, default, false, false, true);
            SetHeight(head, 18);
            var l = Label(head.transform, label, labelFont, 15, Theme.TextDim, TextAnchor.MiddleLeft);
            Flexible(l.gameObject, 1);
            var v = Label(head.transform, value + "/100", numFont, 13, Theme.Primary, TextAnchor.MiddleRight);
            SetWidth(v.gameObject, 70);

            var track = Panel(row, new Color(1, 1, 1, 0.06f), "track");
            SetHeight(track.gameObject, 5);
            var fill = Panel(track.transform, Theme.Primary, "fill");
            var frt = fill.rectTransform;
            frt.anchorMin = new Vector2(0, 0); frt.anchorMax = new Vector2(Mathf.Clamp01(value / 100f), 1);
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            return row;
        }

        // Small caps chip/tag.
        public static Image Chip(Transform parent, string text, Color textColor, Color bg, Font font, int size = 11)
        {
            var img = Panel(parent, bg, "Chip");
            AddHLayout(img.rectTransform, 0, new RectOffset(8, 8, 3, 3), false, false, true);
            AddFitter(img.gameObject, ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize);
            var l = Label(img.transform, text, font, size, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddFitter(l.gameObject, ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize);
            return img;
        }

        // ---------- layout helpers ----------
        public static void FillParent(RectTransform rt, float l = 0, float t = 0, float r = 0, float b = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
        }
        public static void FillParent(Graphic g, float l = 0, float t = 0, float r = 0, float b = 0) => FillParent(g.rectTransform, l, t, r, b);

        public static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        { rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot; }

        public static void SetSize(GameObject go, float w, float h) { var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(w, h); }
        public static void SetWidth(GameObject go, float w) { var le = Ensure<LayoutElement>(go); le.preferredWidth = w; le.minWidth = w; }
        public static void SetHeight(GameObject go, float h) { var le = Ensure<LayoutElement>(go); le.preferredHeight = h; le.minHeight = h; }
        public static void Flexible(GameObject go, float w = 1, float h = -1) { var le = Ensure<LayoutElement>(go); le.flexibleWidth = w; if (h >= 0) le.flexibleHeight = h; }

        // RectTransform convenience overloads
        public static void SetWidth(RectTransform rt, float w) => SetWidth(rt.gameObject, w);
        public static void SetHeight(RectTransform rt, float h) => SetHeight(rt.gameObject, h);
        public static void Flexible(RectTransform rt, float w = 1, float h = -1) => Flexible(rt.gameObject, w, h);

        public static VerticalLayoutGroup AddVLayout(RectTransform rt, float spacing, RectOffset pad,
            bool expandW = true, bool expandH = false, bool controlSize = true)
        {
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.padding = pad ?? new RectOffset();
            v.childForceExpandWidth = expandW; v.childForceExpandHeight = expandH;
            v.childControlWidth = controlSize; v.childControlHeight = controlSize;
            return v;
        }
        public static HorizontalLayoutGroup AddHLayout(RectTransform rt, float spacing, RectOffset pad,
            bool expandW = false, bool expandH = false, bool controlSize = true)
        {
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing; h.padding = pad ?? new RectOffset();
            h.childForceExpandWidth = expandW; h.childForceExpandHeight = expandH;
            h.childControlWidth = controlSize; h.childControlHeight = controlSize;
            return h;
        }
        public static GridLayoutGroup AddGrid(RectTransform rt, Vector2 cell, Vector2 spacing, int cols)
        {
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = cell; g.spacing = spacing;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = cols;
            return g;
        }
        public static ContentSizeFitter AddFitter(GameObject go, ContentSizeFitter.FitMode h, ContentSizeFitter.FitMode v)
        {
            var f = go.AddComponent<ContentSizeFitter>(); f.horizontalFit = h; f.verticalFit = v; return f;
        }

        static T Ensure<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
