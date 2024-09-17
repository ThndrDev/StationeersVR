using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;
using Assets.Scripts;
using UnityEngine.XR;
using Assets.Scripts.Util;
using ImGuiNET;
using Valve.VR.InteractionSystem;
using StationeersVR.Utilities;
using StationeersVR.VRCore;
using StationeersVR.Patches;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace StationeersVR
{
    [BepInPlugin("StationeersVR", "Stationeers VR", "0.1.0.0")]
    public class StationeersVR : BaseUnityPlugin
    {
        public static StationeersVR Instance;

        private GameObject vrPlayer;
        private GazeBasicInputModule gazeInput;
        //private GameObject vrGui;

        void Awake()
        {
            StationeersVR.Instance = this;
            ModLog.Info("Loading StationeersVR mod");
            ConfigFile.HandleConfig(this);     // read/create the configuration file parameters
            var harmony = new Harmony("net.StationeersVR.patches");
            try
            {
                harmony.PatchAll();
                ModLog.Info("Harmony Patch succeeded");
            }
            catch (System.Exception e)
            {
                ModLog.Error("Harmony Patch Failed");
                ModLog.Error(e.ToString());
            }
        }


        void Start()
        {
            ModLog.Debug("Running StartStationeersVR()");  
            StartStationeersVR();
        }

        void Update()
        {
            if (ConfigFile.NonVrPlayer)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                //VRManager.tryRecenter();
            }
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                dumpall();
            }
        }

        void StartStationeersVR()
        {
            // For some reason, harmony patching outside of the StationeersVR namespace is not working. No idea why
            //HarmonyPatcher.DoPatching();


            //Later we should implement an option for non VR players to play together with VR players in multiplayer. But for now let's just force false in here
            /*
                        if (StationeersVR.NonVrPlayer)
                        {
                            ModLog.Debug("Non VR Mode Patching Complete.");
                            return;
                        }
            */
            if (VRManager.InitializeVR())
            {
                VRManager.StartVR();
                vrPlayer = new GameObject("VRPlayer");
                DontDestroyOnLoad(vrPlayer);
                vrPlayer.AddComponent<VRPlayer>();
                gazeInput = this.GetOrAddComponent<GazeBasicInputModule>();
                gazeInput.forceModuleActive = true;
               
                /*vrGui = new GameObject("VRGui");
                DontDestroyOnLoad(vrGui);
                vrGui.AddComponent<VRGUI>();
                if (VHVRConfig.RecenterOnStart())
                {
                    VRManager.tryRecenter();
                }*/
            }
            else
            {
                ModLog.Error("Could not initialize VR.");
                enabled = false;
            }
        }


        void dumpall()
        {
            foreach (var o in GameObject.FindObjectsOfType<GameObject>())
            {
                ModLog.Debug("Name + " + o.name + "   Layer = " + o.layer);
            }
        }
    }
}