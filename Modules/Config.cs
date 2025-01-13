using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using UnityEngine;

namespace AsukaMod.Modules
{
    public static class Config
    {
        public static ConfigFile MyConfig = AsukaPlugin.instance.Config;

        public static ConfigEntry<KeyboardShortcut> punchSpellTrigger;
        public static ConfigEntry<KeyboardShortcut> kickSpellTrigger;
        public static ConfigEntry<KeyboardShortcut> slashSpellTrigger;
        public static ConfigEntry<KeyboardShortcut> heavySpellTrigger;

        /// <summary>
        /// automatically makes config entries for disabling survivors
        /// </summary>
        /// <param name="section"></param>
        /// <param name="characterName"></param>
        /// <param name="description"></param>
        /// <param name="enabledByDefault"></param>
        public static ConfigEntry<bool> CharacterEnableConfig(string section, string characterName, string description = "", bool enabledByDefault = true)
        {

            if (string.IsNullOrEmpty(description))
            {
                description = "Set to false to disable this character and as much of its code and content as possible";
            }
            return BindAndOptions<bool>(section,
                                        "Enable " + characterName,
                                        enabledByDefault,
                                        description,
                                        true);
        }

        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, string description = "", bool restartRequired = false) =>
            BindAndOptions<T>(section, name, defaultValue, 0, 20, description, restartRequired);
        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, float min, float max, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
            {
                description = name;
            }

            if (restartRequired)
            {
                description += " (restart required)";
            }
            ConfigEntry<T> configEntry = MyConfig.Bind(section, name, defaultValue, description);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                //TryRegisterOption(configEntry, min, max, restartRequired);
            }

            punchSpellTrigger = AsukaPlugin.instance.Config.Bind<KeyboardShortcut>
            (
                new ConfigDefinition("Spell Activation Controls", "Spell Slot Punch Input"),
                new KeyboardShortcut(UnityEngine.KeyCode.Alpha1),
                new ConfigDescription("Key to trigger the first spell slot.", null, System.Array.Empty<object>())
            );
            kickSpellTrigger = AsukaPlugin.instance.Config.Bind<KeyboardShortcut>
            (
                new ConfigDefinition("Spell Activation Controls", "Spell Slot Kick Input"),
                new KeyboardShortcut(UnityEngine.KeyCode.Alpha2),
                new ConfigDescription("Key to trigger the second spell slot.", null, System.Array.Empty<object>())
            );
            slashSpellTrigger = AsukaPlugin.instance.Config.Bind<KeyboardShortcut>
            (
                new ConfigDefinition("Spell Activation Controls", "Spell Slot Slash Input"),
                new KeyboardShortcut(UnityEngine.KeyCode.Alpha3),
                new ConfigDescription("Key to trigger the third spell slot.", null, System.Array.Empty<object>())
            );
            heavySpellTrigger = AsukaPlugin.instance.Config.Bind<KeyboardShortcut>
            (
                new ConfigDefinition("Spell Activation Controls", "Spell Slot Heavy Slash Input"),
                new KeyboardShortcut(UnityEngine.KeyCode.Alpha4),
                new ConfigDescription("Key to trigger the fourth spell slot.", null, System.Array.Empty<object>())
            );

            return configEntry;
        }

        //back compat
        public static ConfigEntry<float> BindAndOptionsSlider(string section, string name, float defaultValue, string description, float min = 0, float max = 20, bool restartRequired = false) =>
            BindAndOptions<float>(section, name, defaultValue, min, max, description, restartRequired);

        //add risk of options dll to your project libs and uncomment this for a soft dependency
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOption<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            ModSettingsManager.AddOption(new KeyBindOption(punchSpellTrigger));
            ModSettingsManager.AddOption(new KeyBindOption(kickSpellTrigger));
            ModSettingsManager.AddOption(new KeyBindOption(slashSpellTrigger));
            ModSettingsManager.AddOption(new KeyBindOption(heavySpellTrigger));
        }

        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        public static bool GetKeyPressed(KeyboardShortcut entry)
        {
            foreach (var item in entry.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.MainKey);
        }
    }
}
