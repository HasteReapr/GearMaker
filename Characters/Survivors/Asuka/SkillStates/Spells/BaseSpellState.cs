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
    internal class BaseSpellState : BaseSkillState
    {
        //The BaseSpellState will play the FailedCast animation, and then the other things override it.
        public float duration = 0;

        public float ManaCost = 0;
        public bool CastFailed = false;
        public bool canOverride = false;
        public AsukaManaComponent manaComp;

        private ExtraSkillLocator extraSkillLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            manaComp = GetComponent<AsukaManaComponent>();
            extraSkillLocator = outer.GetComponent<ExtraSkillLocator>();

            //If our current mana is less than our mana cost or we are in mana stun, we do our fail cast animation.
            if (ManaCost < manaComp.mana && !manaComp.inManaStun)
            {
                manaComp.AddMana(-ManaCost);
            }
            else
            {
                CastFailed = true;
                duration = 1.5f / attackSpeedStat; //We divide by the attack speed stat so it's less punishing to try and cast a spell without enough mana later in the run.

                PlayCrossfade("Gesture, Override", "CAST_SLASH", "CAST_SLASH.playbackRate", duration, 0.1f);
                return;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > 0.75f)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!CastFailed)
            {
                //If we have the Sampler 404 buff we don't discard, and remove the buff.
                if (HasBuff(AsukaBuffs.recycleBuff))
                {
                    characterBody.RemoveBuff(AsukaBuffs.recycleBuff);
                }
                else
                    manaComp.DiscardFromHand(activatorSkillSlot);

                //If we have the auto bookmark buff and we DONT have the recycle buff we draw a new card.
                //There has to be a check for the Sampler 404 buff because auto draw was overwriting the recycle.
                if (HasBuff(AsukaBuffs.bookmarkAuto) && !HasBuff(AsukaBuffs.recycleBuff))
                    manaComp.DrawIntoHand(activatorSkillSlot);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return canOverride ? InterruptPriority.Skill : InterruptPriority.Pain;
            //return InterruptPriority.Pain;
        }
    }
}
