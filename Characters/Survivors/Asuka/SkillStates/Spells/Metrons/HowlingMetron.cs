using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class HowlingMetron : BaseSkillState
    {
        private Ray aimRay;
        public float baseDuration = 0.39f;
        public float duration;
        private float fireTime;
        private Animator animator;
        private bool hasFired;
        private float damageCoef = AsukaStaticValues.HowlingMetronCoef;

        public override void OnEnter()
        {
            base.OnEnter();
            aimRay = GetAimRay();
            hasFired = false;
            fireTime = 0.11f / attackSpeedStat;
            duration = baseDuration / attackSpeedStat;
        }

        public override void OnExit()
        {
            base.OnExit();
            //Here we unset the skill override, so it should default to the "empty" card slot.
        }

        private void Fire()
        {
            hasFired = true;
            if (isAuthority)
            {
                FireProjectileInfo info = new FireProjectileInfo()
                {
                    owner = gameObject,
                    damage = damageCoef * characterBody.damage,
                    force = 0,
                    position = aimRay.origin,
                    crit = characterBody.RollCrit(),
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    projectilePrefab = AsukaAssets.HowlingMetron,
                    speedOverride = 128,
                };

                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(fixedAge >= fireTime && !hasFired)
            {
                Fire();
            }

            if(fixedAge >= duration && hasFired)
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
