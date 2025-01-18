using RoR2;
using UnityEngine;
using AsukaMod.Modules;
using System;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.Networking;
using R2API.Utils;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaAssets
    {
        //UI
        public static GameObject ManaUI;
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;

        //Basic cube metrons
        public static GameObject HowlingMetron;
        public static GameObject DelayedHowlingMetron;
        public static GameObject HowlingMetronMSProcessing;
        public static GameObject MetronArpeggio;
        public static GameObject DelayedTardusMetron;
        public static GameObject BitShiftMetron;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            ManaUI = _assetBundle.LoadAsset<GameObject>("AsukaManaUI");
            ManaUI.transform.localScale = new Vector3(1, 1, 1);
            ManaUI.GetComponent<ImageFillController>().fillScalar = 1;

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

            CreateMetrons();
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

        private static void CreateMetrons()
        {
            HowlingMetron = _assetBundle.LoadAsset<GameObject>("HowlingMetron");
            var networkIdentity = HowlingMetron.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("0176acd452adc181"));
            }
            Content.AddProjectilePrefab(HowlingMetron);

            DelayedHowlingMetron = _assetBundle.LoadAsset<GameObject>("DelayedHowlingMetron");
            networkIdentity = DelayedHowlingMetron.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("0981acd452adc181"));
            }
            Content.AddProjectilePrefab(DelayedHowlingMetron);

            HowlingMetronMSProcessing = _assetBundle.LoadAsset<GameObject>("HowlingMetronProcessing");
            networkIdentity = HowlingMetronMSProcessing.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("0189acd452adc181"));
            }
            Content.AddProjectilePrefab(HowlingMetronMSProcessing);

            MetronArpeggio = _assetBundle.LoadAsset<GameObject>("SpaceCube");
            networkIdentity = MetronArpeggio.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("9810acd452adc181"));
            }
            Content.AddProjectilePrefab(MetronArpeggio);

            DelayedTardusMetron = _assetBundle.LoadAsset<GameObject>("DelayedTardus");
            networkIdentity = DelayedTardusMetron.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("1980acd452adc181"));
            }
            Content.AddProjectilePrefab(DelayedTardusMetron);

            BitShiftMetron = _assetBundle.LoadAsset<GameObject>("BitShiftMetron");
            networkIdentity = BitShiftMetron.GetComponent<NetworkIdentity>();
            if (networkIdentity)
            {
                networkIdentity.SetFieldValue("m_AssetId", NetworkHash128.Parse("7610acd452adc181"));
            }
            Content.AddProjectilePrefab(BitShiftMetron);
        }
        #endregion projectiles
    }
}