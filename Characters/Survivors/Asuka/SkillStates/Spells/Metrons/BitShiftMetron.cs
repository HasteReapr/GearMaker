using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class BitShiftMetron : BaseSpellState
    {
        private Ray aimRay;
        public float baseDuration = 0.54f;
        public float duration;
        private float fireTime;
        private Animator animator;
        private bool hasFired;
        private float damageCoef = AsukaStaticValues.DelayedHowlingMetronCoef;

        public override void OnEnter()
        {
            //Set our mana cost before we call base.OnEnter so if the mana cost is more than our current mana, we go into the failed cast state.
            ManaCost = 16;

            aimRay = GetAimRay();
            fireTime = 0.21f / attackSpeedStat;
            hasFired = false;
            duration = baseDuration / attackSpeedStat;

            base.OnEnter();
        }

        private void Fire()
        {
            hasFired = true;
            if (isAuthority)
            {
                //We gotta figure out how to check how many stocks we have
                FireProjectileInfo info = new FireProjectileInfo()
                {
                    owner = gameObject,
                    damage = damageCoef * characterBody.damage,
                    force = 0,
                    position = aimRay.origin,
                    crit = characterBody.RollCrit(),
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    projectilePrefab = AsukaAssets.BitShiftMetron,
                    speedOverride = 64,
                };

                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime && !hasFired && !CastFailed)
            {
                Fire();
            }

            if (fixedAge >= duration && (hasFired || CastFailed))
            {
                outer.SetNextStateToMain();
                return;
            }
        }
    }
}
