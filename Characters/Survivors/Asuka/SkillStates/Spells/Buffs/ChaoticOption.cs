using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class ChaoticOption : BaseSpellState
    {
        public float baseDuration = 0.29f;
        public float duration;
        private Animator animator;

        public override void OnEnter()
        {
            ManaCost = 8;
            base.OnEnter();
            if (CastFailed) return;

            duration = baseDuration / attackSpeedStat;

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

        public override void OnExit()
        {
            base.OnExit();
            if (!CastFailed)
            {
                manaComp.DiscardFromHand(activatorSkillSlot);

                manaComp.DrawIntoHand(activatorSkillSlot);
            }
        }
    }
}
