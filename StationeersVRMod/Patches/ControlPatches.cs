using HarmonyLib;
using JetBrains.Annotations;
using StationeersVR.Utilities;
using StationeersVR.VRCore.UI;
using UnityEngine;
using StationeersVR.VRCore;
using Assets.Scripts;
using Assets.Scripts.Serialization;
using Assets.Scripts.Util;
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



    [HarmonyPatch(typeof(CameraController), nameof(CameraController.SetMouseLook))]
    class CameraController_SetMouseLook_Patch
    {
        static bool Prefix(CameraController __instance)
        {
            if (VRControls.mainControlsActive)
            {
                //Setting this to zero so it does not cause any issues
                float num = 0;//Singleton<InputManager>.Instance.GetAxis("LookY"); //VRControls.instance.GetJoyRightStickY();
                if (KeyManager.HasAxis(ControllerMap.VerticalLook))
                {
                    num = Mathf.Clamp(num + ControllerMap.VerticalLook.Output, -1f, 1f);
                }
                float num2 = VRControls.instance.GetJoyRightStickX();//Singleton<InputManager>.Instance.GetAxis("LookX");
                if (KeyManager.HasAxis(ControllerMap.HorizontalLook))
                {
                    num2 = Mathf.Clamp(num2 + ControllerMap.HorizontalLook.Output, -1f, 1f);
                }
                __instance.RotationX += num * CameraController.CameraSensitivity * (float)((!Settings.CurrentData.InvertMouse) ? 1 : (-1));
                __instance.RotationY += num2 * CameraController.CameraSensitivity;
                __instance.RotationX = InputHelpers.ClampAngle(__instance.RotationX, __instance.CameraTiltMinimum, __instance.CameraTiltMaximum);
                //VR Smooth Turn
                //Below turns the VR Camera with the player
                //Only Doing the Y axis since we do not need a joystick to look up and down
                Quaternion cameraRotation = Quaternion.Euler(0f , __instance.RotationY, 0f);
                VRPlayer.vrPlayerInstance._vrCameraRig.transform.rotation = cameraRotation;
            }
            return false;
        }
    }
}
