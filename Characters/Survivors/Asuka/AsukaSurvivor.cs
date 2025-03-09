using BepInEx.Configuration;
using AsukaMod.Modules;
using AsukaMod.Modules.Characters;
using AsukaMod.Survivors.Asuka.Components;
using AsukaMod.Survivors.Asuka.SkillStates;
using RoR2;
using RoR2.Skills;
using R2API;
using System;
using System.Collections.Generic;
using UnityEngine;
using ExtraSkillSlots;
using AsukaMod.Survivors.Asuka.Spells;

namespace AsukaMod.Survivors.Asuka
{
    public class AsukaSurvivor : SurvivorBase<AsukaSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "asukaplaceholderbundle"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "AsukaBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "AsukaMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlAsuka";
        public override string displayPrefabName => "AsukaDisplay";

        public const string Asuka_PREFIX = AsukaPlugin.DEVELOPER_PREFIX + "_Asuka_";

        public static Dictionary<string, BaseSpell> SpellDict = new Dictionary<string, BaseSpell>();
        public static Dictionary<string, SkillDef> SpellSkills = new Dictionary<string, SkillDef>();
        public static SkillDef emptySpell;

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => Asuka_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = Asuka_PREFIX + "NAME",
            subtitleNameToken = Asuka_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texAsukaIcon"),
            bodyColor = Color.white,
            sortPosition = 100,

