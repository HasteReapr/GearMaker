using RoR2;
using AsukaMod.Modules.Achievements;

namespace AsukaMod.Survivors.Asuka.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class AsukaMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = AsukaSurvivor.Asuka_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = AsukaSurvivor.Asuka_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => AsukaSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}