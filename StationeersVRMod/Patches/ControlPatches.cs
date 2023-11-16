using HarmonyLib;
using JetBrains.Annotations;
using StationeersVR.Utilities;
using StationeersVR.VRCore.UI;
using UnityEngine;
using StationeersVR.VRCore;
using Assets.Scripts;
using Assets.Scripts.Serialization;
using Assets.Scripts.Util;

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
