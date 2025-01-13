using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace AsukaMod.Survivors.Asuka.SkillStates
{
    internal class BookTrackSkillDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new BookTrackSkillDef.InstanceData
            {
                asukaTracker = skillSlot.GetComponent<AsukaTracker>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            AsukaTracker asukaTracker = ((BookTrackSkillDef.InstanceData)skillSlot.skillInstanceData).asukaTracker;
            return (asukaTracker != null) ? asukaTracker.GetTrackingTarget() : null;
        }

        private class InstanceData : BaseSkillInstanceData
        {
            public AsukaTracker asukaTracker { get; set; }
        }
    }
}
