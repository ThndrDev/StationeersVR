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
using UnityEngine.Events;
using System;

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
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && (VRControls.instance.GetButtonDown(key) || Input.GetKeyDown(key));
                return false;
            }
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
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && (VRControls.instance.GetButtonUp(key) || Input.GetKeyUp(key));
                return false;
            }
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
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && (VRControls.instance.GetButton(key) || Input.GetKey(key));
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode)})]
    class Input_GetKey_KeyCode_Patch
    {
        static bool Prefix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = Input.GetKeyInt(key) || (VRControls.instance.GetButton(key));
                return false;
            }
            return true;
        }
    }

    // This patch will enable Continuous Turn if Snapturn is disabled
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetAscend))]
    class KeyManager_GetAscend_Patch
    {
        static bool Prefix(ref float __result)
        {
            if (ConsoleWindow.IsOpen)
            {
                __result = 0f;
                return false;
            }
            if (VRControls.mainControlsActive)
            {
                var joystick = VRControls.instance.GetJoyRightStickY();
                if (VRPlayer.attachedToPlayer)
                {
                    // Deadzone values
                    if (joystick > -0.1f && joystick < 0.1f)
                    {
                        __result = 0f;
                        return false;
                    }
                }
                if (joystick > 0.1f)
                    __result = Mathf.Clamp(joystick, 0f, 1f); 
                return false;
            }
            return true; // VRControls not enable, so just run the vanilla method
        }
    }

    // This patch will enable Continuous Turn if Snapturn is disabled
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetDescend))]
    class KeyManager_GetDescend_Patch
    {
        static bool Prefix(ref float __result)
        {
            if (ConsoleWindow.IsOpen)
            {
                __result = 0f;
                return false;
            }
            if (VRControls.mainControlsActive)
            {
                var joystick = VRControls.instance.GetJoyRightStickY();
                ModLog.Debug("Joystick Y value: " + joystick);
                if (VRPlayer.attachedToPlayer)
                {
                    // Deadzone values
                    if (joystick > -0.1f && joystick < 0.1f)
                    {
                        __result = 0f;
                        return false;
                    }
                }
                if (joystick < 0.1f)
                    __result = Mathf.Clamp(joystick, -1f, 0f);
                return false;
            }
            return true; // VRControls not enable, so just run the vanilla method
        }
    }


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
