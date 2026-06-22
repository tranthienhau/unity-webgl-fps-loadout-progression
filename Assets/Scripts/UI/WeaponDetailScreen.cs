using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    // Skin selector + stat readout for one weapon. Reached from Loadout "Customize" or Armory.
    public static class WeaponDetailScreen
    {
        public static void Build(RectTransform area, AppController app)
        {
            var w = MockDatabase.Weapons.FirstOrDefault(x => x.Id == app.SelectedWeaponId) ?? MockDatabase.EquippedPrimary;
            var root = ScreenUtil.Root(area, 16);

            // Header with back
            var hrow = UIFactory.NewRect("HRow", root);
            UIFactory.SetHeight(hrow.gameObject, 56);
            UIFactory.AddHLayout(hrow, 16, default, false, false, true);
            var back = UIFactory.Button(hrow.transform, "‹ BACK", new Color(0, 0, 0, 0), Theme.Primary, Theme.Display, 16, () => app.Navigate(Screen.Armory));
            var bo = back.gameObject.AddComponent<Outline>(); bo.effectColor = Theme.Primary; bo.effectDistance = new Vector2(1, -1);
            UIFactory.SetWidth(back.gameObject, 110);
            UIFactory.AddHLayout(back.GetComponent<RectTransform>(), 0, new RectOffset(12, 12, 0, 0), false, false, true);
            var hb = UIFactory.NewRect("hb", hrow);
            UIFactory.Flexible(hb.gameObject, 1, 1);
            UIFactory.AddVLayout(hb, 2, default, true, false, true);
            UIFactory.Label(hb.transform, w.Name, Theme.Display, 30, Theme.Text, TextAnchor.LowerLeft, FontStyle.Bold);
            UIFactory.Label(hb.transform, "CUSTOMIZE // SELECT WEAPON SKIN", Theme.Body, 13, Theme.TextDim, TextAnchor.UpperLeft);

            var body = UIFactory.NewRect("Body", root);
            UIFactory.Flexible(body.gameObject, 1, 1);
            UIFactory.AddHLayout(body, 24, default, false, true, true);

            // Left preview + stats
            var left = UIFactory.NewRect("Left", body);
            UIFactory.Flexible(left.gameObject, 1.3f, 1);
            UIFactory.AddVLayout(left, 16, default, true, false, true);
            var preview = UIFactory.Card(left, Theme.Panel, Theme.Outline, 1);
            UIFactory.Flexible(preview.gameObject, 1.4f, 1);
            UIFactory.AddVLayout(preview.rectTransform, 0, default, true, true, true);
            UIFactory.Label(preview.transform, "⌖", Theme.Display, 130, new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.5f), TextAnchor.MiddleCenter);
            var specs = UIFactory.Card(left, Theme.PanelLow, Theme.Outline, 1);
            UIFactory.AddVLayout(specs.rectTransform, 10, new RectOffset(22, 22, 16, 16), true, false, true);
            UIFactory.Label(specs.transform, "WEAPON SPECS", Theme.Display, 15, Theme.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            var grid = UIFactory.NewRect("g", specs.rectTransform);
            UIFactory.AddGrid(grid, new Vector2(190, 30), new Vector2(16, 8), 2);
            UIFactory.SetHeight(grid.gameObject, 100);
            foreach (var (label, value) in w.Stats.Enumerate())
                UIFactory.StatBar(grid, label, value, Theme.Body, Theme.Mono);

            // Right skin list
            var right = UIFactory.NewRect("Right", body);
            UIFactory.Flexible(right.gameObject, 1, 1);
            UIFactory.AddVLayout(right, 10, default, true, false, true);
            UIFactory.Label(right.transform, "SKINS", Theme.Display, 16, Theme.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            foreach (var s in MockDatabase.SkinsFor(w.Id)) SkinRow(right, s, w, app);
        }

        static void SkinRow(RectTransform parent, Skin s, Weapon w, AppController app)
        {
            var col = Theme.Rarity(s.Rarity);
            bool equipped = w.EquippedSkinId == s.Id;
            var card = UIFactory.Card(parent, equipped ? Theme.PanelHigh : Theme.Panel, equipped ? Theme.Primary : Theme.Outline, equipped ? 2 : 1, equipped);
            UIFactory.SetHeight(card.gameObject, 72);
            UIFactory.AddHLayout(card.rectTransform, 14, new RectOffset(14, 16, 12, 12), false, false, true);
            ScreenUtil.Thumb(card.transform, col, "⌖", 46);
            var info = UIFactory.NewRect("i", card.rectTransform);
            UIFactory.Flexible(info.gameObject, 1, 1);
            UIFactory.AddVLayout(info, 3, new RectOffset(0, 0, 4, 4), true, false, true);
            UIFactory.Label(info.transform, s.Name, Theme.Display, 15, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Label(info.transform, s.Rarity.ToString().ToUpper() + (s.Owned ? "  -  OWNED" : ""), Theme.Mono, 10, col, TextAnchor.MiddleLeft);

            if (equipped)
            {
                var chip = UIFactory.Chip(card.transform, "EQUIPPED", Theme.Primary, Theme.PrimaryDim, Theme.Mono, 11);
            }
            else if (s.Owned)
            {
                var eq = UIFactory.Button(card.transform, "EQUIP", Theme.Primary, Color.black, Theme.Display, 13,
                    () => { w.EquippedSkinId = s.Id; app.Navigate(Screen.WeaponDetail); });
                UIFactory.SetWidth(eq.gameObject, 92);
            }
            else
            {
                var buy = UIFactory.Button(card.transform, "♦ " + s.PriceGems, Theme.PanelHigh, Theme.Primary, Theme.Mono, 12,
                    () => { if (MockDatabase.Player.Gems >= s.PriceGems) { MockDatabase.Player.Gems -= s.PriceGems; s.Owned = true; app.RefreshCurrencies(); app.Navigate(Screen.WeaponDetail); } });
                var o = buy.gameObject.AddComponent<Outline>(); o.effectColor = Theme.Primary; o.effectDistance = new Vector2(1, -1);
                UIFactory.SetWidth(buy.gameObject, 92);
            }
        }
    }
}
