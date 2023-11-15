using BepInEx.Configuration;
using StationeersVR.VRCore;
using UnityEngine;
using static Valve.VR.SteamVR_ExternalCamera;

namespace StationeersVR.Utilities
{
    internal class ConfigFile
    {
        // Bepinex Configuration parameter
        private static ConfigEntry<int> configLogLevel;
        private static ConfigEntry<bool> configNonVrPlayer;
        private static ConfigEntry<float> configNearClipPlane;
        private static ConfigEntry<bool> configLeftHanded;
        private static ConfigEntry<bool> configUseLookLocomotion;
        private static ConfigEntry<bool> configUseVrControls;
        private static ConfigEntry<string> configDominantHand;

        // Variables to access the configuration
        public static int LogLevel;
        public static bool NonVrPlayer;
        public static float nearClipPlane;
        public static bool LeftHanded;
        public static bool UseLookLocomotion;
        public static bool UseVrControls;
        public static void HandleConfig(StationeersVR StVR) // Create and manage the configuration file parameters
        {
            //Log Section
            configLogLevel = StVR.Config.Bind("0 - General configuration",
                 "LogLevel",
                 2,
                 new ConfigDescription("Set the log level of the mod. Values can be 0 for errors only (default), 1 for informational logs or 2 for debug logs. " +
                 "Mod logs can be found inside the player.log file in the path %appdata%\\..\\localLow\\rocketwerkz\\rocketstation\\ " +
                 "Warning, if you set this to a value different than 0, the log files can become very large after a extended amount of time playing.",
                 new AcceptableValueRange<int>(0, 2)));

            LogLevel = Mathf.Clamp(configLogLevel.Value, 0, 2);


            configNonVrPlayer = StVR.Config.Bind("0 - General configuration",
             "NonVrPlayer",
             false,
             "Non VR Player description");
            NonVrPlayer = configNonVrPlayer.Value;

            configLeftHanded = StVR.Config.Bind("0 - General configuration",
             "LeftHanded",
             false,
             "If you are left handed, change this to true. Otherwise leave it false for RightHanded");
            LeftHanded = configLeftHanded.Value;

            configNearClipPlane = StVR.Config.Bind("1 - Graphics configuration",
                            "NearClipPlane",
                            .09f,
                            new ConfigDescription("This can be used to adjust the distance where anything inside will be clipped out and not rendered. You can try adjusting this if you experience" +
                                                  " problems where you see the nose of the player character for example.",
                            new AcceptableValueRange<float>(0.05f, 0.5f)));
            nearClipPlane = configNearClipPlane.Value;


            configUseLookLocomotion = StVR.Config.Bind("2 - Controls",
                                "UseLookLocomotion",
                                true,
                                "Setting this to true ties the direction you are looking to the walk direction while in first person mode. " +
                                "Set this to false if you prefer to disconnect these so you can look" +
                                " look by turning your head without affecting movement direction.");
            UseLookLocomotion = configUseLookLocomotion.Value;

            configUseVrControls = StVR.Config.Bind("2 - Controls",
                "UseVRControls",
                true,
                "This setting enables the use of the VR motion controllers as input (Only Oculus Touch and Valve Index supported)." +
                "This setting, if true, will also force UseOverlayGui to be false as this setting Overlay GUI is not compatible with VR laser pointer inputs.");
            UseVrControls = configUseVrControls.Value;

            configDominantHand = StVR.Config.Bind("Controls",
                            "DominantHand",
                            "Right",
                            new ConfigDescription("The dominant hand of the player",
                            new AcceptableValueList<string>(new string[] { "Right", "Left" })));


        }
        public static string GetDominantHand()
        {
            return configDominantHand.Value == "Left" ? VRPlayer.LEFT_HAND : VRPlayer.RIGHT_HAND;
        }
    }
}
