using UnityEngine;
using UnityEngine.UI;

namespace Breachpoint.UI
{
    public static class ScreenUtil
    {
        // Padded vertical root that fills the screen area.
        public static RectTransform Root(RectTransform area, float spacing = 20)
        {
            var root = UIFactory.NewRect("ScreenRoot", area);
            UIFactory.FillParent(root);
            UIFactory.AddVLayout(root, spacing, new RectOffset(48, 48, 32, 32), true, false, true);
            return root;
        }

        // Title + subtitle header row, fixed height.
        public static void Header(RectTransform root, string title, string subtitle)
        {
            var box = UIFactory.NewRect("Header", root);
            UIFactory.AddVLayout(box, 2, default, true, false, true);
            UIFactory.SetHeight(box.gameObject, 56);
            UIFactory.Label(box.transform, title, Theme.Display, 34, Theme.Text, TextAnchor.LowerLeft, FontStyle.Bold);
            UIFactory.Label(box.transform, subtitle, Theme.Body, 14, Theme.TextDim, TextAnchor.UpperLeft);
        }

        // Small uppercase caption label.
        public static Text Caption(Transform parent, string text)
            => UIFactory.Label(parent, text, Theme.Mono, 10, Theme.TextDim, TextAnchor.MiddleLeft);

        // Colored square "thumbnail" placeholder with a glyph (stands in for art).
        public static Image Thumb(Transform parent, Color tint, string glyph, float size)
        {
            var img = UIFactory.Card(parent, new Color(tint.r, tint.g, tint.b, 0.12f), tint, 1, false, "Thumb");
            UIFactory.SetWidth(img.gameObject, size); UIFactory.SetHeight(img.gameObject, size);
            var g = UIFactory.Label(img.transform, glyph, Theme.Body, Mathf.RoundToInt(size * 0.4f), tint, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.FillParent(g);
            return img;
        }
    }
}
