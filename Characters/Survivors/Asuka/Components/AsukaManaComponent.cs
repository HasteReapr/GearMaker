﻿using ExtraSkillSlots;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using RoR2.Projectile;

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

        internal enum HandNum : int
        {
            PUNCH = 0,
            KICK = 1,
            SLASH = 2,
            HEAVY = 3
        }

        //This list contains information about cubes. This is just held here for Staves to communicate to cubes.
        private List<CubeInfo> cubeList;
        private List<BitShiftInfo> bitList;

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
        public List<SkillDef> chaoticDeck;

        private float hurtTimer = 0; //used for the initial 1 second pause
        private float hurtTimer2 = 0; //used for the drain after the 1 second pause
        private float hurtHold = 0;

        //Mana information
        public float minMana = 0;
        public float maxMana = 100;
        public float passiveManaRegen = 0.25f;
        public float manaStunRegen = 5f;
        public float mpsWhileRegen = 30f;
        public float mpsRecoverContinous = 15f; // Recover Mana Continous buff.
        public float manaGainedInstant = 30; // Recover Mana Instant. Doesn't consume cards, and just recovers your mana.
        public float manaGainedOnKill = 2f;
        public float manaLostOnHitMult = 0.75f; //this is a multiplier for the damage, incoming damage gets multiplied by this and this is how much mana you lose.
        public bool inManaStun = false; // This is an indicator for when you're in the mana recovery state, this gets triggered when you take damage and are low on mana, and the damage you take makes you go into <=0 mana.

        //Mana UI Stuff
        public GameObject manaBarUI;
        public string cardOverlayChildLocStr;
        private OverlayController manaOverlayCTRL;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        //private List<TextMesh> uiCountList = new List<TextMesh>();
        private TextMeshProUGUI[] uiCountList;

        private ChildLocator uiChildLoc;
        public Transform deckBInd;
        public Transform deckCInd;
        public Transform starInd;
        public Transform samplerInd;

        //Real Mana Values
        [SyncVar(hook = "OnManaModified")]
        private float _mana;

        #region Values
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
                if (NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;

                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _mana, 1U);
            }
        }
        #endregion Values

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

            if (amount < 0)
            {
                hurtTimer = 1f;
                hurtTimer2 = 1f;
                hurtHold = this.mana + amount;
            }

            this.Network_Mana = Mathf.Clamp(this.mana + amount, 0, maxMana);
        }

        private void OnEnable()
        {
            OverlayCreationParams manaUICreationParams = new OverlayCreationParams
            {
                prefab = AsukaAssets.ManaUI,
                childLocatorEntry = "BottomLeftCluster"
            };
            manaOverlayCTRL = HudOverlayManager.AddOverlay(gameObject, manaUICreationParams);
            manaOverlayCTRL.onInstanceAdded += OnManaOverlayAdded;
            manaOverlayCTRL.onInstanceRemove += OnManaOverlayRemoved;
        }

        private void OnDisable()
        {
            if (manaOverlayCTRL != null)
            {
                manaOverlayCTRL.onInstanceAdded -= OnManaOverlayAdded;
                manaOverlayCTRL.onInstanceRemove -= OnManaOverlayRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(manaOverlayCTRL);
            }
        }
        private void OnManaOverlayAdded(OverlayController controller, GameObject instance)
        {
            fillUiList.Add(instance.GetComponent<ImageFillController>());

            uiChildLoc = instance.GetComponent<ChildLocator>();

            deckBInd = uiChildLoc.FindChild("IndicatorB");
            deckCInd = uiChildLoc.FindChild("IndicatorC");
            starInd = uiChildLoc.FindChild("DeckStar");
            samplerInd = uiChildLoc.FindChild("SamplerSymbol");

            uiCountList = instance.GetComponentsInChildren<TextMeshProUGUI>();
        }

        private void OnManaOverlayRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.GetComponent<ImageFillController>());
        }

        public void Awake()
        {
            cubeList = new List<CubeInfo>();
        }

        public void Start()
        {
            charBody = gameObject.GetComponent<CharacterBody>();
            exSkillLoc = gameObject.GetComponent<ExtraSkillLocator>();
            stateMachines = gameObject.GetComponents<EntityStateMachine>();

            //Populate and setup the decks in the arrays first, for easier tracking.
            PopulateDecks();

            //Check for a character body
            if (charBody != null)
            {
                //We give Asuka the defense buff when we spawn in. This buff gets removed when you run out of mana.
                charBody.AddBuff(AsukaBuffs.manaDefBuff);

                AddMana(100);

                DrawFullHand();
            }
        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {
            if (charBody.HasBuff(AsukaBuffs.manaRegenCont)) // If we have the Recover Mana Continous buff we add that amount of mana
            {
                AddMana(mpsRecoverContinous * Time.fixedDeltaTime);
            }

            if (inManaStun) // If we arent in lost mana penalty we passively recover a little bit of mana, otherwise we quickly recharge the bar
            {
                AddMana(manaStunRegen * Time.fixedDeltaTime);
                //Once we get back to full we wanna get out of mana stun and we get the defense buff back
                if(mana >= maxMana)
                {
                    inManaStun = false;
                    charBody.AddBuff(AsukaBuffs.manaDefBuff);
                }
            }
            else
            {
                AddMana(passiveManaRegen * Time.fixedDeltaTime);
            }

            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(mana / maxMana);
            }

            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                hurtTimer2 -= Time.deltaTime;
            }

            samplerInd.gameObject.SetActive(charBody.HasBuff(AsukaBuffs.recycleBuff));            

            foreach(TextMeshProUGUI textUI in uiCountList)
            {
                StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                stringBuilder.AppendInt(30 - GetDrawCountByIndex(SelectedDeck).Count, 1U, 3U);
                
                textUI.SetText(stringBuilder);

                HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            }

            ClearNullCubes();
            CleanupBitShift();
            UpdateBitShift();
        }


        //Full draw, used for Bookmark Random Import, and initializing.
        public void DrawFullHand()
        {
            DrawIntoHand(0);
            DrawIntoHand(1);
            DrawIntoHand(2);
            DrawIntoHand(3);
        }

        //Bookmark grab
        public SkillDef DrawSkillDef(int deckNum)
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

        public SkillDef DrawSkillDef(List<SkillDef> deck)
        {
            //Check if the deck has been fully used.
            if (deck.Count == deck.Count)
            {
                deck.Clear();
            }

            //Start to pull randomly from the deck
            int randomVarInd = Random.Range(0, deck.Count);

            return deck[randomVarInd];
        }

        public void DrawIntoHand(int handIndex)
        {
            GenericSkill hand = GetHandByIndex(handIndex);

            if (hand.skillDef != emptySpell)
            {
                //If our hand isn't empty then return, since we can't draw into a occupied hand
                return;
            }
            //If our hand is actually empty we draw a card
            SkillDef drawnCard = DrawSkillDef(SelectedDeck);
            //then replace our skill
            hand.SetSkillOverride(gameObject, drawnCard, GenericSkill.SkillOverridePriority.Contextual);
        }

        public void DrawIntoHand(GenericSkill hand)
        {
            if (hand.skillDef != emptySpell)
            {
                //If our hand isn't empty then return, since we can't draw into a occupied hand
                return;
            }
            //If our hand is actually empty we draw a card
            SkillDef drawnCard = DrawSkillDef(SelectedDeck);
            //then replace our skill
            hand.SetSkillOverride(gameObject, drawnCard, GenericSkill.SkillOverridePriority.Contextual);
        }

        public bool DrawIntoHand(GenericSkill hand, int unused = 0)
        {
            if (hand.skillDef != emptySpell)
            {
                //If our hand isn't empty then return, since we can't draw into a occupied hand
                return false;
            }
            //If our hand is actually empty we draw a card
            SkillDef drawnCard = DrawSkillDef(SelectedDeck);
            //then replace our skill
            hand.SetSkillOverride(gameObject, drawnCard, GenericSkill.SkillOverridePriority.Contextual);
            return true;
        }

        public void ChaoticDraw(GenericSkill hand)
        {
            if (hand.skillDef != emptySpell)
            {
                //If our hand isn't empty then return, since we can't draw into a occupied hand
                return;
            }
            //If our hand is actually empty we draw a card
            SkillDef drawnCard = DrawSkillDef(chaoticDeck);
            //then replace our skill
            hand.SetSkillOverride(gameObject, drawnCard, GenericSkill.SkillOverridePriority.Contextual);
        }

        //Bookmark remove
        public void DiscardFromHand(int handIndex)
        {
            GenericSkill hand = GetHandByIndex(handIndex);
            if (hand != null)
            {
                //Discard the hand.
                hand.UnsetSkillOverride(gameObject, hand.skillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        //This one is called from the Skills
        public void DiscardFromHand(GenericSkill hand)
        {
            if (hand != null)
            {
                //Discard the hand.
                hand.UnsetSkillOverride(gameObject, hand.skillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public void TryDrawDiscard(GenericSkill hand)
        {
            //We try to draw into our hand, but if the hand is not empty it will return.
            if (DrawIntoHand(hand, 0)) {
                //We spawn the draw effect here
                return;
            }

            //If this fails we discard, and spawn the discard effect.
            DiscardFromHand(hand);
        }

        private GenericSkill GetHandByIndex(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    return exSkillLoc.extraFirst;
                case 1:
                    return exSkillLoc.extraSecond;
                case 2:
                    return exSkillLoc.extraThird;
                case 3:
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

        private void ClearNullCubes()
        {
            cubeList.RemoveAll(item => item.gameObj == null);
        }

        private void CleanupBitShift()
        {
            bitList.RemoveAll(item => item.nullable == true);
        }

        private void UpdateBitShift()
        {
            foreach(BitShiftInfo info in bitList)
            {
                info.Update();
            }
        }

        public void AddCube(GameObject cube)
        {
            CubeInfo info = new CubeInfo(cube, cube.GetComponent<CubeBehaviourComponent>());

            cubeList.Add(info);
        }

        public void AddBitShift(int count, FireProjectileInfo info)
        {
            BitShiftInfo bit = new BitShiftInfo(count, info);

            bitList.Add(bit);
        }

        //Here at the very bottom we fill out all of the decks with our cards
        private void PopulateDecks()
        {
            deckA = new List<SkillDef>();

            for (int i = 0; i < 2; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("ReduceManaCost"));
            for (int i = 0; i < 2; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BookmarkFull"));
            for (int i = 0; i < 6; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMetron"));
            for (int i = 0; i < 5; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedHowlingMetron"));
            for (int i = 0; i < 5; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMSProcess"));
            for (int i = 0; i < 5; i++)
                deckA.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("MetronScreamer"));


            deckB = new List<SkillDef>();

            for (int i = 0; i < 2; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("RecoverMana"));
            for (int i = 0; i < 2; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BookmarkRandom"));
            for (int i = 0; i < 3; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMetron"));
            for (int i = 0; i < 3; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedHowlingMetron"));
            for (int i = 0; i < 3; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMSProcess"));
            for (int i = 0; i < 2; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedTardusMetron"));
            for (int i = 0; i < 2; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BitShiftMetron"));
            for (int i = 0; i < 3; i++)
                deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("MetronScreamer"));

            deckB.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("ChaoticOption"));

            deckC = new List<SkillDef>();

            for (int i = 0; i < 2; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("RegenMana"));
            for (int i = 0; i < 2; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BookmarkAuto"));
            for (int i = 0; i < 2; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMetron"));
            for (int i = 0; i < 2; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedHowlingMetron"));
            for (int i = 0; i < 2; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("HowlingMSProcess"));
            for (int i = 0; i < 3; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("MetronArpeggio"));
            for (int i = 0; i < 3; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("DelayedTardusMetron"));
            for (int i = 0; i < 4; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BitShiftMetron"));
            for (int i = 0; i < 4; i++)
                deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("GoToMarker"));

            deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("Sampler404"));
            deckC.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("ChaoticOption"));

            chaoticDeck = new List<SkillDef>();
            chaoticDeck.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("RegenMana"));
            chaoticDeck.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("BookmarkAuto"));
            chaoticDeck.Add(AsukaSurvivor.SpellSkills.GetValueOrDefault("MetronArpeggio"));
            //Terra and Accipiter too I just haven't made those spells yet
            //Terra, 808 and Aquila will be big melee hitboxes, while Accipiter is a special case
        }
    }

    internal class CubeInfo
    {
        internal GameObject gameObj;
        internal CubeBehaviourComponent cubeBehaviour;

        internal CubeInfo(GameObject gameObj, CubeBehaviourComponent cubeBehaviour)
        {
            this.gameObj = gameObj;
            this.cubeBehaviour = cubeBehaviour;
        }
    }

    public class BitShiftInfo
    {
        public float timer;
        public FireProjectileInfo fireProjectile;
        public int count;
        private float interval = 0.1f;
        public bool nullable = false;

        public BitShiftInfo(int count, FireProjectileInfo fireProjectile)
        {
            this.count = count;
            Chat.AddMessage($"Bitshift created with a count of {count}");
            this.fireProjectile = fireProjectile;
        }

        public void Update()
        {
            timer += Time.deltaTime;
            Chat.AddMessage($"Current Timer {timer}");
            if (timer % interval == 0)
            {
                Chat.AddMessage("We are attempting to fire a bitshift");
                ProjectileManager.instance.FireProjectile(fireProjectile);
                count--;
            }
        }
    }
}