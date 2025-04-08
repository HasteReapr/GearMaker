using BepInEx;
using AsukaMod.Survivors.Asuka;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using AsukaMod.Survivors.Asuka.Components;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace AsukaMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.KingEnderBrine.ExtraSkillSlots", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class AsukaPlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.hastereapr.AsukaMod";
        public const string MODNAME = "AsukaMod";
        public const string MODVERSION = "0.0.1";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "HASTEREAPR";

        public static AsukaPlugin instance;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new AsukaSurvivor().Initialize();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();

            Hook();
        }

        private void Hook()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body.HasBuff(AsukaBuffs.manaDefBuff))
            {
                damageInfo.damage *= 0.25f; //With mana up, Asuka's effective health is 60*4.
            }

            if(self.TryGetComponent(out AsukaManaComponent passiveCtrl)) //If we are Askua we need to get our mana component and reduce mana.
            {
                if (!passiveCtrl.inManaStun)
                {
                    if(passiveCtrl.mana - (damageInfo.damage * passiveCtrl.manaLostOnHitMult) <= 0)
                    {
                        passiveCtrl.inManaStun = true;
                        self.body.RemoveBuff(AsukaBuffs.manaDefBuff);
                    }
                    passiveCtrl.AddMana((damageInfo.damage * passiveCtrl.manaLostOnHitMult) * -1);
                }
            }

            orig(self, damageInfo);
        }
    }
}
