using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using AsukaMod.Survivors.Asuka.Components;
using ExtraSkillSlots;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class EmptySpell : BaseSpellState
    {
        //This is literally just a copy of the BaseSpell but it has a higher minimum interrupt priority, so if you use an empty slot you cant interrupt it by using a fast spell

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        //We also reset OnExit to be empty so this doesn't unset the spell if you accidentally use this then draw a spell
        public override void OnExit()
        {
            
        }
    }
}
