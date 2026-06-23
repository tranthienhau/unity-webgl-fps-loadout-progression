using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    public static class BattlePassScreen
    {
        public static void Build(RectTransform area, AppController app)
        {
            var p = MockDatabase.Player;
            var root = ScreenUtil.Root(area, 16);
            ScreenUtil.Header(root, "BATTLE PASS", "SEASON 4: BLACKOUT // 12 DAYS REMAINING");

            // Progress header card
            var prog = UIFactory.Card(root, Theme.Panel, Theme.Outline, 1);
            UIFactory.SetHeight(prog.gameObject, 104);
            UIFactory.AddHLayout(prog.rectTransform, 24, new RectOffset(24, 24, 16, 16), false, false, true);

            var badge = UIFactory.Card(prog.transform, Theme.PanelHigh, Theme.Primary, 2, true);
            UIFactory.SetWidth(badge.gameObject, 110);
            UIFactory.AddVLayout(badge.rectTransform, 0, new RectOffset(0, 0, 8, 8), true, true, true);
            UIFactory.Label(badge.transform, "TIER", Theme.Mono, 11, Theme.TextDim, TextAnchor.LowerCenter);
            UIFactory.Label(badge.transform, p.SeasonTier.ToString(), Theme.Display, 40, Theme.Primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Label(badge.transform, "/ " + p.SeasonTierMax, Theme.Mono, 11, Theme.TextDim, TextAnchor.UpperCenter);

            var xpCol = UIFactory.NewRect("xp", prog.rectTransform);
            UIFactory.Flexible(xpCol.gameObject, 1, 1);
            UIFactory.AddVLayout(xpCol, 8, new RectOffset(0, 0, 18, 18), true, false, true);
            var xpHead = UIFactory.NewRect("h", xpCol);
            UIFactory.AddHLayout(xpHead, 8, default, false, false, true);
            var l = UIFactory.Label(xpHead.transform, "PROGRESS TO TIER " + (p.SeasonTier + 1), Theme.Body, 15, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Flexible(l.gameObject, 1);
            UIFactory.Label(xpHead.transform, p.SeasonXp + " / " + p.SeasonXpToNext + " XP", Theme.Mono, 13, Theme.Primary, TextAnchor.MiddleRight);
            var track = UIFactory.Panel(xpCol, new Color(1, 1, 1, 0.06f), "track");
            UIFactory.SetHeight(track.gameObject, 10);
            var fill = UIFactory.Panel(track.transform, Theme.Primary, "fill");
            var frt = fill.rectTransform; frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2((float)p.SeasonXp / p.SeasonXpToNext, 1);
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            UIFactory.Label(xpCol.transform, "Earn XP from matches, challenges and daily logins.", Theme.Body, 12, Theme.TextDim, TextAnchor.UpperLeft);

            if (!p.PremiumPass)
            {
                var up = UIFactory.Button(prog.transform, "UPGRADE\n♦ 1000", Theme.Secondary, Color.black, Theme.Display, 16,
                    () => { p.PremiumPass = true; app.Navigate(Screen.BattlePass); });
                UIFactory.SetWidth(up.gameObject, 150);
            }
            else
            {
                var owned = UIFactory.Card(prog.transform, new Color(Theme.Secondary.r, Theme.Secondary.g, Theme.Secondary.b, 0.12f), Theme.Secondary, 1);
                UIFactory.SetWidth(owned.gameObject, 150);
                UIFactory.AddVLayout(owned.rectTransform, 0, default, true, true, true);
                UIFactory.Label(owned.transform, "PREMIUM\nACTIVE", Theme.Display, 16, Theme.Secondary, TextAnchor.MiddleCenter, FontStyle.Bold);
            }

            // Body: reward track + challenges
            var body = UIFactory.NewRect("Body", root);
            UIFactory.Flexible(body.gameObject, 1, 1);
            UIFactory.AddHLayout(body, 20, default, false, true, true);

            var trackCard = UIFactory.Card(body, Theme.PanelLow, Theme.Outline, 1);
            UIFactory.Flexible(trackCard.gameObject, 2.2f, 1);
            UIFactory.AddVLayout(trackCard.rectTransform, 12, new RectOffset(20, 20, 16, 16), true, false, true);
            BuildTrackLegend(trackCard.transform);
            var cols = UIFactory.NewRect("cols", trackCard.rectTransform);
            UIFactory.Flexible(cols.gameObject, 1, 1);
            UIFactory.AddHLayout(cols, 10, default, false, true, true);
            foreach (var t in MockDatabase.Pass) TierColumn(cols, t, p.PremiumPass);

            var chal = UIFactory.Card(body, Theme.Panel, Theme.Outline, 1);
            UIFactory.Flexible(chal.gameObject, 1, 1);
            UIFactory.AddVLayout(chal.rectTransform, 10, new RectOffset(20, 20, 16, 16), true, false, true);
            UIFactory.Label(chal.transform, "CHALLENGES", Theme.Display, 16, Theme.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            foreach (var c in MockDatabase.Challenges) ChallengeRow(chal.transform, c);
        }

        static void BuildTrackLegend(Transform parent)
        {
            var row = UIFactory.NewRect("legend", (RectTransform)parent);
            UIFactory.SetHeight(row.gameObject, 20);
            UIFactory.AddHLayout(row, 16, default, false, false, true);
            UIFactory.Label(row.transform, "REWARD TRACK", Theme.Display, 14, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Label(row.transform, "● FREE", Theme.Mono, 10, Theme.TextDim, TextAnchor.MiddleLeft);
            UIFactory.Label(row.transform, "● PREMIUM", Theme.Mono, 10, Theme.Secondary, TextAnchor.MiddleLeft);
        }

        static void TierColumn(RectTransform parent, BattlePassTier t, bool premium)
        {
            var col = UIFactory.NewRect("T" + t.Tier, parent);
            UIFactory.Flexible(col.gameObject, 1, 1);
            UIFactory.AddVLayout(col, 8, default, true, false, true);
            RewardNode(col, t.FreeReward, Theme.Primary, t.State, false);
            var num = UIFactory.Card(col, t.State == TierState.Current ? Theme.Primary : Theme.PanelHigh,
                t.State == TierState.Current ? Theme.Primary : Theme.Outline, 1);
            UIFactory.SetHeight(num.gameObject, 26);
            UIFactory.AddVLayout(num.rectTransform, 0, default, true, true, true);
            UIFactory.Label(num.transform, t.Tier.ToString(), Theme.Display, 13,
                t.State == TierState.Current ? Color.black : Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            RewardNode(col, t.PremiumReward, Theme.Secondary, premium ? t.State : TierState.Locked, true, t.PremiumRarity);
        }

        static void RewardNode(RectTransform parent, string label, Color accent, TierState state, bool prem, Rarity rarity = Rarity.Common)
        {
            bool locked = state == TierState.Locked;
            var col = prem ? Theme.Rarity(rarity) : Theme.Primary;
            var node = UIFactory.Card(parent, locked ? Theme.PanelLow : new Color(col.r, col.g, col.b, 0.10f),
                locked ? Theme.Outline : col, prem ? 2 : 1, state == TierState.Current);
            UIFactory.Flexible(node.gameObject, 1, 1);
            UIFactory.AddVLayout(node.rectTransform, 2, new RectOffset(4, 4, 6, 6), true, true, true);
            string icon = state == TierState.Claimed ? "✓" : locked ? "🔒" : "⌖";
            UIFactory.Label(node.transform, icon, Theme.Body, 22, locked ? Theme.TextDim : col, TextAnchor.MiddleCenter);
            UIFactory.Label(node.transform, label, Theme.Mono, 8, locked ? Theme.TextDim : Theme.Text, TextAnchor.UpperCenter);
        }

        static void ChallengeRow(Transform parent, Challenge c)
        {
            var row = UIFactory.NewRect("C", (RectTransform)parent);
            UIFactory.SetHeight(row.gameObject, 52);
            UIFactory.AddVLayout(row, 4, new RectOffset(0, 0, 4, 4), true, false, true);
            var head = UIFactory.NewRect("h", row);
            UIFactory.AddHLayout(head, 8, default, false, false, true);
            var n = UIFactory.Label(head.transform, c.Name, Theme.Body, 13, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Flexible(n.gameObject, 1);
            UIFactory.Label(head.transform, (c.Weekly ? "+" : "+") + c.Xp + " XP", Theme.Mono, 11, Theme.Primary, TextAnchor.MiddleRight);
            var track = UIFactory.Panel(row, new Color(1, 1, 1, 0.06f), "track");
            UIFactory.SetHeight(track.gameObject, 6);
            var fill = UIFactory.Panel(track.transform, c.Weekly ? Theme.Secondary : Theme.Primary, "fill");
            var frt = fill.rectTransform; frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2((float)c.Progress / c.Goal, 1);
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            var meta = UIFactory.Label(row.transform, c.Progress + " / " + c.Goal + (c.Weekly ? "   WEEKLY" : "   DAILY"), Theme.Mono, 9, Theme.TextDim, TextAnchor.UpperLeft);
        }
    }
}
