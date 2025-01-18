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

        public override void OnEnter()
        {
            base.OnEnter();
            manaComp = GetComponent<AsukaManaComponent>();
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}