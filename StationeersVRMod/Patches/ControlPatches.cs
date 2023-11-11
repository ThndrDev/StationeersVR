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
        static void Postfix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonDown(key);
                //ModLog.Debug("GetButtonDown Patch: KeyCode: " + key + "__result: " + __result + "VRControls Response: " + VRControls.instance.GetButtonDown(key));
            }
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetButtonUp))]
    class KeyManager_GetButtonUp_Patch
    {
        static void Postfix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonUp(key);
                //ModLog.Debug("GetButtonUp Patch: KeyCode: " + key + "__result: " + __result);
            }
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetButton))]
    class KeyManager_GetButton_Patch
    {
        static void Postfix(KeyCode key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = (key == KeyMap.ToggleConsole || !ConsoleWindow.IsOpen) && VRControls.instance.GetButtonUp(key);
                //ModLog.Debug("GetButton Patch: KeyCode: " + key + "__result: " + __result + " ConsoleWindow.IsOpen: " + ConsoleWindow.IsOpen + " VRControls.instance.GetButtonUp(key): "+ VRControls.instance.GetButtonUp(key));
            }
        }
    }
    /*
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetMouseDown))]
    class KeyManager_GetMouseDown_Patch
    {
        static void Postfix(string key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = VRControls.instance.GetMouseDown(key);
                //ModLog.Debug("GetMouseDown Patch: KeyCode: " + key + " VRControls.instance.GetMouseDown Returned: " +  __result);
            }
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetMouseUp))]
    class KeyManager_GetMouseUp_Patch
    {
        static void Postfix(string key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = VRControls.instance.GetMouseUp(key);
                //ModLog.Debug("GetMouseUp Patch: KeyCode: " + key + " VRControls.instance.GetMouseUp Returned: " + __result);
            }
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.GetMouse))]
    class KeyManager_GetMouse_Patch
    {
        static void Postfix(string key, ref bool __result)
        {
            if (VRControls.mainControlsActive)
            {
                __result = VRControls.instance.GetMouse(key);
               // ModLog.Debug("GetMouse Patch: KeyCode: " + key + " VRControls.instance.GetMouse Returned: " + __result);
            }
        }
    }
    */

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
