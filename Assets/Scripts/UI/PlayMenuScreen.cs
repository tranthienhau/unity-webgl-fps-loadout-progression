using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    public static class PlayMenuScreen
    {
        static GameMode _selected;

        public static void Build(RectTransform area, AppController app)
        {
            _selected ??= MockDatabase.Modes[0];
            var root = ScreenUtil.Root(area);
            ScreenUtil.Header(root, "PLAY", "SELECT GAME MODE // SEASON 4: BLACKOUT");

            var body = UIFactory.NewRect("Body", root);
            UIFactory.Flexible(body.gameObject, 1, 1);
            UIFactory.AddHLayout(body, 24, default, false, true, true);

            // Mode list
            var col = UIFactory.NewRect("Modes", body);
            UIFactory.Flexible(col.gameObject, 1.4f, 1);
            UIFactory.AddVLayout(col, 12, default, true, false, true);
            foreach (var m in MockDatabase.Modes) ModeCard(col, m, app);

            // Side column
            var side = UIFactory.NewRect("Side", body);
            UIFactory.Flexible(side.gameObject, 1, 1);
            UIFactory.AddVLayout(side, 16, default, true, false, true);

            // Featured season banner
            var banner = UIFactory.Card(side, Theme.Panel, Theme.Secondary, 1);
            UIFactory.SetHeight(banner.gameObject, 180);
            UIFactory.AddVLayout(banner.rectTransform, 6, new RectOffset(20, 20, 18, 18), true, false, true);
            UIFactory.Chip(banner.transform, "SEASON EVENT", Theme.Secondary, new Color(Theme.Secondary.r, Theme.Secondary.g, Theme.Secondary.b, 0.12f), Theme.Mono);
            UIFactory.Label(banner.transform, "OPERATION\nBLACKOUT", Theme.Display, 26, Theme.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            var sp = UIFactory.NewRect("sp", banner.rectTransform); UIFactory.Flexible(sp.gameObject, 1, 1);
            UIFactory.Label(banner.transform, "Limited-time map + ranked rewards. Ends in 12 days.", Theme.Body, 13, Theme.TextDim, TextAnchor.LowerLeft);

            // Squad row
            var squad = UIFactory.Card(side, Theme.PanelLow, Theme.Outline, 1);
            UIFactory.SetHeight(squad.gameObject, 96);
            UIFactory.AddVLayout(squad.rectTransform, 8, new RectOffset(18, 18, 14, 14), true, false, true);
            ScreenUtil.Caption(squad.transform, "SQUAD // PHANTOM_9");
            var slots = UIFactory.NewRect("slots", squad.rectTransform);
            UIFactory.AddHLayout(slots, 10, default, false, false, true);
            ScreenUtil.Thumb(slots.transform, Theme.Primary, "✓", 40);
            ScreenUtil.Thumb(slots.transform, Theme.Primary, "✓", 40);
            ScreenUtil.Thumb(slots.transform, Theme.TextDim, "+", 40);

            // Find match
            var find = UIFactory.Button(side, "FIND MATCH", Theme.Primary, Color.black, Theme.Display, 24, () => app.StartMatch(_selected));
            UIFactory.SetHeight(find.gameObject, 72);
        }

        static void ModeCard(RectTransform parent, GameMode m, AppController app)
        {
            bool sel = m == _selected;
            var card = UIFactory.Card(parent, sel ? Theme.PanelHigh : Theme.Panel, sel ? Theme.Primary : Theme.Outline, sel ? 2 : 1, sel);
            UIFactory.SetHeight(card.gameObject, 88);
            var btn = card.gameObject.AddComponent<Button>(); btn.targetGraphic = card;
            btn.onClick.AddListener(() => { _selected = m; app.Navigate(Screen.Play); });
            UIFactory.AddHLayout(card.rectTransform, 16, new RectOffset(16, 20, 14, 14), false, false, true);

            ScreenUtil.Thumb(card.transform, sel ? Theme.Primary : Theme.TextDim, "▣", 60);

            var info = UIFactory.NewRect("info", card.rectTransform);
            UIFactory.Flexible(info.gameObject, 1, 1);
            UIFactory.AddVLayout(info, 3, new RectOffset(0, 0, 6, 6), true, false, true);
            var titleRow = UIFactory.NewRect("tr", info);
            UIFactory.AddHLayout(titleRow, 10, default, false, false, true);
            UIFactory.Label(titleRow.transform, m.Name, Theme.Display, 18, sel ? Theme.Primary : Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            if (m.IsNew) UIFactory.Chip(titleRow.transform, "NEW", Theme.Secondary, new Color(Theme.Secondary.r, Theme.Secondary.g, Theme.Secondary.b, 0.18f), Theme.Mono);
            if (m.IsRanked) UIFactory.Chip(titleRow.transform, m.RankTier, Theme.Primary, Theme.PrimaryDim, Theme.Mono);
            UIFactory.Label(info.transform, m.Desc, Theme.Body, 13, Theme.TextDim, TextAnchor.MiddleLeft);

            var players = UIFactory.NewRect("pl", card.rectTransform);
            UIFactory.AddVLayout(players, 2, default, true, false, true);
            UIFactory.SetWidth(players.gameObject, 80);
            UIFactory.Label(players.transform, m.Players, Theme.Display, 20, sel ? Theme.Primary : Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Label(players.transform, "PLAYERS", Theme.Mono, 9, Theme.TextDim, TextAnchor.MiddleCenter);
        }
    }
}
