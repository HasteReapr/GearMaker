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
            if (CastFailed) return;

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

            //PlayAnimation("Gesture, Override", "CAST_SPIN", "CAST_SPIN.playbackRate", 1.39f);
            Animator animator = GetModelAnimator();
            GetModelAnimator().SetFloat("CAST_SPIN.playbackRate", attackSpeedStat);

            PlayCrossfade("Gesture, Override", "CAST_SPIN", "CAST_SPIN.playbackRate", duration, 0.1f);
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
    }
}
