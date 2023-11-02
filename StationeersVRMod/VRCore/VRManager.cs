using UnityEngine.XR;
using Unity.XR.OpenVR;
using Valve.VR;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR.Management;
using UnityEngine;
using StationeersVR.Utilities;

namespace StationeersVR.VRCore
{
    /**
     * VRManager is responsible for initializing/starting the OpenVR XRSDK and the SteamVR
     * interactions system for HMD/controller input.
     */
    class VRManager
    {
        //public static UnityEngine.XR.Management.XRManagerSettings managerSettings = null;

        public static bool InitializeVR()
        {
            if (!VRAssetManager.Initialize())
            {
                ModLog.Error("Problem initializing VR Assets");
                return false;
            }

            // Need to PreInitialize actions before XRSDK
            // to ensure SteamVR_Input is enabled.
            ModLog.Debug("PreInitializing SteamVR Actions...");
            SteamVR_Actions.PreInitialize();
            ModLog.Info("Initializing VR...");
            if (!InitXRSDK())
            {
                ModLog.Error("Failed to initialize VR!.");
                return false;
            }
            if (!InitializeSteamVR())
            {
                ModLog.Error("Problem initializing SteamVR");
                return false;
            }
            return true;
        }

        public static bool StartVR()
        {
            ModLog.Info("Starting VR...");
            return StartXRSDK();
        }

        public static bool InitializeSteamVR()
        {
            ModLog.Info("Initializing SteamVR...");
            SteamVR.Initialize();
            if (!(SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess))
            {
                ModLog.Error("Problem Initializing SteamVR");
                return false;
            }
            if (!SteamVR_Input.initialized)
            {
                ModLog.Error("Problem Initializing SteamVR_Input");
                return false;
            }

            ApplicationManifestHelper.UpdateManifest(Path.Combine(Application.streamingAssetsPath, "stationeers.vrmanifest"),
                                                    "steam.app.544550",
                                                    "https://steamcdn-a.akamaihd.net/steam/apps/544550/header.jpg",
                                                    "Stationeers VR",
                                                    "VR mod for Stationeers",
                                                    steamBuild: true,
                                                    steamAppId: 544550);
            return true;
        }

        private static bool StartXRSDK()
        {
            ModLog.Info("Starting XRSDK!");
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null
                && XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            } else
            {
                ModLog.Error("Error Starting XRSDK!");
                if (XRGeneralSettings.Instance == null)
                {
                    ModLog.Error("XRGeneralSettings Instance is null!");
                    return false;
                } else if (XRGeneralSettings.Instance.Manager == null)
                {
                    ModLog.Error("XRManager instance is null!");
                    return false;
                } else if (XRGeneralSettings.Instance.Manager.activeLoader == null)
                {
                    ModLog.Error("Active loader is null!");
                    return false;
                }
            }
            return true;
        }

        private static bool InitXRSDK()
        {
            XRGeneralSettings xrGeneralSettings = LoadXRSettingsFromAssetBundle();
            if (xrGeneralSettings == null)
            {
                ModLog.Info("XRGeneralSettings Instance is null!");
                return false;
            }
            ModLog.Debug("Loaded XRGeneralSettings!");
            return InitializeXRSDKLoaders(xrGeneralSettings.Manager);
        }

        private static XRGeneralSettings LoadXRSettingsFromAssetBundle()
        {
            /* In the Unity project we need to assign the XR General Settings, the OpenVR Settings and the loaders to an AssetBundle and then
             * generate an AssetBundle file using Window > AssetBundle Browser (tool added by this Unity package: 
             * https://docs.unity3d.com/Manual/AssetBundles-Browser.html). Leave the assetBundle name as xrmanager
             * Check if the assets show up here, leave default options and hit "build". 2 xrmanager files will be saved inside the folder choosed inside the unity project.
             * Copy them to the StreamingAssets folder of the game being modded.
             * The code below will unpack the Streamingasset file and load the XR configuration. If, after trying to load VR, you get the error
             * "XRManager.activeLoader is null! Cannot initialize VR!", make sure to only choose the OpenVR setting in the project settings. If you choose any other
             * option and then disable, Unity will leave some garbage and it will cause this error. If that happens, create a new unity project and try again.
             * You also need to change the Stereo settings to Multipass or VR will not work.*/
            string xrManagerAssetPath = Path.Combine(Application.streamingAssetsPath, "xrmanager");
            ModLog.Debug("Loading XR Settings from AssetBundle: " + xrManagerAssetPath);
            var assetBundle = AssetBundle.LoadFromFile(xrManagerAssetPath);
            foreach (var a in assetBundle.LoadAllAssets())
            {
                ModLog.Debug("XRManagement Asset Loaded: " + a.name);
            }
            XRGeneralSettings instance = XRGeneralSettings.Instance;
            if (instance == null)
            {
                ModLog.Error("XRGeneralSettings Instance is null!");
                return null;
            }
            ModLog.Debug("XRGeneralSettings Instance is non-null.");
            return instance;
        }

