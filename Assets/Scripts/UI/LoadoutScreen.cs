using UnityEngine;
using UnityEngine.UI;
using Breachpoint.Data;
using Breachpoint.App;
using Screen = Breachpoint.App.Screen;

namespace Breachpoint.UI
{
    public static class LoadoutScreen
    {
        public static void Build(RectTransform area, AppController app)
        {
            var w = MockDatabase.EquippedPrimary;
            var equippedSkin = SkinName(w);

            var root = ScreenUtil.Root(area);
            ScreenUtil.Header(root, "LOADOUT", "PRIMARY // " + w.Name + "  -  " + equippedSkin);

            var body = UIFactory.NewRect("Body", root);
            UIFactory.Flexible(body.gameObject, 1, 1);
            UIFactory.AddHLayout(body, 24, default, false, true, true);

            // ---- Left: preview + slots ----
            var left = UIFactory.NewRect("Left", body);
            UIFactory.Flexible(left.gameObject, 2, 1);
            UIFactory.AddVLayout(left, 16, default, true, false, true);

            var preview = UIFactory.Card(left, Theme.Panel, Theme.Outline, 1);
            UIFactory.Flexible(preview.gameObject, 1, 1);
            UIFactory.AddVLayout(preview.rectTransform, 4, new RectOffset(24, 24, 20, 20), true, false, true);
            var nameBox = UIFactory.NewRect("name", preview.rectTransform);
            UIFactory.AddVLayout(nameBox, 2, new RectOffset(12, 0, 0, 0), true, false, true);
            var nameBar = nameBox.gameObject.AddComponent<Outline>();
            nameBar.effectColor = Theme.Primary; nameBar.effectDistance = new Vector2(-2, 0);
            UIFactory.Label(nameBox.transform, w.Name, Theme.Display, 30, Theme.Primary, TextAnchor.UpperLeft, FontStyle.Bold);
            UIFactory.Label(nameBox.transform, "EXPERIMENTAL " + Pretty(w.Class) + " // SKIN: " + equippedSkin, Theme.Body, 13, Theme.TextDim, TextAnchor.UpperLeft);
            var glyphWrap = UIFactory.NewRect("g", preview.rectTransform); UIFactory.Flexible(glyphWrap.gameObject, 1, 1);
            UIFactory.AddVLayout(glyphWrap, 0, default, true, true, true);
            UIFactory.Label(glyphWrap.transform, "⌖", Theme.Display, 140, new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.5f), TextAnchor.MiddleCenter);

            var slots = UIFactory.NewRect("slots", left);
            UIFactory.SetHeight(slots.gameObject, 120);
            UIFactory.AddHLayout(slots, 12, default, true, true, true);
            SlotCard(slots, "PRIMARY", w.Name, true, w.EquippedSkinId != null);
            SlotCard(slots, "SECONDARY", "P9 MODULAR", false, false);
            SlotCard(slots, "MELEE", "TACTICAL BLADE", false, false);
            SlotCard(slots, "TACTICAL", "SENSOR GRENADE", false, false);
            SlotCard(slots, "LETHAL", "M67 FRAG", false, false);

            // ---- Right: specs + mods + actions ----
            var right = UIFactory.NewRect("Right", body);
            UIFactory.Flexible(right.gameObject, 1, 1);
            UIFactory.AddVLayout(right, 16, default, true, false, true);

            var specs = UIFactory.Card(right, Theme.Panel, Theme.Outline, 1);
            UIFactory.AddVLayout(specs.rectTransform, 14, new RectOffset(24, 24, 20, 20), true, false, true);
            SectionHead(specs.transform, "WEAPON SPECS", "v2.0.4.R");
            foreach (var (label, value) in w.Stats.Enumerate())
                UIFactory.StatBar(specs.transform, label, value, Theme.Body, Theme.Mono);

            var mods = UIFactory.Card(right, Theme.PanelLow, Theme.Outline, 1);
            UIFactory.AddVLayout(mods.rectTransform, 12, new RectOffset(24, 24, 18, 18), true, false, true);
            SectionHead(mods.transform, "MODIFICATIONS", "[4/5]");
            var grid = UIFactory.NewRect("grid", mods.rectTransform);
            UIFactory.AddGrid(grid, new Vector2(180, 54), new Vector2(12, 10), 2);
            UIFactory.SetHeight(grid.gameObject, 118);
            foreach (var a in w.Attachments) ModChip(grid, a);

