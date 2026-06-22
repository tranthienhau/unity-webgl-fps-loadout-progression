using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    public static class ArmoryScreen
    {
        public static void Build(RectTransform area, AppController app)
        {
            var root = ScreenUtil.Root(area);
            ScreenUtil.Header(root, "ARMORY", MockDatabase.Weapons.Count + " WEAPONS // TAP TO INSPECT & CUSTOMIZE");

            var scroll = UIFactory.NewRect("Grid", root);
            UIFactory.Flexible(scroll.gameObject, 1, 1);
            UIFactory.AddGrid(scroll, new Vector2(290, 150), new Vector2(20, 20), 4);

            foreach (var w in MockDatabase.Weapons) WeaponCard(scroll, w, app);
        }

        static void WeaponCard(RectTransform parent, Weapon w, AppController app)
        {
            int owned = MockDatabase.SkinsFor(w.Id).Count(s => s.Owned);
            var best = MockDatabase.SkinsFor(w.Id).OrderByDescending(s => (int)s.Rarity).FirstOrDefault();
            var rarityCol = best != null ? Theme.Rarity(best.Rarity) : Theme.Outline;

            var card = UIFactory.Card(parent, Theme.Panel, Theme.Outline, 1, false, "W_" + w.Id);
            var btn = card.gameObject.AddComponent<Button>(); btn.targetGraphic = card;
            btn.onClick.AddListener(() => { app.SelectedWeaponId = w.Id; app.Navigate(Screen.WeaponDetail); });

            // rarity top bar
            var bar = UIFactory.Panel(card.transform, rarityCol, "bar");
            var brt = bar.rectTransform; brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(0.5f, 1); brt.sizeDelta = new Vector2(0, 3);

            UIFactory.AddVLayout(card.rectTransform, 4, new RectOffset(16, 16, 16, 14), true, false, true);
            var topRow = UIFactory.NewRect("tr", card.rectTransform);
            UIFactory.AddHLayout(topRow, 12, default, false, false, true);
            ScreenUtil.Thumb(topRow.transform, rarityCol, "⌖", 52);
            var meta = UIFactory.NewRect("m", topRow); UIFactory.Flexible(meta.gameObject, 1, 1);
            UIFactory.AddVLayout(meta, 2, new RectOffset(0, 0, 4, 0), true, false, true);
            UIFactory.Label(meta.transform, w.Name, Theme.Display, 16, Theme.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIFactory.Label(meta.transform, Pretty(w.Class), Theme.Mono, 10, Theme.TextDim, TextAnchor.UpperLeft);
            var unlock = w.UnlockLevel <= MockDatabase.Player.Level;
            UIFactory.Label(meta.transform, unlock ? "UNLOCKED" : "LVL " + w.UnlockLevel, Theme.Mono, 10, unlock ? Theme.Primary : Theme.Secondary, TextAnchor.UpperLeft);

            var sp = UIFactory.NewRect("sp", card.rectTransform); UIFactory.Flexible(sp.gameObject, 1, 1);
            var foot = UIFactory.NewRect("f", card.rectTransform);
            UIFactory.AddHLayout(foot, 8, default, false, false, true);
            UIFactory.Label(foot.transform, "PWR " + Power(w), Theme.Display, 14, Theme.Primary, TextAnchor.MiddleLeft, FontStyle.Bold);
            var s = UIFactory.NewRect("s", foot); UIFactory.Flexible(s.gameObject, 1);
            UIFactory.Label(foot.transform, owned + " SKINS", Theme.Mono, 10, Theme.TextDim, TextAnchor.MiddleRight);
        }

        static int Power(Weapon w)
        {
            var s = w.Stats;
            return Mathf.RoundToInt((s.Damage + s.FireRate + s.Range + s.Accuracy + s.Mobility + s.Control) / 6f);
        }

        static string Pretty(WeaponClass c) => c switch
        {
            WeaponClass.AssaultRifle => "ASSAULT RIFLE",
            WeaponClass.SMG => "SMG",
            WeaponClass.LMG => "LMG",
            _ => c.ToString().ToUpper()
        };
    }
}
