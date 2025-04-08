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
            base.OnEnter();
            if (CastFailed) return;

            aimRay = GetAimRay();
            fireTime = 0.21f / attackSpeedStat;
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            
            PlayCrossfade("Gesture, Override", "CAST_SLASH", "CAST_SLASH.playbackRate", 1, 0.1f);
        }

        private void Fire()
        {
            hasFired = true;
            canOverride = true;
            //Instead of *actually* firing we send this to a queue that gets fired.
            
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
                    projectilePrefab = AsukaAssets.BitShiftMetron,
                    speedOverride = 64,
                };

                Chat.AddMessage($"Sent bitshift info with a count of {activatorSkillSlot.stock}");
                manaComp.AddBitShift(activatorSkillSlot.stock + 1, info);
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
