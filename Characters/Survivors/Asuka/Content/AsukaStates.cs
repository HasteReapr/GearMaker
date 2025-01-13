using AsukaMod.Survivors.Asuka.SkillStates;
using AsukaMod.Survivors.Asuka.Spells;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BookFire));

            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));

            Modules.Content.AddEntityState(typeof(BaseSpellState));
            Modules.Content.AddEntityState(typeof(HowlingMetron));
            Modules.Content.AddEntityState(typeof(DelayedHowlingMetron));
            Modules.Content.AddEntityState(typeof(HowlingMSProcess));
            Modules.Content.AddEntityState(typeof(BitShiftMetron));
            Modules.Content.AddEntityState(typeof(MetronArpeggio));
            Modules.Content.AddEntityState(typeof(DelayedTardusMetron));
            Modules.Content.AddEntityState(typeof(GoToMarker));
            Modules.Content.AddEntityState(typeof(ManaRegenCont));
            Modules.Content.AddEntityState(typeof(ManaRegenInstant));
            Modules.Content.AddEntityState(typeof(ReduceManaCost));
        }
    }
}
