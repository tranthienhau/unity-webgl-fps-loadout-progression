using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    public static class StoreScreen
    {
        static string _category = "ALL";
        static readonly string[] Cats = { "ALL", "WEAPON SKIN", "OPERATOR", "CHARM", "BUNDLE" };

        public static void Build(RectTransform area, AppController app)
        {
            var root = ScreenUtil.Root(area, 16);
            ScreenUtil.Header(root, "STORE", "PREMIUM COSMETICS // RESETS DAILY");

            // Category tabs
            var tabs = UIFactory.NewRect("Tabs", root);
            UIFactory.SetHeight(tabs.gameObject, 36);
            UIFactory.AddHLayout(tabs, 10, default, false, false, true);
            foreach (var c in Cats) Tab(tabs, c, app);

            var body = UIFactory.NewRect("Body", root);
            UIFactory.Flexible(body.gameObject, 1, 1);
            UIFactory.AddHLayout(body, 20, default, false, true, true);

            // Featured
            var featured = MockDatabase.Store.First(s => s.Featured);
            FeaturedHero(body, featured, app);

            // Grid
            var grid = UIFactory.NewRect("Grid", body);
            UIFactory.Flexible(grid.gameObject, 2, 1);
            UIFactory.AddGrid(grid, new Vector2(214, 184), new Vector2(16, 16), 3);
            foreach (var item in MockDatabase.Store.Where(Match)) ItemCard(grid, item, app);
        }

        static bool Match(StoreItem i) => _category == "ALL" || i.Category == _category || (_category == "BUNDLE" && i.Category == "BUNDLE");

        static void Tab(RectTransform parent, string cat, AppController app)
        {
            bool sel = cat == _category;
            var t = UIFactory.Button(parent, cat, sel ? Theme.PrimaryDim : new Color(0, 0, 0, 0),
                sel ? Theme.Primary : Theme.TextDim, Theme.Mono, 12, () => { _category = cat; app.Navigate(Screen.Store); });
            UIFactory.AddFitter(t.gameObject, ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.Unconstrained);
            UIFactory.AddHLayout(t.GetComponent<RectTransform>(), 0, new RectOffset(16, 16, 0, 0), false, false, true);
            if (sel) { var o = t.gameObject.AddComponent<Outline>(); o.effectColor = Theme.Primary; o.effectDistance = new Vector2(1, -1); }
        }

        static void FeaturedHero(RectTransform parent, StoreItem item, AppController app)
        {
            var col = Theme.Rarity(item.Rarity);
            var hero = UIFactory.Card(parent, Theme.Panel, col, 2, true, "Featured");
            UIFactory.Flexible(hero.gameObject, 1.3f, 1);
            UIFactory.AddVLayout(hero.rectTransform, 10, new RectOffset(26, 26, 24, 24), true, false, true);
            var tagRow = UIFactory.NewRect("tr", hero.rectTransform);
            UIFactory.AddHLayout(tagRow, 8, default, false, false, true);
            UIFactory.Chip(tagRow.transform, "FEATURED", Theme.Secondary, new Color(col.r, col.g, col.b, 0.18f), Theme.Mono);
            UIFactory.Chip(tagRow.transform, item.Rarity.ToString().ToUpper(), col, new Color(col.r, col.g, col.b, 0.18f), Theme.Mono);
            var big = UIFactory.NewRect("big", hero.rectTransform); UIFactory.Flexible(big.gameObject, 1, 1);
            UIFactory.AddVLayout(big, 0, default, true, true, true);
            UIFactory.Label(big.transform, "⌖", Theme.Display, 120, new Color(col.r, col.g, col.b, 0.55f), TextAnchor.MiddleCenter);
            UIFactory.Label(hero.transform, item.Name, Theme.Display, 28, Theme.Text, TextAnchor.LowerLeft, FontStyle.Bold);
            UIFactory.Label(hero.transform, "Weapon skin + operator + charm + 2 sprays.", Theme.Body, 13, Theme.TextDim, TextAnchor.UpperLeft);
            var foot = UIFactory.NewRect("foot", hero.rectTransform);
            UIFactory.SetHeight(foot.gameObject, 48);
            UIFactory.AddHLayout(foot, 12, new RectOffset(0, 0, 8, 0), false, false, true);
            var timer = UIFactory.NewRect("tm", foot); UIFactory.Flexible(timer.gameObject, 1, 1);
            UIFactory.AddVLayout(timer, 0, default, true, false, true);
            ScreenUtil.Caption(timer.transform, "ENDS IN");
            UIFactory.Label(timer.transform, item.Tag, Theme.Mono, 18, Theme.Secondary, TextAnchor.MiddleLeft, FontStyle.Bold);
            var buy = UIFactory.Button(foot.transform, "♦ " + item.PriceGems, Theme.Primary, Color.black, Theme.Display, 18,
                () => Buy(item, app));
            UIFactory.SetWidth(buy.gameObject, 150);
        }

        static void ItemCard(RectTransform parent, StoreItem item, AppController app)
        {
            var col = Theme.Rarity(item.Rarity);
            var card = UIFactory.Card(parent, Theme.Panel, Theme.Outline, 1, false, "I_" + item.Name);
            var bar = UIFactory.Panel(card.transform, col, "bar");
            var brt = bar.rectTransform; brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(0.5f, 1); brt.sizeDelta = new Vector2(0, 3);
            UIFactory.AddVLayout(card.rectTransform, 6, new RectOffset(14, 14, 14, 12), true, false, true);

            var top = UIFactory.NewRect("t", card.rectTransform);
            UIFactory.AddHLayout(top, 0, default, false, false, true);
            ScreenUtil.Caption(top.transform, item.Category);
            var sp0 = UIFactory.NewRect("s", top); UIFactory.Flexible(sp0.gameObject, 1);
            if (item.Tag != null) UIFactory.Chip(top.transform, item.Tag, Theme.Secondary, new Color(Theme.Secondary.r, Theme.Secondary.g, Theme.Secondary.b, 0.18f), Theme.Mono, 9);

            var thumbWrap = UIFactory.NewRect("tw", card.rectTransform); UIFactory.Flexible(thumbWrap.gameObject, 1, 1);
            UIFactory.AddVLayout(thumbWrap, 0, default, true, true, true);
            UIFactory.Label(thumbWrap.transform, "⌖", Theme.Display, 52, new Color(col.r, col.g, col.b, 0.5f), TextAnchor.MiddleCenter);

            UIFactory.Label(card.transform, item.Name, Theme.Display, 14, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            var foot = UIFactory.NewRect("f", card.rectTransform);
            UIFactory.SetHeight(foot.gameObject, 30);
            UIFactory.AddHLayout(foot, 6, default, false, false, true);
            UIFactory.Label(foot.transform, item.Rarity.ToString().ToUpper(), Theme.Mono, 9, col, TextAnchor.MiddleLeft);
            var sp = UIFactory.NewRect("s", foot); UIFactory.Flexible(sp.gameObject, 1);
            var buy = UIFactory.Button(foot.transform, "♦ " + item.PriceGems, Theme.PanelHigh, Theme.Primary, Theme.Mono, 12, () => Buy(item, app));
            var o = buy.gameObject.AddComponent<Outline>(); o.effectColor = Theme.Primary; o.effectDistance = new Vector2(1, -1);
            UIFactory.SetWidth(buy.gameObject, 84);
            UIFactory.AddHLayout(buy.GetComponent<RectTransform>(), 0, new RectOffset(8, 8, 6, 6), false, false, true);
        }

        static void Buy(StoreItem item, AppController app)
        {
            if (MockDatabase.Player.Gems >= item.PriceGems)
            {
                MockDatabase.Player.Gems -= item.PriceGems;
                app.RefreshCurrencies();
            }
        }
    }
}
