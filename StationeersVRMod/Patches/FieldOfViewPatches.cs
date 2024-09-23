using Assets.Scripts;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersVR.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace StationeersVR.Patches
{
    internal class FieldOfViewPatches
    {
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.SetFieldOfView))]
        public static class CameraController_SetFieldOfView_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(float fov) { 
                if ((object)CameraController.Instance == null)
                {
                    return false;
                }
                //We don't need to set the field of view while in vr, just leads to log spam
                /*foreach (Camera controlledCamera in CameraController._controlledCameras)
                {
                    controlledCamera.fieldOfView = fov;
                }*/
                return false;
            }
        }

        [HarmonyPatch(typeof(MenuCutscene), nameof(MenuCutscene.SetPosition))]
        public static class MenuCutscene_SetPosition_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(float fov, MenuCutscene __instance)
            {
                if (!GameManager.IsBatchMode)
                {
                    RenderSettings.skybox = __instance.SkyboxMaterial;
                    RenderSettings.sun = __instance.Sun;
                    RenderSettings.ambientMode = AmbientMode.Trilight;
                    Camera currentCamera = CameraController.CurrentCamera;
                    //This one still give us fov spam with MainCamera
                    //currentCamera.fieldOfView = MenuCutscene.FieldOfView;
                    currentCamera.transform.SetPositionAndRotation(MenuCutscene.Position, __instance.Rotation);
                    SkyBoxController.SetPosition(MenuCutscene.Position);
                    SkyBoxController.ApplyDefaults();
                }
                return false;
            }
        }
    }
}
