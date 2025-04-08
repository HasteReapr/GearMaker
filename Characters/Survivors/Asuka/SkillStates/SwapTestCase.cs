using EntityStates;
using Unity;
using RoR2;
using AsukaMod.Survivors.Asuka.Components;
using UnityEngine;
using ExtraSkillSlots;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    public class SwapTestCase : BaseSkillState
    {
        AsukaManaComponent manaComp;
        ExtraInputBankTest extraInput;
        

        float fadeoutTimer;
        float fadeReset = 1/0.12f;

        public override void OnEnter()
        {
            base.OnEnter();
            manaComp = GetComponent<AsukaManaComponent>();
            extraInput = outer.GetComponent<ExtraInputBankTest>();
            manaComp.starInd.gameObject.SetActive(true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (extraInput.extraSkill1.justPressed)
            {
                manaComp.SelectedDeck = 0;
                manaComp.deckBInd.gameObject.SetActive(false);
                manaComp.deckCInd.gameObject.SetActive(false);

                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }

            if (extraInput.extraSkill2.justPressed)
            {
                manaComp.SelectedDeck = 1;
                manaComp.deckBInd.gameObject.SetActive(true);
                manaComp.deckCInd.gameObject.SetActive(false);

                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }

            if (extraInput.extraSkill3.justPressed)
            {
                manaComp.SelectedDeck = 2;
                manaComp.deckBInd.gameObject.SetActive(false);
                manaComp.deckCInd.gameObject.SetActive(true);

                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }

            if (!inputBank.skill3.down)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            manaComp.starInd.gameObject.SetActive(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}