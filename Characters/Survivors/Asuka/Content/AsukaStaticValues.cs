using System;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaStaticValues
    {
        public const float swordDamageCoefficient = 2.8f;

        public const float bookDamageCoef = 0.75f;

        public const float bombDamageCoefficient = 16f;

        public const float meow = 4f;
        public const float HowlingMetronCoef = meow * 2f;
        public const float DelayedHowlingMetronCoef = meow;
        public const float HowlingMetronMSProcessCoef = meow * 1.2f;
        public const float MetronArpeggioCoef = meow * 0.7f;
        public const float DelayedTardusCoef = meow * 1.5f;
        public const float ScreamerCoef = meow * 3f;
        public const float TerraCoef = meow * 3f;
        public const float RMSCoef = 2f; //2/3/5/8/12 increases like this as it goes up in levels
    }
}