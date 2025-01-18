using System;
using AsukaMod.Modules;
using AsukaMod.Survivors.Asuka.Achievements;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaTokens
    {
        public static void Init()
        {
            AddAsukaTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Asuka.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddAsukaTokens()
        {
            string prefix = AsukaSurvivor.Asuka_PREFIX;

            string desc = "The Gear Maker is a powerful mage wielding the power of the Tome of Origin.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Bookmark is a channeled ability. Hold down secondary and press a corresponding spell slot to draw or discard in the designated slot." + Environment.NewLine + Environment.NewLine
             + "< ! > The Gear Maker has three decks to choose from, each offering a wide array of unique spells at your disposal." + Environment.NewLine + Environment.NewLine
             + "< ! > Recover Mana has a set minimum duration so you will always recover at least 10 mana." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, praying for the accumulation of goodwill.";
            string outroFailure = "..and so he vanished, in cinders.";

            Language.Add(prefix + "NAME", "Gear Maker");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Master of Sorcery");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Asuka passive");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Sample text.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_BOOK_NAME", "Tome of Origin");
            Language.Add(prefix + "PRIMARY_BOOK_DESCRIPTION", Tokens.agilePrefix + $"Open The Tome of Origin to cast for <style=cIsDamage>{100f * AsukaStaticValues.swordDamageCoefficient}% damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_BOOKMARK_NAME", "Bookmark");
            Language.Add(prefix + "SECONDARY_BOOKMARK_DESCRIPTION",  "Draw or discard a Spell in the designated spell slot.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_DECKSWAP_NAME", "Change Test Case");
            Language.Add(prefix + "UTILITY_DECKSWAP_DESCRIPTION", "Cycle through your <style=cIsUtility>deck</style>.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_RECOVER_MANA_NAME", "Recover Mana");
            Language.Add(prefix + "SPECIAL_RECOVER_MANA_DESCRIPTION", "Focus and regenerate <style=cIsUtility>5 mana per second</style> while channeled.");
            #endregion

            #region Spells

            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(AsukaMasteryAchievement.identifier), "Asuka: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(AsukaMasteryAchievement.identifier), "As Asuka, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
