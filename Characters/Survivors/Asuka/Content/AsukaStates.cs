using AsukaMod.Survivors.Asuka.SkillStates;

namespace AsukaMod.Survivors.Asuka
{
    public static class AsukaStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Shoot));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
