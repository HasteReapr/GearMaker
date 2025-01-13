using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class BookmarkFull : BaseSkillState
    {
        public float baseDuration = 0.29f;
        public float duration;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;


        }

        public override void OnExit()
        {
            base.OnExit();
            //Here we unset the skill override, so it should default to the "empty" card slot.
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
