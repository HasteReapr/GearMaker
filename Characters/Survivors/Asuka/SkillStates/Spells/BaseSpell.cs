using RoR2;
using RoR2.Skills;
using JetBrains.Annotations;
using AsukaMod.Survivors.Asuka.Components;
using EntityStates;

namespace AsukaMod.Survivors.Asuka.Spells
{
    public class BaseSpell : SkillDef
    {
        //The mana cost of the skill, -100 to 100. Negative values regenerate mana.
        public float manaCost = 0;
        //Checks if the spell can be cast, by default it's false, but if the player's mana is greater than manaCost, this becomes true.
        public bool canCast = false;

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public AsukaManaComponent manaComp;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                manaComp = skillSlot.GetComponent<AsukaManaComponent>()
            };
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            AsukaManaComponent manaCtrl = ((InstanceData)skillSlot.skillInstanceData).manaComp;
            return manaCtrl.mana > manaCost && base.CanExecute(skillSlot);
        }
    }
}
