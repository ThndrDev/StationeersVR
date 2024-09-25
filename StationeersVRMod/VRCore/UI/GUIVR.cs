using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersVR.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UI.ImGuiUi;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace StationeersVR.VRCore.UI
{
    internal class GUIVR : MonoBehaviour
    {
        public static GameObject gazeCursor;
        public static Transform panelClothing;
        public static Transform panelStatusInfo;
        public static Transform panelinWorldToolTip;
        public static Transform panelGameMenu;
        public static Transform alertCanvas;
        public static Transform gameCanvas;

        public static Quaternion lastVrPlayerRotation = Quaternion.identity;

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
                gameCanvas = GameObject.Find("GameCanvas").transform;
                alertCanvas = GameObject.Find("AlertCanvas").transform;
                ModLog.Error("AlertCanvas: " + alertCanvas);
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

        private static Vector3 getCurrentGuiDirection()
        {

            if (alertCanvas == null)
            {
                return Vector3.forward;
            }
            return alertCanvas.transform.forward;

        }

        public static void UpdateHud()
        {
            if (alertCanvas != null && VRPlayer.instance != null)
            {
                float scaleFactor = 2.5f / Camera.current.pixelWidth / 2;
                float hudDistance = 2;
                //alertCanvas.GetComponent<Canvas>().worldCamera = Camera.current;
                //ModLog.Error("worldCamera:" + Human.LocalHuman.AimIk.transform.position.z);
                var playerInstance = VRPlayer.vrPlayerInstance._vrCameraRig.transform;
                alertCanvas.SetParent(playerInstance);
                lastVrPlayerRotation = playerInstance.rotation;
                var offsetPosition = new Vector3(1f, 1.5f, 2.0f);
                float rotationDelta = playerInstance.rotation.eulerAngles.y - lastVrPlayerRotation.eulerAngles.y;
                lastVrPlayerRotation = playerInstance.rotation;
                var newRotation = Quaternion.LookRotation(getCurrentGuiDirection(), playerInstance.up);
                newRotation *= Quaternion.AngleAxis(rotationDelta, Vector3.up);
                alertCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                alertCanvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor * hudDistance * 1;
                alertCanvas.LookAt(playerInstance.position);
                alertCanvas.rotation = playerInstance.rotation;
                alertCanvas.transform.position = playerInstance.position + alertCanvas.rotation * offsetPosition;
            }

            List<GameObject> canvas = new List<GameObject>();
            GameObject gCanvas = GameObject.Find("GameCanvas");
            GameObject aCanvas = GameObject.Find("AlertCanvas");
            GameObject cCanvas = GameObject.Find("CursorCanvas");
            GameObject sCanvas = GameObject.Find("SystemCanvas");
            GameObject fCanvas = GameObject.Find("FadeCanvas");
            GameObject puCanvas = GameObject.Find("PopupsCanvas");
            //GameObject pCanvas = GameObject.Find("PingCanvas");
            GameObject phCanvas = GameObject.Find("PanelHelpMenu");
            GameObject pwCanvas = GameObject.Find("PanelInWorldToolTip");
            //GameObject piCanvas = GameObject.Find("PanelInternal");
            //GameObject pdCanvas = GameObject.Find("PanelDynamicThing");
            //GameObject imgui = GameObject.Find("ImGUI");
            //GameObject popupCanvas = GameObject.Find("PopupsCanvas");
            //GameObject tooltipCanvas = GameObject.Find("TooltipCanvas");
            //GameObject valucomp = GameObject.Find("ValueCompass");
            if (canvas.Count == 0)
            {
                if (gCanvas != null)
                    canvas.Add(gCanvas);

                if (aCanvas != null)
                    // canvas.Add(aCanvas);

                    if (cCanvas != null)
                        canvas.Add(cCanvas);

                if (sCanvas != null)
                    canvas.Add(sCanvas);

                if (fCanvas != null)
                    canvas.Add(fCanvas);

                if (puCanvas != null)
                    canvas.Add(puCanvas);

                /* if (pCanvas != null)
                     canvas.Add(pCanvas);*/

                if (phCanvas != null)
                    canvas.Add(phCanvas);

                if (pwCanvas != null)
                    canvas.Add(pwCanvas);

                /*if (piCanvas != null)
                    canvas.Add(piCanvas);

                if (pdCanvas != null)
                    canvas.Add(pdCanvas);

                if (imgui != null)
                {
                    canvas.Add(imgui);
                    ModLog.Error("Imgui: " + imgui.name);
                }

                if (popupCanvas != null)
                    canvas.Add(popupCanvas);*/

                //if (tooltipCanvas != null)
                //   canvas.Add(tooltipCanvas);
                /*

                if (valucomp != null)
                    canvas.Add(valucomp);*/
            }

            if (canvas.Count > 0)
            {
                foreach (var can in canvas)
                {
                    if (can != null)
                    {
                        if (can.GetComponent<Canvas>() != null && Camera.current != null)
                        {
                            if (can.GetComponent<Canvas>().isRootCanvas)
                                setCameraHudPosition(can.GetComponent<Canvas>());
                        }
                    }
                }
            }
        }

        private static void setCameraHudPosition(Canvas canvas)
        {
            float scaleFactor = 3.5f / Camera.current.pixelWidth / 2;
            float scaleFactor1 = 2.0f / Camera.current.pixelWidth / 2;
            float hudDistance = 2;
            float hudVerticalOffset = +1.0f;
            float hudHorizontalOffset = 1.0f;
            canvas.renderMode = UnityEngine.RenderMode.WorldSpace;
            /* if (canvas.name == "AlertCanvas")
             {
                 //ModLog.Error("RenderMode: " + canvas.renderMode);
                 canvas.transform.SetParent(Camera.current.transform, false);
                 canvas.worldCamera = Camera.current;
                 canvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor1 * hudDistance * 1;
                 canvas.transform.position = new Vector3(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f, hudDistance);
                 canvas.transform.localPosition = new Vector3(0 + hudHorizontalOffset, 0, hudDistance);
             }
             else
             {*/
            canvas.gameObject.transform.SetParent(Camera.current.transform, false);
            canvas.gameObject.transform.position = new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f);

            canvas.transform.localPosition = new Vector3(0, 0 + hudVerticalOffset, hudDistance);
            canvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor * hudDistance * 1;
            canvas.transform.localRotation = Quaternion.Euler(Vector3.zero);
            //}
        }
    }
}
