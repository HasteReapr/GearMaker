using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using RoR2.UI;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ExtraSkillSlots;
using AsukaMod.Modules;
using AsukaMod.Survivors.Asuka.Spells;

namespace AsukaMod.Survivors.Asuka.Components
{
    //Huge thanks to the folks over at the Starstorm 2 Team, especially Swuff for reaching out and providing the Nemesis Captain stuff to use as reference here.
    //https://github.com/TeamMoonstorm/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/Components/NemCaptain/NemCaptainController.cs

    public class AsukaManaComponent : NetworkBehaviour
    {
        //Cached on wakeup
        public CharacterBody charBody;
        public Animator charAnim;
        public ExtraSkillLocator exSkillLoc;

        //Spell Skills & stuff
        public SkillFamily deck;
        public SkillDef emptySpell;

        //Decks, these get initialized in a minute.
        public int SelectedDeck = 0; //0 deckA 1 deckB 2 deckC
        public SkillFamily deckA;
        private List<int> drawnSkillIndiciesA = new List<int>();
        public SkillFamily deckB;
        private List<int> drawnSkillIndiciesB = new List<int>();
        public SkillFamily deckC;
        private List<int> drawnSkillIndiciesC = new List<int>();

        private SkillFamily[] decks = new SkillFamily[3];
        private List<int>[] deckDrawIndex = new List<int>[3];

        private bool initialDeck = true;
        
        //Mana information
        public float minMana = 0;
        public float maxMana = 100;
        public float mpsInCombat = 0.5f; //mps is short for Mana Per Second.
        public float mpsOutOfCombat = 1f;
        public float mpsWhileRegen = 5f;
        public float mpsRecoverContinous = 18f; // Recover Mana Continous buff.
        public float manaGainedInstant = 50; // Recover Mana Instant. Doesn't consume cards, and just recovers your mana.
        public float manaGainedOnKill = 2f;
        public float manaLostOnHitMult = 0.125f; //this is a multiplier for the damage, incoming damage gets multiplied by this and this is how much mana you lose. So if you took 10 damage you lose 1 mana.

        //Mana UI Stuff
        [SerializeField]
        [Header("UI")]
        public GameObject cardOverlay;
        public string cardOverlayChildLocStr;
        private ChildLocator cardOverlayerChildLoc;

        //Real Mana Values
        [SyncVar(hook = "OnManaModified")]
        private float _mana;

        public float mana
        {
            get
            {
                return _mana;
            }
        }

        public float manaFrac
        {
            get
            {
                return mana / maxMana;
            }
        }

        public float manaPerc
        {
            get
            {
                return manaFrac * 100;
            }
        }

        public bool isFullMana
        {
            get
            {
                return mana >= maxMana;
            }
        }

        private HealthComponent bodyHealthComponent
        {
            get
            {
                return charBody.healthComponent;
            }
        }

        public float Network_Mana
        {
            get
            {
                return _mana;
            }
            [param: In]
            set
            {
                if(NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;

                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _mana, 1U);
            }
        }

        [Server]
        public void AddMana(float amount)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void AsukaMod.AsukaManaComponent::AddMana(System.Single)' called on client.");
                return;
            }

            //Reduce the mana cost by 50% if we have the Reduce Mana Cost buff.
            if (charBody.HasBuff(AsukaBuffs.manaDefBuff) && amount < 0) //the <0 bit is because we use AddMana(-skillCost) to remove mana, instead of using a seperate function, so if it is less than 0 it is expending mana.
                amount *= 0.5f;

