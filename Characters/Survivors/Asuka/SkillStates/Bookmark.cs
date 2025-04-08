using EntityStates;
using Unity;
using RoR2;
using AsukaMod.Survivors.Asuka.Components;
using UnityEngine;
using ExtraSkillSlots;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    public class Bookmark : BaseSkillState
    {
        AsukaManaComponent manaComp;
        ExtraInputBankTest extraBank;
        ExtraSkillLocator extraSkills;

        public override void OnEnter()
        {
            base.OnEnter();
            manaComp = GetComponent<AsukaManaComponent>();
            extraBank = outer.GetComponent<ExtraInputBankTest>();
            extraSkills = GetComponent<ExtraSkillLocator>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (extraBank.extraSkill1.justPressed)
            {
                manaComp.TryDrawDiscard(extraSkills.extraFirst);
                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }
            if (extraBank.extraSkill2.justPressed)
            {
                manaComp.TryDrawDiscard(extraSkills.extraSecond);
                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }
            if (extraBank.extraSkill3.justPressed)
            {
                manaComp.TryDrawDiscard(extraSkills.extraThird);
                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }
            if (extraBank.extraSkill4.justPressed)
            {
                manaComp.TryDrawDiscard(extraSkills.extraFourth);
                PlayCrossfade("Gesture, Override", "DRAW_CARD", "DRAW_CARD.playbackRate", 1, 0.1f);
            }

            if (!inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}