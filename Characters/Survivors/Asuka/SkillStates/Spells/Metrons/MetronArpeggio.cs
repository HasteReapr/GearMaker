﻿using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class MetronArpeggio : BaseSpellState
    {
        private Ray aimRay;
        public float baseDuration = 1.5f;
        public float duration;
        private float fireTime;
        private Animator animator;
        private bool hasFired;
        private float damageCoef = AsukaStaticValues.DelayedHowlingMetronCoef;

        public override void OnEnter()
        {
            ManaCost = 24;
            base.OnEnter();
            if (CastFailed) return;
            
            aimRay = GetAimRay();
            fireTime = 0.21f / attackSpeedStat;
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            PlayCrossfade("Gesture, Override", "CAST_SPIN", "CAST_SPIN.playbackRate", 1, 0.1f);
            
        }

        private void Fire()
        {
            hasFired = true;
            canOverride = true;
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
                    projectilePrefab = AsukaAssets.MetronArpeggio,
                    //Spawn a projectile with 0 speed that stays still and spawns the 10 cubes.
                    speedOverride = 0,
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
