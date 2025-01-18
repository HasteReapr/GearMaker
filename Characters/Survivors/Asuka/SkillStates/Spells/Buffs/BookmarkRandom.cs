using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using AsukaMod.Survivors.Asuka.Components;
using ExtraSkillSlots;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class BookmarkRandom : BaseSpellState
    {
        public float baseDuration = 0.29f;
        public float duration;
        private Animator animator;

        AsukaManaComponent manaComp;
        ExtraSkillLocator extraSkills;

        public override void OnEnter()
        {
            ManaCost = 6;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            manaComp = GetComponent<AsukaManaComponent>();
            extraSkills = outer.GetComponent<ExtraSkillLocator>();

            manaComp.TryDrawDiscard(extraSkills.extraFirst);
            manaComp.TryDrawDiscard(extraSkills.extraSecond);
            manaComp.TryDrawDiscard(extraSkills.extraThird);
            manaComp.TryDrawDiscard(extraSkills.extraFourth);

            manaComp.DrawIntoHand(extraSkills.extraFirst);
            manaComp.DrawIntoHand(extraSkills.extraSecond);
            manaComp.DrawIntoHand(extraSkills.extraThird);
            manaComp.DrawIntoHand(extraSkills.extraFourth);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            
        }
    }
}