            crosshair = Asset.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 60f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = assetBundle.LoadMaterial("matAsuka"),
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                },
                new CustomRendererInfo
                {
                    childName = "Model",
                }
        };

        public override UnlockableDef characterUnlockableDef => AsukaUnlockables.characterUnlockableDef;
        
        public override ItemDisplaysBase itemDisplays => new AsukaItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Asuka");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            AsukaUnlockables.Init();

            base.InitializeCharacter();

            AsukaConfig.Init();
            AsukaStates.Init();
            AsukaTokens.Init();

            AsukaAssets.Init(assetBundle);
            AsukaBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<AsukaManaComponent>();
            bodyPrefab.AddComponent<ExtraInputBankTest>();
            bodyPrefab.AddComponent<AsukaTracker>();
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");

            Prefabs.SetupHitBoxGroup(characterModelObject, "SpellsA", "ScreamerHB");
            Prefabs.SetupHitBoxGroup(characterModelObject, "SpellsB", "TerraHB");
            Prefabs.SetupHitBoxGroup(characterModelObject, "SpellsC", "BoostHB");

            ChildLocator childLocator = characterModelObject.GetComponent<ChildLocator>();

            Transform screamerTransform = childLocator.FindChild("ScreamerHB");
            Prefabs.SetupHitbox(prefabCharacterModel.gameObject, screamerTransform, "ScreamerHitbox");
            Transform terraTransform = childLocator.FindChild("TerraHB");
            Prefabs.SetupHitbox(prefabCharacterModel.gameObject, screamerTransform, "TerraHitbox");
            Transform rmsTransform = childLocator.FindChild("BoostHB");
            Prefabs.SetupHitbox(prefabCharacterModel.gameObject, screamerTransform, "RMSBoostHitbox");
        }

        public override void InitializeEntityStateMachines() 
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
                //don't forget to register custom entitystates in your AsukaStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            //AddPassiveSkill();
            InitializeSpells();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = Asuka_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = Asuka_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "AsukaPassive",
                skillNameToken = Asuka_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = Asuka_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            SkillDef primarySkillDef = Skills.CreateSkillDef<SkillDef>(new SkillDefInfo
                (
                    "AsukaBook",
                    Asuka_PREFIX + "PRIMARY_BOOK_NAME",
                    Asuka_PREFIX + "PRIMARY_BOOK_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(BookFire)),
                    "Weapon",
                    true
                ));

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            SkillDef secondarySkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "AsukaBookmarkSpells",
                skillNameToken = Asuka_PREFIX + "SECONDARY_BOOKMARK_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SECONDARY_BOOKMARK_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Bookmark)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Pain,

                baseRechargeInterval = 0,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef);
        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "AsukaDeckCycle",
                skillNameToken = Asuka_PREFIX + "UTILITY_DECKSWAP_NAME",
                skillDescriptionToken = Asuka_PREFIX + "UTILITY_DECKSWAP_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SwapTestCase)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Pain,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "AsukaManaRecover",
                skillNameToken = Asuka_PREFIX + "SPECIAL_RECOVER_MANA_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPECIAL_RECOVER_MANA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(RecoverMana)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Pain,

                baseRechargeInterval = 0,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0,

                isCombatSkill = false,
                mustKeyPress = true,
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
        }

        //These are all of Asuka's spells. They are made into SkillDefs with the BaseSpell thing so we can handle mana and stuff
        public void InitializeSpells()
        {
            ExtraSkillLocator exSkillLoc = bodyPrefab.AddComponent<ExtraSkillLocator>();

            //Create new SkillFamily
            var spellSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();

            //Adding skill variants to the family
            (spellSkillFamily as ScriptableObject).name = bodyPrefab.name + "Spell" + "Family";
            spellSkillFamily.variants = new SkillFamily.Variant[0];

            Content.AddSkillFamily(spellSkillFamily);

            emptySpell = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "EmptySpell",
                skillNameToken = Asuka_PREFIX + "SPELL_EMPTY_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_EMPTY_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(EmptySpell)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 0f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("EmptySpell", emptySpell);

            SkillDef howlingMetron = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HowlingMetron",
                skillNameToken = Asuka_PREFIX + "SPELL_HOWLING_METRON_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_HOWLING_METRON_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("HowlingMetron"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(HowlingMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("HowlingMetron", howlingMetron);
            SkillDef delayedHowling = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "DelayedHowlingMetron",
                skillNameToken = Asuka_PREFIX + "SPELL_DELAYED_HOWLING_METRON_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_DELAYED_HOWLING_METRON_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("DelayedHowlingMetron"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DelayedHowlingMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("DelayedHowlingMetron", delayedHowling);
            SkillDef delayedTardusMetron = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "DelayedTardusMetron",
                skillNameToken = Asuka_PREFIX + "SPELL_DELAYED_TARDUS_METRON_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_DELAYED_TARDUS_METRON_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("DelayedTardusMetron"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DelayedTardusMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("DelayedTardusMetron", delayedTardusMetron);
            SkillDef howlingMSProcess = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HowlingMetronMSProcess",
                skillNameToken = Asuka_PREFIX + "SPELL_HOWLING_METRON_MS_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_HOWLING_METRON_MS_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("HowlingMetronMSProcessessing"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(HowlingMSProcess)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("HowlingMSProcess", howlingMSProcess);
            SkillDef arpeggioMetron = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "MetronArpeggio",
                skillNameToken = Asuka_PREFIX + "SPELL_METRON_ARPEGGIO_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_METRON_ARPEGGIO_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("MetronArpeggio"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(MetronArpeggio)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("MetronArpeggio", arpeggioMetron);
            SkillDef bitShiftMetron = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "BitShiftMetron",
                skillNameToken = Asuka_PREFIX + "SPELL_BIT_SHIFT_METRON_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_BIT_SHIFT_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("BitShiftMetron"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(BitShiftMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                fullRestockOnAssign = false,
                requiredStock = 0,
                baseMaxStock = 4,
                baseRechargeInterval = 5f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("BitShiftMetron", bitShiftMetron);
            SkillDef metronScreamer = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "MetronScreamer",
                skillNameToken = Asuka_PREFIX + "SPELL_METRON_SCREAMER_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_METRON_SCREAMER_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("MetronScreamer808"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(MetronScreamer808)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 0f,

                isCombatSkill = true,
                mustKeyPress = false,
            });
            SpellSkills.Add("MetronScreamer", metronScreamer);

            SkillDef goToMarker = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "goToMarker",
                skillNameToken = Asuka_PREFIX + "SPELL_TELEPORT_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_TELEPORT_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("goToMarker"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(GoToMarker)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("GoToMarker", goToMarker);
            SkillDef sampler404 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Sampler404",
                skillNameToken = Asuka_PREFIX + "SPELL_SAMPLER_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_SAMPLER_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("Sampler404"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Sampler404)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("Sampler404", sampler404);
            SkillDef chaoticOption = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "chaoticOption",
                skillNameToken = Asuka_PREFIX + "SPELL_CHAOTIC_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_CHAOTIC_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("ChaoticOption"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ChaoticOption)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("ChaoticOption", chaoticOption);

            SkillDef reduceMana = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "ReduceManaCost",
                skillNameToken = Asuka_PREFIX + "SPELL_REDUCE_MANA_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_REDUCE_MANA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("ManaReduce"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ReduceManaCost)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("ReduceManaCost", reduceMana);
            SkillDef regenMana = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "regenMana",
                skillNameToken = Asuka_PREFIX + "SPELL_REGEN_MANA_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_REGEN_MANA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("ManaRegen"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ManaRegenCont)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("RegenMana", regenMana);
            SkillDef recoverMana = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SpellRecoverMana",
                skillNameToken = Asuka_PREFIX + "SPELL_RECOVER_MANA_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_RECOVER_MANA_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("ManaRecover"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ManaRegenInstant)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("RecoverMana", recoverMana);

            SkillDef bookmarkAuto = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "BookmarkAutoImport",
                skillNameToken = Asuka_PREFIX + "SPELL_BOOKMARK_AUTO_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_BOOKMARK_AUTO_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("BookmarkAutoImport"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(BookmarkAuto)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("BookmarkAuto", bookmarkAuto);
            SkillDef bookmarkRand = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "BookmarkRandom",
                skillNameToken = Asuka_PREFIX + "SPELL_BOOKMARK_RAND_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_BOOKMARK_RAND_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("BookmarkRandomImport"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(BookmarkRandom)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("BookmarkRandom", bookmarkRand);
            SkillDef bookmarkFull = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SpellRecoverMana",
                skillNameToken = Asuka_PREFIX + "SPELL_BOOKMARK_FULL_NAME",
                skillDescriptionToken = Asuka_PREFIX + "SPELL_BOOKMARK_FULL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("BookmarkFullImport"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(BookmarkFull)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,

                isCombatSkill = false,
                mustKeyPress = false,
            });
            SpellSkills.Add("BookmarkFull", bookmarkFull);

            Skills.AddSkillToFamily(spellSkillFamily, emptySpell);

            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            passiveGenericSkill.SetLoadoutTitleTokenOverride("Spells");
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, emptySpell);

            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, howlingMetron);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, delayedHowling);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, howlingMSProcess);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, delayedTardusMetron);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, arpeggioMetron);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bitShiftMetron);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, metronScreamer);/*
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bitShiftMetron);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bitShiftMetron);*/

            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, goToMarker);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, sampler404);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, chaoticOption);

            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, reduceMana);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, recoverMana);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, regenMana);

            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bookmarkFull);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bookmarkRand);
            Skills.AddSkillToFamily(passiveGenericSkill.skillFamily, bookmarkAuto);

            //Adding new GenericSkill component to character prefab
            var SpellGenericSkillA = bodyPrefab.AddComponent<GenericSkill>();
            SpellGenericSkillA._skillFamily = spellSkillFamily;
            SpellGenericSkillA.hideInCharacterSelect = true;
            SpellGenericSkillA.SetHideInLoadout(true);

            var SpellGenericSkillB = bodyPrefab.AddComponent<GenericSkill>();
            SpellGenericSkillB._skillFamily = spellSkillFamily;
            SpellGenericSkillB.hideInCharacterSelect = true;
            SpellGenericSkillB.SetHideInLoadout(true);

            var SpellGenericSkillC = bodyPrefab.AddComponent<GenericSkill>();
            SpellGenericSkillC._skillFamily = spellSkillFamily;
            SpellGenericSkillC.hideInCharacterSelect = true;
            SpellGenericSkillC.SetHideInLoadout(true);

            var SpellGenericSkillD = bodyPrefab.AddComponent<GenericSkill>();
            SpellGenericSkillD._skillFamily = spellSkillFamily;
            SpellGenericSkillD.hideInCharacterSelect = true;
            SpellGenericSkillD.SetHideInLoadout(true);

            exSkillLoc.extraFirst = SpellGenericSkillA;
            exSkillLoc.extraSecond = SpellGenericSkillB;
            exSkillLoc.extraThird = SpellGenericSkillC;
            exSkillLoc.extraFourth = SpellGenericSkillD;
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshAsukaSword",
            //    "meshAsukaGun",
            //    "meshAsuka");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            
            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(Asuka_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    AsukaUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshAsukaSwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshAsukaAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matAsukaAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matAsukaAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matAsukaAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);
            
            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            AsukaAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private SkillFamily.Variant makeVariant(SkillDef skillDef)
        {
            return new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        private void AddHooks()
        {

        }
    }
}