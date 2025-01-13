using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class HowlingMetron : BaseSpellState
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
            //Set our mana cost before we call base.OnEnter so if the mana cost is more than our current mana, we go into the failed cast state.
            ManaCost = 8;
            base.OnEnter();
            //Check to make sure our cast didn't fail. If it did we aren't able to do anything.
            if (!CastFailed)
            {
                aimRay = GetAimRay();
                hasFired = false;
                fireTime = 0.11f / attackSpeedStat;
                duration = baseDuration / attackSpeedStat;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
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

            if (CastFailed)
                return; //If we failed to cast we just exit out of FixedUpdate();

            if (fixedAge >= fireTime && !hasFired)
            {
                Fire();
            }

            if (fixedAge >= duration && hasFired)
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
