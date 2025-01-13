using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    public class BookFire : BaseSkillState
    {
        public static float damageCoefficient = AsukaStaticValues.bookDamageCoef;
        public static float baseDuration = 0.1f; //We have a very weak but very fast firing hitscan weapon as the primary.

        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        private AsukaTracker tracker;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            tracker = GetComponent<AsukaTracker>();

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();

                    RaycastHit raycastHit;
                    Physics.Raycast(aimRay, out raycastHit, 9999, LayerIndex.CommonMasks.bullet);
                    
                    float offsetX = Random.Range(-4, 4);
                    float offsetY = Random.Range(0, 3);
                    float offsetZ = Random.Range(-4, 4);

                    Vector3 angle = new Vector3(offsetX, offsetY, offsetZ);

                    Vector3 newOrig = aimRay.origin + angle;
                    Vector3 origDir = raycastHit.point - newOrig;
                    Vector3 newDirection = origDir / origDir.magnitude;

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = raycastHit.point == Vector3.zero ? aimRay.direction : newDirection,
                        origin = newOrig,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = 256,
                        force = 0,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = 1,
                        radius = 0.06f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        //tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    }.Fire();

                    EffectData tracerData = new EffectData();
                    tracerData.Reset();
                    tracerData.origin = raycastHit.point == Vector3.zero ? aimRay.direction : raycastHit.point;
                    tracerData.start = newOrig;

                    EffectManager.SpawnEffect(tracerEffectPrefab, tracerData, true);

                    tracerData.Reset();
                    tracerData.origin = newOrig;
                    EffectManager.SpawnEffect(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, tracerData, true);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}