using UnityEngine;
using Breachpoint.Data;

namespace Breachpoint.UI
{
    // Central design tokens - mirrors design/DESIGN.md ("Neo-Tactical Protocol").
    public static class Theme
    {
        public static readonly Color Surface     = Hex("0E1116");
        public static readonly Color Panel        = Hex("1A1E26");
        public static readonly Color PanelLow     = Hex("141821");
        public static readonly Color PanelHigh    = Hex("232833");
        public static readonly Color Primary      = Hex("27E1FF"); // cyan
        public static readonly Color Secondary    = Hex("FF6A2C"); // orange
        public static readonly Color Text         = Hex("E1E2E9");
        public static readonly Color TextDim       = Hex("8593A0");
        public static readonly Color Outline      = Hex("3B494C");
        public static readonly Color PrimaryDim   = new(0.153f, 0.882f, 1f, 0.12f);

        public static Color Rarity(Rarity r) => r switch
        {
            Data.Rarity.Common    => Hex("9AA3A8"),
            Data.Rarity.Rare      => Hex("27A8FF"),
            Data.Rarity.Epic      => Hex("A35BFF"),
            Data.Rarity.Legendary => Hex("FF6A2C"),
            _ => Text
        };

        // --- Fonts (loaded from Resources/Fonts) ---
        static Font _display, _heading, _body, _mono;
        public static Font Display => _display ??= Load("Orbitron");          // titles
        public static Font Heading => _heading ??= Load("Rajdhani-SemiBold"); // section heads
        public static Font Body    => _body    ??= Load("Rajdhani-Medium");   // body/labels
        public static Font Mono    => _mono    ??= Load("JetBrainsMono");     // numbers/serials

        static Font Load(string name)
        {
            var f = Resources.Load<Font>("Fonts/" + name);
            return f != null ? f : Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