            var sp = UIFactory.NewRect("sp", right); UIFactory.Flexible(sp.gameObject, 1, 1);
            var customize = UIFactory.Button(right, "✦  CUSTOMIZE", new Color(0, 0, 0, 0), Theme.Primary, Theme.Display, 17,
                () => { app.SelectedWeaponId = w.Id; app.Navigate(Screen.WeaponDetail); });
            var cob = customize.gameObject.AddComponent<Outline>(); cob.effectColor = Theme.Primary; cob.effectDistance = new Vector2(1, -1);
            UIFactory.SetHeight(customize.gameObject, 52);
            var equip = UIFactory.Button(right, "EQUIP LOADOUT", Theme.Primary, Color.black, Theme.Display, 22, () => { });
            UIFactory.SetHeight(equip.gameObject, 60);
        }

        static void SlotCard(RectTransform parent, string slot, string name, bool active, bool legendary)
        {
            var card = UIFactory.Card(parent, active ? Theme.PanelHigh : Theme.Panel, active ? Theme.Primary : Theme.Outline, active ? 2 : 1, active);
            UIFactory.AddVLayout(card.rectTransform, 4, new RectOffset(12, 12, 10, 10), true, false, true);
            var top = UIFactory.NewRect("t", card.rectTransform);
            UIFactory.AddHLayout(top, 4, default, false, false, true);
            var s = ScreenUtil.Caption(top.transform, slot); UIFactory.Flexible(s.gameObject, 1);
            if (legendary) UIFactory.Chip(top.transform, "LEG", Theme.Secondary, new Color(Theme.Secondary.r, Theme.Secondary.g, Theme.Secondary.b, 0.18f), Theme.Mono, 8);
            var icon = UIFactory.NewRect("i", card.rectTransform); UIFactory.Flexible(icon.gameObject, 1, 1);
            UIFactory.AddVLayout(icon, 0, default, true, true, true);
            UIFactory.Label(icon.transform, "⌖", Theme.Body, 30, active ? Theme.Primary : new Color(1, 1, 1, 0.25f), TextAnchor.MiddleCenter);
            UIFactory.Label(card.transform, name, Theme.Display, 10, active ? Theme.Primary : Theme.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        static void ModChip(RectTransform parent, Attachment a)
        {
            var c = UIFactory.Card(parent, Theme.PanelHigh, new Color(Theme.Outline.r, Theme.Outline.g, Theme.Outline.b, 0.4f), 1);
            UIFactory.AddVLayout(c.rectTransform, 3, new RectOffset(12, 12, 8, 8), true, false, true);
            var top = UIFactory.NewRect("t", c.rectTransform);
            UIFactory.AddHLayout(top, 4, default, false, false, true);
            var s = ScreenUtil.Caption(top.transform, a.Slot); UIFactory.Flexible(s.gameObject, 1);
            if (a.Equipped) UIFactory.Chip(top.transform, "EQUIPPED", Theme.Primary, Theme.PrimaryDim, Theme.Mono, 8);
            UIFactory.Label(c.transform, a.Name, Theme.Body, 14, Theme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
        }

        static void SectionHead(Transform parent, string title, string meta)
        {
            var row = UIFactory.NewRect("head", (RectTransform)parent);
            UIFactory.AddHLayout(row, 8, new RectOffset(0, 0, 0, 8), false, false, true);
            UIFactory.SetHeight(row.gameObject, 30);
            var t = UIFactory.Label(row.transform, title, Theme.Display, 18, Theme.Text, TextAnchor.LowerLeft, FontStyle.Bold);
            UIFactory.Flexible(t.gameObject, 1);
            UIFactory.Label(row.transform, meta, Theme.Mono, 10, Theme.TextDim, TextAnchor.LowerRight);
            var ul = row.gameObject.AddComponent<Image>(); ul.color = new Color(0, 0, 0, 0); // ensure rect
        }

        static string SkinName(Weapon w)
        {
            if (w.EquippedSkinId == null) return "DEFAULT";
            foreach (var s in MockDatabase.Skins) if (s.Id == w.EquippedSkinId) return s.Name;
            return "DEFAULT";
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
