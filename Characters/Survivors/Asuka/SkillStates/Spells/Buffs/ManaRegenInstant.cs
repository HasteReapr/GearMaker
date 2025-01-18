using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using AsukaMod.Survivors.Asuka.Components;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class ManaRegenInstant : BaseSpellState
    {
        public float baseDuration = 0.39f;
        public float duration;
        private Animator animator;

        public override void OnEnter()
        {
            ManaCost = 4;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            if (isAuthority)
            {
                characterBody.GetComponent<AsukaManaComponent>().AddMana(25);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }
    }
}