        private static bool InitializeXRSDKLoaders(XRManagerSettings managerSettings)
        {
            ModLog.Debug("Initializing XRSDK Loaders...");
            if (managerSettings == null)
            {
                ModLog.Error("XRManagerSettings instance is null, cannot initialize loader.");
                return false;
            }
            ModLog.Debug("Manager settings: " + managerSettings);
            managerSettings.InitializeLoaderSync();
            if (managerSettings.activeLoader == null)
            {
                ModLog.Error("XRManager.activeLoader is null! Cannot initialize VR!");
                return false;
            }

            //Mirror mode option. Let's just leave the default option for now.
            /*OpenVRSettings openVrSettings = OpenVRSettings.GetSettings(false);
            if (openVrSettings != null)
            {
                OpenVRSettings.MirrorViewModes mirrorMode = VHVRConfig.GetMirrorViewMode();
                ModLog.Info("Mirror View Mode: " + mirrorMode);
                openVrSettings.SetMirrorViewMode(mirrorMode);
            }*/

            ModLog.Debug("Got non-null Active Loader.");
            return true;
        }

        //Not yet used:
        public static void TryRecenter()
        {
/*
            if (VRPlayer.ShouldPauseMovement)
            {
                return;
            }
*/
            List<XRInputSubsystem> inputSubsystems = new();
            SubsystemManager.GetInstances(inputSubsystems);
            foreach (var subsystem in inputSubsystems)
            {
                ModLog.Debug("Recentering Input Subsystem: " + subsystem);
                subsystem.TryRecenter();
            }
            
            // Trigger recentering head position on player body
            VRPlayer.headPositionInitialized = false;
            //VRPlayer.vrPlayerInstance?.ResetRoomscaleCamera();
        }


        //Debug entry to dump all the SteamVR settings
        private static void PrintSteamVRSettings()
        {
            SteamVR_Settings settings = SteamVR_Settings.instance;
            if (settings == null)
            {
                ModLog.Warning("SteamVR Settings are null.");
                return;
            }
            ModLog.Debug("SteamVR Settings:");
            ModLog.Debug("  actionsFilePath: " + settings.actionsFilePath);
            ModLog.Debug("  editorAppKey: " + settings.editorAppKey);
            ModLog.Debug("  activateFirstActionSetOnStart: " + settings.activateFirstActionSetOnStart);
            ModLog.Debug("  autoEnableVR: " + settings.autoEnableVR);
            ModLog.Debug("  inputUpdateMode: " + settings.inputUpdateMode);
            ModLog.Debug("  legacyMixedRealityCamera: " + settings.legacyMixedRealityCamera);
            ModLog.Debug("  mixedRealityCameraPose: " + settings.mixedRealityCameraPose);
            ModLog.Debug("  lockPhysicsUpdateRateToRenderFrequency: " + settings.lockPhysicsUpdateRateToRenderFrequency);
            ModLog.Debug("  mixedRealityActionSetAutoEnable: " + settings.mixedRealityActionSetAutoEnable);
            ModLog.Debug("  mixedRealityCameraInputSource: " + settings.mixedRealityCameraInputSource);
            ModLog.Debug("  mixedRealityCameraPose: " + settings.mixedRealityCameraPose);
            ModLog.Debug("  pauseGameWhenDashboardVisible: " + settings.pauseGameWhenDashboardVisible);
            ModLog.Debug("  poseUpdateMode: " + settings.poseUpdateMode);
            ModLog.Debug("  previewHandLeft: " + settings.previewHandLeft);
            ModLog.Debug("  previewHandRight: " + settings.previewHandRight);
            ModLog.Debug("  steamVRInputPath: " + settings.steamVRInputPath);
        }

        //Debug entry to dump all the OpenVR settings
        private static void PrintOpenVRSettings()
        {
            OpenVRSettings settings = OpenVRSettings.GetSettings(false);
            if (settings == null)
            {
                ModLog.Warning("OpenVRSettings are null.");
                return;
            }
            ModLog.Debug("OpenVR Settings:");
            ModLog.Debug("  StereoRenderingMode: " + settings.StereoRenderingMode);
            ModLog.Debug("  InitializationType: " + settings.InitializationType);
            ModLog.Debug("  ActionManifestFileRelativeFilePath: " + settings.ActionManifestFileRelativeFilePath);
            ModLog.Debug("  MirrorView: " + settings.MirrorView);

        }

        //Debug entry to dump all the Unity XRSettings
        private static void PrintUnityXRSettings()
        {
            ModLog.Debug("Unity.XR.XRSettings: ");
            ModLog.Debug("  enabled: " + XRSettings.enabled);
            ModLog.Debug("  deviceEyeTextureDimension: " + XRSettings.deviceEyeTextureDimension);
            ModLog.Debug("  eyeTextureDesc: " + XRSettings.eyeTextureDesc);
            ModLog.Debug("  eyeTextureHeight: " + XRSettings.eyeTextureHeight);
            ModLog.Debug("  eyeTextureResolutionScale: " + XRSettings.eyeTextureResolutionScale);
            ModLog.Debug("  eyeTextureWidth: " + XRSettings.eyeTextureWidth);
            ModLog.Debug("  gameViewRenderMode: " + XRSettings.gameViewRenderMode);
            ModLog.Debug("  isDeviceActive: " + XRSettings.isDeviceActive);
            ModLog.Debug("  loadedDeviceName: " + XRSettings.loadedDeviceName);
            ModLog.Debug("  occlusionMaskScale: " + XRSettings.occlusionMaskScale);
            ModLog.Debug("  renderViewportScale: " + XRSettings.renderViewportScale);
            ModLog.Debug("  showDeviceView: " + XRSettings.showDeviceView);
            ModLog.Debug("  stereoRenderingMode: " + XRSettings.stereoRenderingMode);
            ModLog.Debug("  supportedDevices: " + XRSettings.supportedDevices);
            ModLog.Debug("  useOcclusionMesh: " + XRSettings.useOcclusionMesh);
        }

    }
}