            this.Network_Mana = Mathf.Clamp(this.mana + amount, 0, maxMana);
        }

        //finally we get into the meat of this component lol
        private void OnEnable()
        {
            charBody = base.gameObject.GetComponent<CharacterBody>();
            charAnim = base.gameObject.GetComponent<Animator>();
            exSkillLoc = base.gameObject.GetComponent<ExtraSkillLocator>();

            /*            //add prefab & necessary hooks
            OverlayCreationParams manaOverlayCreationParams = new OverlayCreationParams
            {
                prefab = manaOverlayPrefab,
                childLocatorEntry = manaOverlayChildLocatorEntry
            };
            manaOverlayController = HudOverlayManager.AddOverlay(gameObject, manaOverlayCreationParams);
            manaOverlayController.onInstanceAdded += OnManaOverlayInstanceAdded;
            manaOverlayController.onInstanceRemove += OnManaOverlayInstanceRemoved;

            OverlayCreationParams cardOverlayCreationParams = new OverlayCreationParams
            {
                prefab = cardOverlayPrefab,
                childLocatorEntry = cardOverlayChildLocatorEntry
            };
            cardOverlayController = HudOverlayManager.AddOverlay(gameObject, cardOverlayCreationParams);
            cardOverlayController.onInstanceAdded += OnCardOverlayInstanceAdded;
            cardOverlayController.onInstanceRemove += OnCardOverlayInstanceRemoved;*/

            //Check for a character body
            if (charBody != null)
            {
                if (NetworkServer.active)
                {
                    HealthComponent.onCharacterHealServer += HealthComponent_onCharacterHealServer;
                }
                //Populate and setup the decks in the arrays first, for easier tracking.
                //Initialize();
                //This method is more versatile than StarStorm's version, as it allows you to choose which deck is used.

                // We initialize the decks, each one being it's own unique skill family.
                //TODO Actually figure out how to use skill families, this is gonna end up being a kind of jumbled messs I think
                InitDecks();
                
                //We give Asuka the defense buff when we spawn in. This buff gets removed when you run out of mana.
                charBody.AddBuff(AsukaBuffs.manaDefBuff);

                InitHand();
            }
        }

        private void FixedUpdate()
        {
            if (charBody.HasBuff(AsukaBuffs.manaRegenCont))
            {
                AddMana(mpsRecoverContinous);
            }
        }

        /*private void Initialize()
        {
            decks[0] = deckA;
            decks[1] = deckB;
            decks[2] = deckC;
            deckDrawIndex[0] = drawnSkillIndiciesA;
            deckDrawIndex[1] = drawnSkillIndiciesB;
            deckDrawIndex[2] = drawnSkillIndiciesC;
        }*/

        private void InitHand()
        {
            //Reset on Start.
            if (initialDeck)
            {
                drawnSkillIndiciesA.Clear();
                initialDeck = false;
            }
            DrawFullHand();
        }

        //Full draw, used for Bookmark Random Import, and initializing.
        private void DrawFullHand()
        {
            //Draw a card in each hand.
            exSkillLoc.extraFirst.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);
            if (exSkillLoc.extraFirst.skillDef == emptySpell)
                exSkillLoc.extraFirst.UnsetSkillOverride(gameObject, exSkillLoc.extraFirst.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraSecond.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);
            if (exSkillLoc.extraSecond.skillDef == emptySpell)
                exSkillLoc.extraSecond.UnsetSkillOverride(gameObject, exSkillLoc.extraSecond.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraThird.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);
            if (exSkillLoc.extraThird.skillDef == emptySpell)
                exSkillLoc.extraThird.UnsetSkillOverride(gameObject, exSkillLoc.extraThird.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraFourth.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);
            if (exSkillLoc.extraFourth.skillDef == emptySpell)
                exSkillLoc.extraFourth.UnsetSkillOverride(gameObject, exSkillLoc.extraFourth.skillDef, GenericSkill.SkillOverridePriority.Replacement);
        }

        //Bookmark grab
        public SkillDef DrawFromDeck(int deckNum)
        {
            //Check if the deck has been fully used.
            if (GetDrawCountByIndex(deckNum).Count == GetDeckByIndex(deckNum).variants.Length)
            {
                GetDrawCountByIndex(deckNum).Clear();
            }

            //Start to pull randomly from the deck
            int randomVarInd;
            do
            {
                randomVarInd = Random.Range(0, GetDeckByIndex(deckNum).variants.Length);
            }
            while (GetDrawCountByIndex(deckNum).Contains(randomVarInd));

            //Mark the spell as used.
            GetDrawCountByIndex(deckNum).Add(randomVarInd);

            return GetDeckByIndex(deckNum).variants[randomVarInd].skillDef;
        }

        //Bookmark remove
        public void DiscardFromHand(int handIndex)
        {
            GenericSkill hand = GetHandByIndex(handIndex);
            if(hand != null)
            {
                //Discard the hand.
                hand.UnsetSkillOverride(gameObject, hand.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                hand.SetSkillOverride(gameObject, emptySpell, GenericSkill.SkillOverridePriority.Replacement);
            }
        }

        private GenericSkill GetHandByIndex(int handIndex)
        {
            switch (handIndex)
            {
                case 1:
                    return exSkillLoc.extraFirst;
                case 2:
                    return exSkillLoc.extraSecond;
                case 3:
                    return exSkillLoc.extraThird;
                case 4:
                    return exSkillLoc.extraFourth;
                default:
                    return null;
            }
        }

        private SkillFamily GetDeckByIndex(int deckIndex)
        {
            switch (deckIndex)
            {
                case 0:
                    return deckA;
                case 1:
                    return deckB;
                case 2:
                    return deckC;
                default:
                    return null;
            }
        }

        private List<int> GetDrawCountByIndex(int deckIndex)
        {
            switch (deckIndex)
            {
                case 0:
                    return drawnSkillIndiciesA;
                case 1:
                    return drawnSkillIndiciesB;
                case 2:
                    return drawnSkillIndiciesC;
                default:
                    return null;
            }
        }

        private void HealthComponent_onCharacterHealServer(HealthComponent arg1, float arg2, ProcChainMask arg3)
        {
            
        }

        //Here at the very bottom we fill out all of the decks with our cards
        public void InitDecks()
        {
            BaseSpell GoToMarker = new BaseSpell
            {
                skillName = "GoToMarker",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_GO_TO_MARKER",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 8,

                activationState = new EntityStates.SerializableEntityStateType(typeof(GoToMarker)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant MarkerVar = new SkillFamily.Variant();
            MarkerVar.skillDef = GoToMarker;

            BaseSpell ReduceManaCost = new BaseSpell
            {
                skillName = "ReduceManaCost",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_REDUCE_MANA_COST",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 6,

                activationState = new EntityStates.SerializableEntityStateType(typeof(ReduceManaCost)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant ReduceManaVar = new SkillFamily.Variant();
            ReduceManaVar.skillDef = ReduceManaCost;

            BaseSpell RegenManaCont = new BaseSpell
            {
                skillName = "RegenManaContinuous",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_REGEN_MANA_CONTINUOUS",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 8,

                activationState = new EntityStates.SerializableEntityStateType(typeof(ManaRegenCont)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant RegenContVar = new SkillFamily.Variant();
            RegenContVar.skillDef = RegenManaCont;

            BaseSpell RegenManaInstant = new BaseSpell
            {
                skillName = "RegenManaInstant",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_REGEN_MANA_INSTANT",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 8,

                activationState = new EntityStates.SerializableEntityStateType(typeof(ManaRegenInstant)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant InstantManaVar = new SkillFamily.Variant();
            InstantManaVar.skillDef = RegenManaInstant;

            BaseSpell HowlingMetron = new BaseSpell
            {
                skillName = "HowlingMetron",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_HOWLING_METRON",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 8,

                activationState = new EntityStates.SerializableEntityStateType(typeof(HowlingMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant HowlingMetVar = new SkillFamily.Variant();
            HowlingMetVar.skillDef = HowlingMetron;

            BaseSpell DelayedHowlingMetron = new BaseSpell
            {
                skillName = "DelayedHowlingMetron",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_DELAYED_HOWLING_METRON",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 12,

                activationState = new EntityStates.SerializableEntityStateType(typeof(DelayedHowlingMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant DelHowlMetVar = new SkillFamily.Variant();
            DelHowlMetVar.skillDef = DelayedHowlingMetron;

            BaseSpell HowlingMetronProcess = new BaseSpell
            {
                skillName = "HowlingMetronMSProcessing",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_HOWLING_METRON_MS_PROCESS",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 16,

                activationState = new EntityStates.SerializableEntityStateType(typeof(HowlingMSProcess)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant MetronProcessVar = new SkillFamily.Variant();
            MetronProcessVar.skillDef = HowlingMetronProcess;

            BaseSpell DelayedTardusMetron = new BaseSpell
            {
                skillName = "DelayedTardusMetron",
                skillNameToken = AsukaSurvivor.Asuka_PREFIX + "SPELL_DELAYED_TARDUS_METRON",
                //icon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),
                manaCost = 12,

                activationState = new EntityStates.SerializableEntityStateType(typeof(DelayedTardusMetron)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 0f,
                baseMaxStock = 1,

                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            };
            SkillFamily.Variant TardusVar = new SkillFamily.Variant();
            TardusVar.skillDef = DelayedTardusMetron;

            deckA = new SkillFamily();
            SkillFamily.Variant[] deckACards = { 
                ReduceManaVar, ReduceManaVar, 
                HowlingMetVar, HowlingMetVar, HowlingMetVar, HowlingMetVar, HowlingMetVar, HowlingMetVar, 
                DelHowlMetVar, DelHowlMetVar, DelHowlMetVar, DelHowlMetVar, DelHowlMetVar,
                MetronProcessVar, MetronProcessVar, MetronProcessVar, MetronProcessVar, MetronProcessVar,
            };
            deckA.variants = deckACards;

            deckB = new SkillFamily();
            SkillFamily.Variant[] deckBCards =
            {

            };
        }
    }
}