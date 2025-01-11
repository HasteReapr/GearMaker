using AsukaMod.Survivors.Asuka.Achievements;
using RoR2;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                AsukaMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(AsukaMasteryAchievement.identifier),
                AsukaSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
