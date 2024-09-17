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

        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.ManagerAwake))]
        public static class WorldManager_ManagerAwake_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                gazeCursor = new GameObject("GazeCursor");
                DontDestroyOnLoad(gazeCursor);
                gazeCursor.AddComponent<SimpleGazeCursor>();
                panelClothing = GameObject.Find("GameCanvas/PanelClothing").transform;
                panelStatusInfo = GameObject.Find("GameCanvas/PanelStatusInfo").transform;
                panelinWorldToolTip = GameObject.Find("GameCanvas/PanelInWorldToolTip").transform;
                ModLog.Error("panelClothing: " + panelClothing);
                ModLog.Error("panelStatusInfo: " + panelStatusInfo);
                ModLog.Error("panelinWorldToolTip: " + panelinWorldToolTip);
                panelClothing.transform.localPosition = new Vector3(panelClothing.transform.localPosition.x + 1000, panelClothing.transform.localPosition.y, panelClothing.transform.localPosition.z);
                panelStatusInfo.transform.localPosition = new Vector3(panelStatusInfo.transform.localPosition.x - 900, panelStatusInfo.transform.localPosition.y, panelStatusInfo.transform.localPosition.z);
                Vector3 defaultpos = new Vector3(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f, InputMouse.MaxInteractDistance);
                Vector3 posi = Camera.current.ScreenPointToRay(Input.mousePosition).GetPoint(InputMouse.MaxInteractDistance);
                if (Cursor.lockState == CursorLockMode.Locked)
                    panelinWorldToolTip.transform.localPosition = Camera.current.ScreenToWorldPoint(defaultpos);
                else
                    panelinWorldToolTip.transform.localPosition = SimpleGazeCursor.GetRayCastMode();
            }
        }
    }
}
