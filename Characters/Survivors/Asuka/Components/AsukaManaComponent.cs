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

    //Took a look at Lee Hyperreal for a better way to do a lot of this
    //https://github.com/Popcorn-Factory/lee-hyperreal-ror2/blob/d9fa573940e9c8d4276a363f3fedc75683c49a07/LeeHyperrealMod/Content/Controllers/OrbController.cs

    public class AsukaManaComponent : NetworkBehaviour
    {
        //Cached on wakeup
        CharacterBody charBody;
        CharacterMaster charMaster;
        EntityStateMachine[] stateMachines;
        ExtraSkillLocator exSkillLoc;

        public bool isExecutingSkill = false;
        public bool isCheckingInput = false;

        public float isCheckingInputTimer = 0f;
        public float isExecutingInputTimer = 0f;

        internal enum HandNum : ushort
        {
            PUNCH = 0,
            KICK = 1,
            SLASH = 2,
            HEAVY = 3
        }

        //Decks, these get initialized in a minute.
        public SkillDef emptySpell = AsukaSurvivor.emptySpell;

        public int SelectedDeck = 0; //0 deckA 1 deckB 2 deckC
        //public SkillFamily deckA;
        private List<SkillDef> deckA;
        private List<int> drawnSkillIndiciesA = new List<int>();
        public List<SkillDef> deckB;
        private List<int> drawnSkillIndiciesB = new List<int>();
        public List<SkillDef> deckC;
        private List<int> drawnSkillIndiciesC = new List<int>();

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

        public void Start()
        {
            charBody = gameObject.GetComponent<CharacterBody>();
            exSkillLoc = gameObject.GetComponent<ExtraSkillLocator>();
            stateMachines = gameObject.GetComponents<EntityStateMachine>();

            charMaster = charBody.master;
        }

        //finally we get into the meat of this component lol
        public void OnEnable()
        {
            //Check for a character body
            if (charBody != null)
            {
                //Populate and setup the decks in the arrays first, for easier tracking.
                PopulateDecks();
                
                //We give Asuka the defense buff when we spawn in. This buff gets removed when you run out of mana.
                charBody.AddBuff(AsukaBuffs.manaDefBuff);

                InitHand();
            }
        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {
            if (charBody.HasBuff(AsukaBuffs.manaRegenCont))
            {
                AddMana(mpsRecoverContinous * Time.fixedDeltaTime);
            }
        }

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
        public void DrawFullHand()
        {
            //Draw a card in each hand.
            //We unset the skill override in case it isn't the empty spell, then immedietly replace it with a spell.
            if (exSkillLoc.extraFirst.skillDef != emptySpell)
                exSkillLoc.extraFirst.UnsetSkillOverride(gameObject, exSkillLoc.extraFirst.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraFirst.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);

            if (exSkillLoc.extraSecond.skillDef != emptySpell)
                exSkillLoc.extraSecond.UnsetSkillOverride(gameObject, exSkillLoc.extraSecond.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraSecond.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);

            if (exSkillLoc.extraThird.skillDef != emptySpell)
                exSkillLoc.extraThird.UnsetSkillOverride(gameObject, exSkillLoc.extraThird.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraThird.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);

            if (exSkillLoc.extraFourth.skillDef != emptySpell)
                exSkillLoc.extraFourth.UnsetSkillOverride(gameObject, exSkillLoc.extraFourth.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            exSkillLoc.extraFourth.SetSkillOverride(gameObject, DrawFromDeck(SelectedDeck), GenericSkill.SkillOverridePriority.Replacement);

            
        }

        //Bookmark grab
        public SkillDef DrawFromDeck(int deckNum)
        {
            //Check if the deck has been fully used.
            if (GetDrawCountByIndex(deckNum).Count == GetDeckByIndex(deckNum).Count)
            {
                GetDrawCountByIndex(deckNum).Clear();
            }

            //Start to pull randomly from the deck
            int randomVarInd;
            do
            {
                randomVarInd = Random.Range(0, GetDeckByIndex(deckNum).Count);
            }
            while (GetDrawCountByIndex(deckNum).Contains(randomVarInd));

            //Mark the spell as used.
            GetDrawCountByIndex(deckNum).Add(randomVarInd);

            return GetDeckByIndex(deckNum)[randomVarInd];
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

        private List<SkillDef> GetDeckByIndex(int deckIndex)
        {
            switch (deckIndex)
            {
                case 0:
                    return deckA;
                case 1:
                    return deckA;
                case 2:
                    return deckA;
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

        //Here at the very bottom we fill out all of the decks with our cards
        private void PopulateDecks()
        {
            deckA = new List<SkillDef>();

            for(int i = 0; i < 2; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("RecoverMana"));
            for(int i = 0; i < 2; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BookmarkFull"));
            for(int i = 0; i < 6; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMetron"));
            for(int i = 0; i < 5; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedHowlingMetron"));
            for(int i = 0; i < 5; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMSProcess"));
        }
    }
}