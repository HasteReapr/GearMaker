using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class GoToMarker : BaseSkillState
    {
        //This is an uninteruptible teleport skill, just like huntress blink again, lol.
        private Ray aimRay;
        public float baseDuration = 0.35f;
        public float duration;
        private Animator animator;

        private Transform modelTransform;
        public static GameObject blinkPrefab;
        public float spdCoef = 25f;
        public string beginSoundString;
        public string endSoundString;

        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        private HurtBoxGroup hurtboxGroupTransform;
        private float stopwatch;
        private Vector3 blinkVector = Vector3.zero;

        public override void OnEnter()
        {
            base.OnEnter();
            aimRay = GetAimRay();
            duration = baseDuration / moveSpeedStat;

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

        public override void OnExit()
        {
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

            base.OnExit();
            //Here we unset the skill override, so it should default to the "empty" card slot.
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
            return ((inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector).normalized;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
