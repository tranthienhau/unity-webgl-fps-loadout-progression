using System.Collections.Generic;
using System.Linq;

namespace Breachpoint.Data
{
    // Static, deterministic mock content. No backend - everything renders from here so the
    // POC stays demoable on a simulator/browser with realistic data.
    public static class MockDatabase
    {
        public static readonly PlayerProfile Player = new();

        public static readonly List<Weapon> Weapons = new()
        {
            Mk("ar_phantom", "AR-X PHANTOM", WeaponClass.AssaultRifle, new WeaponStats(78, 85, 62, 91, 54, 70), 12,
                ("OPTIC", "REDSIGHT X2"), ("BARREL", "SILENCED_MOD"), ("MAGAZINE", "45R_EXTENDED"), ("GRIP", "TACTICAL_FORE")),
            Mk("smg_viper", "VIPER SMG", WeaponClass.SMG, new WeaponStats(58, 96, 38, 64, 88, 60), 5,
                ("OPTIC", "HOLO_MICRO"), ("BARREL", "STEADY_PORT"), ("MAGAZINE", "DRUM_50"), ("GRIP", "QUICKDRAW")),
            Mk("snp_longbow", "LONGBOW DMR", WeaponClass.Marksman, new WeaponStats(94, 42, 95, 88, 40, 55), 22,
                ("OPTIC", "VARIABLE_8X"), ("BARREL", "HEAVY_LONG"), ("MAGAZINE", "20R_FMJ"), ("GRIP", "BIPOD")),
            Mk("sht_breaker", "BREAKER 12", WeaponClass.Shotgun, new WeaponStats(99, 30, 18, 50, 70, 45), 8,
                ("OPTIC", "IRON"), ("BARREL", "CHOKE"), ("MAGAZINE", "TUBE_8"), ("GRIP", "PUMP_GRIP")),
            Mk("lmg_titan", "TITAN LMG", WeaponClass.LMG, new WeaponStats(82, 78, 70, 60, 30, 48), 30,
                ("OPTIC", "REFLEX"), ("BARREL", "HEAVY"), ("MAGAZINE", "BELT_120"), ("GRIP", "FOREGRIP")),
            Mk("p9_modular", "P9 MODULAR", WeaponClass.Pistol, new WeaponStats(55, 70, 40, 75, 95, 80), 1,
                ("OPTIC", "MINI_DOT"), ("BARREL", "COMPENSATOR"), ("MAGAZINE", "17R"), ("GRIP", "STIPPLED")),
        };

        // Weapon skins (armory / loadout customize).
        public static readonly List<Skin> Skins = new()
        {
            new("sk_phantom_apex",   "APEX PREDATOR",  "ar_phantom",  Rarity.Legendary, 2400, true),
            new("sk_phantom_carbon", "CARBON STRIKE",  "ar_phantom",  Rarity.Epic,      1200, true),
            new("sk_phantom_jungle", "JUNGLE WARFARE", "ar_phantom",  Rarity.Rare,        600, false),
            new("sk_viper_neon",     "NEON VENOM",     "smg_viper",   Rarity.Legendary, 2400, false),
            new("sk_viper_urban",    "URBAN CAMO",     "smg_viper",   Rarity.Rare,        600, true),
            new("sk_longbow_gold",   "GILDED EAGLE",   "snp_longbow", Rarity.Legendary, 2800, false),
            new("sk_breaker_blaze",  "BLAZE",          "sht_breaker", Rarity.Epic,      1200, false),
            new("sk_titan_arctic",   "ARCTIC WOLF",    "lmg_titan",   Rarity.Epic,      1400, false),
        };

        public static readonly List<StoreItem> Store = new()
        {
            new("APEX PREDATOR BUNDLE", "BUNDLE",      Rarity.Legendary, 2400, featured: true, tag: "20:14:55"),
            new("NEON VENOM",           "WEAPON SKIN", Rarity.Legendary, 1800, tag: "NEW"),
            new("GHOST OPERATOR",       "OPERATOR",    Rarity.Legendary, 2000),
            new("GILDED EAGLE",         "WEAPON SKIN", Rarity.Legendary, 1900),
            new("ARCTIC WOLF",          "WEAPON SKIN", Rarity.Epic,      1200),
            new("BLAZE",                "WEAPON SKIN", Rarity.Epic,      1200, tag: "-20%"),
            new("VIPER CHARM",          "CHARM",       Rarity.Rare,       300),
            new("SKULL SPRAY",          "SPRAY",       Rarity.Rare,       250),
            new("RECON OPERATOR",       "OPERATOR",    Rarity.Epic,      1600),
        };

        public static readonly List<GameMode> Modes = new()
        {
            new(ModeType.TeamDeathmatch, "TEAM DEATHMATCH", "First squad to 75 eliminations wins.", "6v6"),
            new(ModeType.CaptureTheFlag, "CAPTURE THE FLAG", "Steal the enemy flag, defend your own.", "5v5"),
            new(ModeType.FreeForAll,     "FREE-FOR-ALL",    "Every operator for themselves. 30 kills.", "FFA-12"),
            new(ModeType.Ranked,         "RANKED",          "Competitive ladder. Earn rank points.", "5v5", ranked: true, rankTier: "PLATINUM III"),
            new(ModeType.LimitedTime,    "GUN GAME",        "Cycle every weapon. Limited time event.", "FFA-10", isNew: true),
        };

        public static readonly List<BattlePassTier> Pass = BuildPass();

        public static readonly List<Challenge> Challenges = new()
        {
            new("Win 3 matches",            1500, 2, 3,  false),
            new("Eliminate 50 operators",    800, 38, 50, false),
            new("Headshots x20",             600, 11, 20, false),
            new("Capture 10 flags",         3000, 4, 10,  true),
            new("Play 5 Ranked games",      2500, 1, 5,   true),
        };

        public static Weapon EquippedPrimary =>
            Weapons.First(w => w.Id == Player.Loadout[SlotType.Primary]);

        public static IEnumerable<Skin> SkinsFor(string weaponId) => Skins.Where(s => s.WeaponId == weaponId);

        static Weapon Mk(string id, string name, WeaponClass cls, WeaponStats stats, int unlock,
            params (string slot, string mod)[] mods)
        {
            var w = new Weapon(id, name, cls, stats, unlock);
            foreach (var m in mods) w.Attachments.Add(new Attachment(m.slot, m.mod, true));
            return w;
        }

        static List<BattlePassTier> BuildPass()
        {
            var list = new List<BattlePassTier>();
            string[] free  = { "+500 XP", "100 Credits", "Weapon Charm", "+1000 XP", "Spray", "Calling Card", "200 Credits", "+1500 XP" };
            string[] prem  = { "VIPER Skin", "Operator Skin", "500 Gems", "PHANTOM Skin", "XP Boost", "LONGBOW Skin", "Finisher", "APEX Bundle" };
            Rarity[] pr    = { Rarity.Epic, Rarity.Legendary, Rarity.Rare, Rarity.Legendary, Rarity.Rare, Rarity.Epic, Rarity.Epic, Rarity.Legendary };
            int current = MockDatabase.Player?.SeasonTier ?? 42;
            for (int i = 0; i < 8; i++)
            {
                int tierNum = 40 + i;   // window around current tier
                TierState st = tierNum < current ? TierState.Claimed
                            : tierNum == current ? TierState.Current
                            : tierNum == current + 1 ? TierState.Unlocked
                            : TierState.Locked;
                list.Add(new BattlePassTier(tierNum, free[i], prem[i], pr[i], st));
            }
            return list;
        }
    }
}
