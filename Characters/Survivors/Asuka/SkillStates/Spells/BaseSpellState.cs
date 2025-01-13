using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using AsukaMod.Survivors.Asuka.Components;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class BaseSpellState : BaseSkillState
    {
        //The BaseSpellState will play the FailedCast animation, and then the other things override it.
        public float duration = 0;

        public float ManaCost = 0;
        public bool CastFailed = false;

        public override void OnEnter()
        {
            base.OnEnter();

            //If our current mana is less than our mana cost, we do our fail cast animation.
            if(GetComponent<AsukaManaComponent>().mana < ManaCost)
            {
                CastFailed = true;
                duration = 1.5f / attackSpeedStat; //We divide by the attack speed stat so it's less punishing to try and cast a spell without enough mana later in the run.
            }
        }

        public override void OnExit()
        {
            base.OnExit();
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
