using RoR2;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaBuffs
    {
        // armor buff gained during roll
        public static BuffDef manaDefBuff;
        public static BuffDef reduceManaCost; // Reduce Mana Cost
        public static BuffDef manaRegenCont; // Mana Regeneration Continuious
        public static BuffDef bookmarkAuto; // Mana Regeneration Continuious

        public static void Init(AssetBundle assetBundle)
        {
            manaDefBuff = Modules.Content.CreateAndAddBuff("AsukaArmorBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.blue,
                false,
                false);

            reduceManaCost = Modules.Content.CreateAndAddBuff("AsukaReduceMana",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.yellow,
                false,
                false);
            
            manaRegenCont = Modules.Content.CreateAndAddBuff("AsukaManaRegenBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.green,
                false,
                false);
            
            bookmarkAuto = Modules.Content.CreateAndAddBuff("AsukaAutoBookmarkBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.magenta,
                false,
                false);
        }
    }
}
