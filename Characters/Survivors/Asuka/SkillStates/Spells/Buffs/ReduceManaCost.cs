using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class ReduceManaCost : BaseSpellState
    {
        public float baseDuration = 0.29f;
        public float duration;
        private Animator animator;

        public override void OnEnter()
        {
            ManaCost = 6;
            base.OnEnter();
            if (CastFailed) return;

            duration = baseDuration / attackSpeedStat;
            characterBody.AddTimedBuff(AsukaBuffs.reduceManaCost, 10);

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
