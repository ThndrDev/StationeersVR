using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using SimpleSpritePacker;
using StationeersVR.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UI.ImGuiUi;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
        public static Transform alertCanvas;
        public static Transform gameCanvas;
        public static Transform panelHands;
        public static Transform helperHints;
        public static Transform inventoryWindows;
        public static Transform cursor;
        public static Transform stationpediaHint;
        public static Transform panelInputPrefabs;
        public static Transform panelInputText;
        public static Transform panelHelpMenu;
        public static Transform panelInputCode;
        public static Transform popupsCanvas;
        public static Transform panelDyanmicThing;
        public static Transform windowLeftHand;
        public static Transform windowRightHand;

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
                gameCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                gameCanvas.gameObject.layer = 27;
                ModLog.Error("GameCanvas: " + gameCanvas);

                alertCanvas = GameObject.Find("AlertCanvas").transform;
                alertCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                alertCanvas.gameObject.layer = 27;
                ModLog.Error("AlertCanvas: " + alertCanvas);

                popupsCanvas = GameObject.Find("PopupsCanvas").transform;
                popupsCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                popupsCanvas.gameObject.layer = 27;
                ModLog.Error("PopupsCanvas: " + popupsCanvas);

                panelDyanmicThing = GameObject.Find("PanelDynamicThing").transform;
                panelDyanmicThing.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                panelDyanmicThing.gameObject.layer = 27;
                ModLog.Error("PanelDynamicThing: " + panelDyanmicThing);

                panelHelpMenu = GameObject.Find("PanelHelpMenu").transform;
                panelHelpMenu.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                panelHelpMenu.gameObject.layer = 27;
                panelHelpMenu.GetComponent<Canvas>().sortingOrder = gameCanvas.GetComponent<Canvas>().sortingOrder + 1;
                ModLog.Error("PanelHelpMenu: " + panelHelpMenu);

                panelClothing = gameCanvas.Find("PanelClothing").transform;
                panelStatusInfo = gameCanvas.Find("PanelStatusInfo").transform;
                panelHands = gameCanvas.Find("PanelHands").transform;
                helperHints = gameCanvas.Find("HelperHints").transform;
                inventoryWindows = gameCanvas.Find("InventoryWindows").transform;
                inventoryWindows.gameObject.layer = 27;
                cursor = gameCanvas.Find("Cursor");
                stationpediaHint = gameCanvas.Find("StationpediaHint");
                panelInputPrefabs = gameCanvas.Find("PanelInputPrefabs");

                ModLog.Error("panelClothing: " + panelClothing);
                ModLog.Error("panelStatusInfo: " + panelStatusInfo);
                ModLog.Error("PanelHands: " + panelHands);
                ModLog.Error("HelperHints: " + helperHints);
                ModLog.Error("InventoryWindows: " + inventoryWindows);
                ModLog.Error("Cursor: " + cursor);
                ModLog.Error("StationpediaHint: " + stationpediaHint);
                ModLog.Error("PanelInputPrefabs: " + panelInputPrefabs);
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
            if (Camera.current != null && gameCanvas != null)
            {
                float scaleFactor = 2.5f / Camera.current.pixelWidth / 2;
                float scaleFactorx = 4.5f / Camera.current.pixelWidth / 2;
                float scaleFactory = 3.5f / Camera.current.pixelWidth / 2;
                float hudDistance = 0.8f;
                float hudVerticalOffset = +0f;
                if (GameObject.Find("PanelInputText") != null)
                {
                    panelInputText = GameObject.Find("PanelInputText").transform;
                    panelInputText.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    panelInputText.gameObject.transform.SetParent(Camera.current.transform, false);
                    panelInputText.transform.position = Camera.current.transform.position + Camera.current.transform.forward * hudDistance;
                    panelInputText.GetComponent<RectTransform>().localScale = Vector3.one * 8.0f / Camera.current.pixelWidth / 2 * hudDistance * 1;
                    panelInputText.gameObject.layer = 27;
                }
                if (GameObject.Find("PanelInputCode") != null)
                {
                    panelInputCode = GameObject.Find("PanelInputCode").transform;
                    panelInputCode.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    panelInputCode.gameObject.layer = 27;
                    panelInputCode.GetComponent<Canvas>().sortingOrder = gameCanvas.GetComponent<Canvas>().sortingOrder + 1;
                    panelInputCode.LookAt(Camera.current.transform);
                    panelInputCode.Rotate(0, 180, 0);
                    panelInputCode.position = Camera.current.transform.position + Camera.current.transform.forward * 1;
                    panelInputCode.GetComponent<RectTransform>().localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                }

                var playerInstance = VRPlayer.vrPlayerInstance._vrCameraRig.transform;
                var offsetPosition = new Vector3(0f, 1.5f, 1.0f);
                panelHelpMenu.LookAt(Camera.current.transform);
                panelHelpMenu.Rotate(0, 180, 0);
                panelHelpMenu.GetComponent<RectTransform>().localScale = new Vector3(0.001f, 0.001f, 0.001f);
                panelHelpMenu.position = Camera.current.transform.position + Camera.current.transform.forward * 1;

                panelDyanmicThing.SetParent(Camera.current.transform,false);
                panelDyanmicThing.LookAt(Camera.current.transform);
                panelDyanmicThing.Rotate(0, 180, 0);
                panelDyanmicThing.GetComponent<RectTransform>().localScale = new Vector3(0.003f, 0.003f, 0.003f);
                panelDyanmicThing.position = Camera.current.transform.position + Camera.current.transform.forward * 1;
                panelDyanmicThing.localPosition = new Vector3(panelDyanmicThing.localPosition.x +3, panelDyanmicThing.localPosition.y-1, panelDyanmicThing.localPosition.z);
                panelDyanmicThing.transform.localRotation = Quaternion.Euler(Vector3.zero);

                popupsCanvas.LookAt(Camera.current.transform);
                popupsCanvas.Rotate(0, 180, 0);
                popupsCanvas.GetComponent<RectTransform>().localScale = new Vector3(0.001f, 0.001f, 0.001f);
                popupsCanvas.position = Camera.current.transform.position + Camera.current.transform.forward * hudDistance;

                gameCanvas.gameObject.transform.SetParent(Camera.current.transform, false);
                gameCanvas.gameObject.transform.position = Camera.current.transform.position + Camera.current.transform.forward * hudDistance;//new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f);
                gameCanvas.transform.localPosition = new Vector3(0, 0 + hudVerticalOffset, hudDistance);
                gameCanvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor * hudDistance * 1;
                gameCanvas.transform.localRotation = Quaternion.Euler(Vector3.zero);

                if (gameCanvas.Find("WindowLeftHand") != null)
                {
                    windowLeftHand = gameCanvas.Find("WindowLeftHand");
                    //ModLog.Error("WindowLeftHand: " + windowLeftHand);
                    if (windowLeftHand.position.z != gameCanvas.position.z)
                        windowLeftHand.position = new Vector3(windowLeftHand.position.x, windowLeftHand.position.y, gameCanvas.position.z);
                }
                if (gameCanvas.Find("WindowRightHand") != null)
                {
                    windowRightHand = gameCanvas.Find("WindowRightHand");
                    //ModLog.Error("WindowRightHand: " + windowRightHand);
                    if(windowRightHand.position.z != gameCanvas.position.z)
                        windowRightHand.position = new Vector3(windowRightHand.position.x, windowRightHand.position.y, gameCanvas.position.z);
                }
                alertCanvas.SetParent(playerInstance,false);
                alertCanvas.gameObject.layer = 27;
                lastVrPlayerRotation = playerInstance.rotation;
                
                float rotationDelta = playerInstance.rotation.eulerAngles.y - lastVrPlayerRotation.eulerAngles.y;
                lastVrPlayerRotation = playerInstance.rotation;
                var newRotation = Quaternion.LookRotation(getCurrentGuiDirection(), playerInstance.up);
                newRotation *= Quaternion.AngleAxis(rotationDelta, Vector3.up);
                alertCanvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor * hudDistance * 1;
                alertCanvas.LookAt(playerInstance.position);
                alertCanvas.rotation = playerInstance.rotation;
                alertCanvas.transform.position = playerInstance.position + alertCanvas.rotation * offsetPosition;
            }
        }

        public static void ToggleHudElements()
        {
            if (!InventoryManager.AllowMouseControl)
            {
                if (panelClothing)
                    panelClothing.gameObject.SetActive(true);
                if (panelStatusInfo)
                    panelStatusInfo.gameObject.SetActive(true);
                if (helperHints)
                    helperHints.gameObject.SetActive(true);
                if (inventoryWindows)
                    inventoryWindows.gameObject.SetActive(true);
                if (stationpediaHint)
                    stationpediaHint.gameObject.SetActive(true);
            }
            else
            {
                if (panelClothing)
                    panelClothing.gameObject.SetActive(false);
                if (panelStatusInfo)
                    panelStatusInfo.gameObject.SetActive(false);
                if (cursor)
                    cursor.gameObject.SetActive(false);
                if (helperHints)
                    helperHints.gameObject.SetActive(false);
                if (inventoryWindows)
                    inventoryWindows.gameObject.SetActive(false);
                if (stationpediaHint)
                    stationpediaHint.gameObject.SetActive(false);
            }
        }
    }
}
