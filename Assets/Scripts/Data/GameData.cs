using System.Collections.Generic;

namespace Breachpoint.Data
{
    // Visual rarity tiers. Drives the color ramp on cards and tags.
    public enum Rarity { Common, Rare, Epic, Legendary }

    public enum WeaponClass { AssaultRifle, SMG, Sniper, Shotgun, LMG, Marksman, Pistol }

    public enum ModeType { TeamDeathmatch, CaptureTheFlag, FreeForAll, Ranked, LimitedTime }

    // Slot types shown on the loadout screen.
    public enum SlotType { Primary, Secondary, Melee, Tactical, Lethal }

    // 0-100 tuning values rendered as stat bars. Also the surface for "balancing" work.
    public class WeaponStats
    {
        public int Damage, FireRate, Range, Accuracy, Mobility, Control;

        public WeaponStats(int dmg, int fire, int range, int acc, int mob, int ctrl)
        {
            Damage = dmg; FireRate = fire; Range = range; Accuracy = acc; Mobility = mob; Control = ctrl;
        }

        public IEnumerable<(string label, int value)> Enumerate()
        {
            yield return ("DAMAGE", Damage);
            yield return ("FIRE RATE", FireRate);
            yield return ("RANGE", Range);
            yield return ("ACCURACY", Accuracy);
            yield return ("MOBILITY", Mobility);
            yield return ("CONTROL", Control);
        }
    }

    public class Attachment
    {
        public string Slot;   // OPTIC, BARREL, MAGAZINE, GRIP
        public string Name;
        public bool Equipped;
        public Attachment(string slot, string name, bool equipped) { Slot = slot; Name = name; Equipped = equipped; }
    }

    public class Weapon
    {
        public string Id;
        public string Name;
        public WeaponClass Class;
        public WeaponStats Stats;
        public int UnlockLevel;
        public string EquippedSkinId;          // null = default skin
        public List<Attachment> Attachments = new();

        public Weapon(string id, string name, WeaponClass cls, WeaponStats stats, int unlock)
        {
            Id = id; Name = name; Class = cls; Stats = stats; UnlockLevel = unlock;
        }
    }

    public class Skin
    {
        public string Id;
        public string Name;
        public string WeaponId;     // weapon this skin applies to
        public Rarity Rarity;
        public int PriceGems;
        public bool Owned;

        public Skin(string id, string name, string weaponId, Rarity rarity, int price, bool owned)
        {
            Id = id; Name = name; WeaponId = weaponId; Rarity = rarity; PriceGems = price; Owned = owned;
        }
    }

    // Cosmetic store entry (skin bundle, operator, charm, spray...).
    public class StoreItem
    {
        public string Name;
        public string Category;     // WEAPON SKIN, OPERATOR, CHARM, SPRAY, BUNDLE
        public Rarity Rarity;
        public int PriceGems;
        public bool Featured;
        public string Tag;          // optional: "NEW", "-20%", null

        public StoreItem(string name, string cat, Rarity rarity, int price, bool featured = false, string tag = null)
        {
            Name = name; Category = cat; Rarity = rarity; PriceGems = price; Featured = featured; Tag = tag;
        }
    }

    public class GameMode
    {
        public ModeType Type;
        public string Name;
        public string Desc;
        public string Players;      // "6v6", "FFA-12"
        public bool IsNew;
        public bool IsRanked;
        public string RankTier;     // for ranked: "PLATINUM III"

        public GameMode(ModeType type, string name, string desc, string players, bool isNew = false, bool ranked = false, string rankTier = null)
        {
            Type = type; Name = name; Desc = desc; Players = players; IsNew = isNew; IsRanked = ranked; RankTier = rankTier;
        }
    }

    public enum TierState { Claimed, Current, Unlocked, Locked }

    public class BattlePassTier
    {
        public int Tier;
        public string FreeReward;
        public string PremiumReward;
        public Rarity PremiumRarity;
        public TierState State;

        public BattlePassTier(int tier, string free, string premium, Rarity premiumRarity, TierState state)
        {
            Tier = tier; FreeReward = free; PremiumReward = premium; PremiumRarity = premiumRarity; State = state;
        }
    }

    public class Challenge
    {
        public string Name;
        public int Xp;
        public int Progress;
        public int Goal;
        public bool Weekly;
        public Challenge(string name, int xp, int progress, int goal, bool weekly)
        {
            Name = name; Xp = xp; Progress = progress; Goal = goal; Weekly = weekly;
        }
    }

    // Single mutable player state. Equip/claim actions mutate this and screens re-read it.
    public class PlayerProfile
    {
        public string Name = "OPERATOR_01";
        public int Level = 55;
        public int Credits = 25400;     // soft currency
        public int Gems = 1250;         // premium currency
        public int SeasonTier = 42;
        public int SeasonTierMax = 100;
        public int SeasonXp = 7400;
        public int SeasonXpToNext = 10000;
        public bool PremiumPass = false;

        // Equipped loadout: slot -> weapon id (or item name for non-weapon slots).
        public Dictionary<SlotType, string> Loadout = new()
        {
            { SlotType.Primary,  "ar_phantom" },
            { SlotType.Secondary,"p9_modular" },
            { SlotType.Melee,    "Tactical Blade" },
            { SlotType.Tactical, "Sensor Grenade" },
            { SlotType.Lethal,   "M67 Frag" },
        };
    }
}
