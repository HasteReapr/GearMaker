using RoR2;
using UnityEngine;
using AsukaMod.Modules;
using System;
using RoR2.Projectile;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;

        //Spell Icons
        //We wanna load these here because we do not wanna be reloading all of these every single time we load into a new map.


        //Basic cube metrons
        public static GameObject HowlingMetron;
        public static GameObject DelayedHowlingMetron;
        public static GameObject HowlingMetronMSProcessing;
        public static GameObject MetronArpeggio;
        public static GameObject DelayedTardusMetron;
        public static GameObject BitShiftMetron; // How do we go about upgrading this? It would need to be slowly upgraded while the card is held, but dunno how to do that lol. Maybe some internal timer? But they need a timer per slot.

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("AsukaSwordHit");

            CreateEffects();

            CreateProjectiles();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("AsukaSwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactAsukaSlash");
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "AsukaBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateBombProjectile();
            Content.AddProjectilePrefab(bombProjectilePrefab);

            CreateHowlingMetron();
            Content.AddProjectilePrefab(HowlingMetron);
        }

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            bombProjectilePrefab = Asset.CloneProjectilePrefab("CommandoGrenadeProjectile", "AsukaBombProjectile");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(bombProjectilePrefab.GetComponent<ProjectileImpactExplosion>());
            ProjectileImpactExplosion bombImpactExplosion = bombProjectilePrefab.AddComponent<ProjectileImpactExplosion>();
            
            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.blastDamageCoefficient = 1f;
            bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = bombExplosionEffect;
            bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("AsukaBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombProjectilePrefab.GetComponent<ProjectileController>();

            if (_assetBundle.LoadAsset<GameObject>("AsukaBombGhost") != null)
                bombController.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("AsukaBombGhost");
            
            bombController.startSound = "";
        }

        private static void CreateHowlingMetron()
        {
            HowlingMetron = Asset.CloneProjectilePrefab("Bandit2ShivProjectile", "Howling Metron");

            HowlingMetron.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            Rigidbody metronRigidBody = HowlingMetron.GetComponent<Rigidbody>();
            if (!metronRigidBody)
            {
                metronRigidBody = HowlingMetron.AddComponent<Rigidbody>();
            }

            ProjectileController metronController = HowlingMetron.GetComponent<ProjectileController>();
            metronController.rigidbody = metronRigidBody;
            metronController.rigidbody.useGravity = false;
            metronController.procCoefficient = 1f;

            //metronController.ghostPrefab = Asset.CreateProjectileGhostPrefab(_assetBundle, "mdlKnife");

            UnityEngine.Object.Destroy(HowlingMetron.transform.GetChild(0).gameObject);
        }
        #endregion projectiles
    }
}