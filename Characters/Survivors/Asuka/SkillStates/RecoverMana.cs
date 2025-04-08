using EntityStates;
using Unity;
using RoR2;
using AsukaMod.Survivors.Asuka.Components;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    public class RecoverMana : BaseSkillState
    {
        AsukaManaComponent manaComp;
        float duration = 0.25f;
        float increaseOverTime = 0.1f;
        ChildLocator child;

        public override void OnEnter()
        {
            base.OnEnter();
            manaComp = GetComponent<AsukaManaComponent>();

            //PlayAnimation("FullBody, Override", "RECOVER_INTRO", "RECOVER_INTRO.playbackRate", 1.1f);

            Animator animator = GetModelAnimator();
            animator.SetBool("manaRegen", true);
            GetModelAnimator().SetFloat("RECOVER_INTRO.playbackRate", attackSpeedStat);

            PlayCrossfade("FullBody, Override", "RECOVER_INTRO", "RECOVER_INTRO.playbackRate", duration, 0.1f);

            child = GetModelChildLocator();
            child.FindChild("RegenVFX").gameObject.SetActive(true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //We just recover mana as long as we hold this ability
            float slowAdd = increaseOverTime * fixedAge;
            manaComp.AddMana((manaComp.mpsWhileRegen + slowAdd) * Time.fixedDeltaTime);

            if (fixedAge >= duration && !inputBank.skill4.down)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("manaRegen", false);

            child.FindChild("RegenVFX").gameObject.SetActive(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}