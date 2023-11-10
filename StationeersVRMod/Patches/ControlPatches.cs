using HarmonyLib;
using JetBrains.Annotations;
using Valve.VR;
using StationeersVR.Utilities;
using StationeersVR.VRCore.UI;
using UnityEngine;
using Valve.VR.InteractionSystem;
using StationeersVR.VRCore;
using Assets.Scripts.UI;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Serialization;

// These Harmony patches are used to inject the VR inputs into the game's control system
namespace StationeersVR
{
    [HarmonyPatch(typeof(Settings), nameof(Settings.LoadSettings))]
    [UsedImplicitly]
    class Settings_LoadSettings_Patch
    {
        static void Postfix()
        {
            ModLog.Debug("Initializing VRControls");
            VRControls.instance.init();
        }
    }


    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetButtonDown))]
    [UsedImplicitly]
    class KeyManager_GetButtonDown_Patch
    {
        static bool Prefix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonDown(key);
                ModLog.Debug("GetButtonDown Patch: KeyCode: " + key + "__result: " + __result);
                return false;
            }
            ModLog.Debug("GetButtonDown Patch: Running Vanilla method");
            return true;
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetButtonUp))]
    class KeyManager_GetButtonUp_Patch
    {
        static bool Prefix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonUp(key);
                ModLog.Debug("GetButtonUp Patch: KeyCode: " + key + "__result: " + __result);
                return false;
            }
            ModLog.Debug("GetButtonUp Patch: Running Vanilla method");
            return true;
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetButton))]
    class KeyManager_GetButton_Patch
    {
        static bool Prefix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonUp(key);
                ModLog.Debug("GetButton Patch: KeyCode: " + key + "__result: " + __result + " ConsoleWindow.IsOpen: " + ConsoleWindow.IsOpen + " VRControls.instance.GetButtonUp(key): "+ VRControls.instance.GetButtonUp(key));
                return false;
            }
            return true;
        }
    }
    /*
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetKey))]
    class KeyManager_GetKeyDown_Patch
    {
        private static HashSet<KeyCode> pendingKeys = new HashSet<KeyCode>();

        static public void EmulateKeyDown(KeyCode key)
        {
            pendingKeys.Add(key);
        }

        static bool Prefix(KeyCode key, ref bool __result)
        {
            if (pendingKeys.Remove(key))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }*/

    // This patch will make the left VR joystick move the character forward/backwards
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetForwardAxis))]
    class KeyManager_GetForwardAxis_Patch
    {
        static bool Prefix(ref float __result)
        {
            if (VRControls.mainControlsActive)
            {
                var joystick = VRControls.instance.GetJoyLeftStickY();
                //ModLog.Debug("Joystick Y value: " +  joystick);
                if (VRPlayer.attachedToPlayer)
                {
                    // Deadzone values
                    if (joystick > -0.1f && joystick < 0.1f)
                    {
                        __result = 0f;
                        return false;
                    }
                }
                __result = __result + joystick;
                return false;
            }
            return true;
        }
    }

    // This patch will make the left VR joystick move the character sideways
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetRightAxis))]
    class KeyManager_GetRightAxis_Patch
    {
        static bool Prefix(ref float __result)
        {
            if (VRControls.mainControlsActive)
            {
                var joystick = VRControls.instance.GetJoyLeftStickX();
                //ModLog.Debug("Joystick X value: " + joystick);
                if (VRPlayer.attachedToPlayer)
                {
                    // Deadzone values
                    if (joystick > -0.1f && joystick < 0.1f)
                    {
                        __result = 0f;
                        return false;
                    }
                }
                __result = __result + joystick;
                return false;
            }
            return true;
        }
    }
}
