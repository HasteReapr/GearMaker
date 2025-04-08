using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class GoToMarker : BaseSpellState
    {
        //This is an uninteruptible teleport skill, just like huntress blink again, lol.
        private Ray aimRay;
        public float baseDuration = 0.35f;
        public float duration;
        private Animator animator;

        private Transform modelTransform;
        public static GameObject blinkPrefab;
        public float spdCoef = 120;

        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        private float stopwatch;
        private Vector3 blinkVector = Vector3.zero;

        public override void OnEnter()
        {
            ManaCost = 8;
            base.OnEnter();

            if (CastFailed) return;

            if (!CastFailed)
            {
                aimRay = GetAimRay();
                duration = baseDuration / moveSpeedStat;

                PlayCrossfade("Gesture, Override", "CAST_HOVER", "CAST_HOVER.playbackRate", duration, 0.1f);

                modelTransform = GetModelTransform();
                if (modelTransform)
                {
                    characterModel = modelTransform.GetComponent<CharacterModel>();
                    hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
                }
                if (characterModel)
                {
                    characterModel.invisibilityCount++;
                }
                if (hurtboxGroup)
                {
                    HurtBoxGroup hbg = hurtboxGroup;
                    int hbDeactivatorCounter = hbg.hurtBoxesDeactivatorCounter + 1;
                    hbg.hurtBoxesDeactivatorCounter = hbDeactivatorCounter;
                }
                blinkVector = GetBlinkVector();

                Util.PlaySound("Play_huntress_shift_start", gameObject);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }
            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }
            if (characterMotor)
            {
                characterMotor.disableAirControlUntilCollision = false;
            }

            Util.PlaySound("Play_huntress_shift_end", gameObject);

            if (!CastFailed)
            {
                if (HasBuff(AsukaBuffs.recycleBuff))
                    characterBody.RemoveBuff(AsukaBuffs.recycleBuff);
                else
                    manaComp.DiscardFromHand(activatorSkillSlot);

                if (HasBuff(AsukaBuffs.bookmarkAuto) && !HasBuff(AsukaBuffs.recycleBuff))
                    manaComp.DrawIntoHand(activatorSkillSlot);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += GetDeltaTime();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += blinkVector * (moveSpeedStat * spdCoef * GetDeltaTime());
            }

            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private Vector3 GetBlinkVector()
        {
            return (inputBank.moveVector == Vector3.zero) ? inputBank.aimDirection : inputBank.moveVector.normalized;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
