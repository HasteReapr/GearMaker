using AsukaMod.Survivors.Asuka.Spells;
using EntityStates;
using AsukaMod.Survivors.Asuka;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using AsukaMod.Modules.BaseStates;

namespace AsukaMod.Survivors.Asuka.Spells
{
    internal class MetronScreamer808 : BaseMeleeSpell
    {
        public override void OnEnter()
        {
            ManaCost = 16;
            base.OnEnter();
            if (CastFailed) return;

            hitboxGroupName = "ScreamerHitbox";

            damageType = DamageTypeCombo.Generic;
            damageCoefficient = AsukaStaticValues.ScreamerCoef;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 0.34f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0f;
            attackEndPercentTime = 0.22f;

            hitStopDuration = 0.012f;
            attackRecoil = 0f;
            hitHopVelocity = 0f;
        }
    }
}