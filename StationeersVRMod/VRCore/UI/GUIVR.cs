using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersVR.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UI.ImGuiUi;
using UnityEngine;

namespace StationeersVR.VRCore.UI
{
    internal class GUIVR : MonoBehaviour
    {
        public static GameObject gazeCursor;
        public static Transform panelClothing;
        public static Transform panelStatusInfo;
        public static Transform panelinWorldToolTip;
        public static Transform panelGameMenu;

        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.ManagerAwake))]
        public static class WorldManager_ManagerAwake_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                VRManager.TryRecenter();
                gazeCursor = new GameObject("GazeCursor");
                DontDestroyOnLoad(gazeCursor);
                gazeCursor.AddComponent<SimpleGazeCursor>();
                panelClothing = GameObject.Find("GameCanvas/PanelClothing").transform;
                panelStatusInfo = GameObject.Find("GameCanvas/PanelStatusInfo").transform;
                panelinWorldToolTip = GameObject.Find("GameCanvas/PanelInWorldToolTip").transform;
                ModLog.Error("panelClothing: " + panelClothing);
                ModLog.Error("panelStatusInfo: " + panelStatusInfo);
                ModLog.Error("panelinWorldToolTip: " + panelinWorldToolTip);
                panelClothing.localPosition = new Vector3(panelClothing.localPosition.x + 1000, panelClothing.localPosition.y, panelClothing.localPosition.z);
                panelStatusInfo.localPosition = new Vector3(panelStatusInfo.localPosition.x - 900, panelStatusInfo.localPosition.y, panelStatusInfo.localPosition.z);
                
               // panelGameMenu = GameObject.Find("AlertCanvas").transform;
                //ModLog.Error("panelGameMenu: " + panelGameMenu);
               // panelGameMenu.localScale = new Vector3(0.5f, 0.5f, 0.5f);
               //panelGameMenu.position = new Vector3(panelGameMenu.localPosition.x + 4000, panelGameMenu.localPosition.y, panelGameMenu.localPosition.z);
            }
        }
    }
}
