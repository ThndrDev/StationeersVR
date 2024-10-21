using Assets.Scripts;
using HarmonyLib;
using JetBrains.Annotations;
using StationeersVR.Utilities;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Assets.Scripts.Serialization;

namespace StationeersVR.Patches
{
    internal class CameraControllerPatches
    {
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.SetAntialising))]
        public static class CameraController_SetAntialising_Patch
        {
            [UsedImplicitly]
            [HarmonyPostfix]
            static void Postfix(Antialiasing component)
            {
                {
                    Camera vrCam = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
                    if (vrCam == null)
                    {
                        ModLog.Debug("VR Camera not found while trying to add Antialiasing to it");
                        return;
                    }                    
                    string antialiasing = Settings.CurrentData.Antialiasing;
                    if (antialiasing == "none")
                    {
                        ModLog.Debug("Antialiasing disabled on VR Camera");
                        if (vrCam.gameObject.GetComponent<Antialiasing>() != null)
                        {
                            vrCam.gameObject.GetComponent<Antialiasing>().enabled = false;
                        }
                        return;
                    }                    
                    if (vrCam.gameObject.GetComponent<Antialiasing>() != null)
                    {
                        // update antialiasing options
                        Antialiasing vrAntialiasing = vrCam.gameObject.GetComponent<Antialiasing>();
                        vrAntialiasing.mode = component.mode;
                        vrAntialiasing.enabled = true;
                        return;
                    }
                    else
                    {
                        ModLog.Debug("Adding Antialiasing component to VR Camera");
                        Antialiasing vrAntialiasing = vrCam.gameObject.AddComponent<Antialiasing>();
                        // Populate the antialiasing component shaders otherwise we get a lot of missing shader errors
                        vrAntialiasing.shaderFXAAPreset3 = component.shaderFXAAPreset3;
                        vrAntialiasing.shaderFXAAPreset2 = component.shaderFXAAPreset2;
                        vrAntialiasing.shaderFXAAII = component.shaderFXAAII;
                        vrAntialiasing.shaderFXAAIII = component.shaderFXAAIII;
                        vrAntialiasing.nfaaShader = component.nfaaShader;
                        vrAntialiasing.ssaaShader = component.ssaaShader;
                        vrAntialiasing.dlaaShader = component.dlaaShader;
                        vrAntialiasing.ssaaShader = component.ssaaShader;
                        vrAntialiasing.dlaaSharp = component.dlaaSharp;
                        
                        vrAntialiasing.mode = component.mode; // Copy the original camera antialising mode
                        vrAntialiasing.enabled = true;
                    }
                }
            }
        }
    }
}
