﻿using BepInEx.Configuration;
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
        private static ConfigEntry<bool> configUseSnapTurn;
        private static ConfigEntry<bool> autoOpenKeyboardOnInteract;

        // Variables to access the configuration
        public static int LogLevel;
        public static bool NonVrPlayer;
        public static float nearClipPlane;
        public static bool LeftHanded;
        public static bool UseLookLocomotion;
        public static bool UseVrControls;
        public static bool UseSnapTurn;
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
                            0.17f,
                            new ConfigDescription("This can be used to adjust the distance where anything inside will be clipped out and not rendered. You can try adjusting this if you experience" +
                                                  " problems where you see the mouth of the player character for example.",
                            new AcceptableValueRange<float>(0.05f, 0.5f)));
            nearClipPlane = configNearClipPlane.Value;

            autoOpenKeyboardOnInteract = StVR.Config.Bind("UI",
                            "AutoOpenKeyboardOnInteract",
                            true,
                            "Automatically open a keyboard when you interact with things that have text input (eg. Searchboxes, Programming Window), Only works with Vr Controls");


            configUseLookLocomotion = StVR.Config.Bind("2 - Controls",
                                    "UseLookLocomotion",
                                    true,
                                    "Setting this to true ties the direction you are looking to the walk direction while in first person mode. " +
                                    "Set this to false if you prefer to disconnect these so you can look by turning your head without affecting " +
                                    "movement direction.");
            UseLookLocomotion = configUseLookLocomotion.Value;

            configUseVrControls = StVR.Config.Bind("2 - Controls",
                "UseVRControls",
                false,
                "This setting enables the use of the VR motion controllers as input (Only Oculus Touch and Valve Index supported)." +
                "Set this to false to use the keyboard and mouse as input." +
                "VR native Controls are still a huge WIP and not completely playable, so for now only use it for testing purposes.");
            UseVrControls = configUseVrControls.Value;



            configDominantHand = StVR.Config.Bind("2 - Controls",
                            "DominantHand",
                            "Right",
                            new ConfigDescription("The dominant hand of the player",
                            new AcceptableValueList<string>(new string[] { "Right", "Left" })));


            configUseSnapTurn = StVR.Config.Bind("2 - Controls",
                "UseSnapTurn",
                false,
                "This setting enables the use of the SnapTurn in VR to turn the character sideways." +
                "Set this to false to enable Continuous Turn mode.");
            UseSnapTurn = configUseSnapTurn.Value;

        }
        public static string GetDominantHand()
        {
            return configDominantHand.Value == "Left" ? VRPlayer.LEFT_HAND : VRPlayer.RIGHT_HAND;
        }

        public static bool AutoOpenKeyboardOnInteract()
        {
            return autoOpenKeyboardOnInteract.Value;
        }
    }
}
