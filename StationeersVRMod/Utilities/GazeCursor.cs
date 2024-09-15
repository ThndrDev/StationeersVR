using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using static ImGuiNET.Unity.CursorShapesAsset;
using System.Collections.Generic;
using ImGuiNET;
using UI.ImGuiUi;
using Assets.Scripts.UI;
using Assets.Scripts.Objects;
using Assets.Scripts;
using Assets.Scripts.Util;
using UnityEngine.UI;
using System.Linq;
using Discord;
using Objects.Items;
using static UnityEngine.UIElements.UIR.Allocator2D;
using Assets.Scripts.Inventory;

namespace StationeersVR.Utilities
{

    public class SimpleGazeCursor : MonoBehaviour
    {
        public static RaycastHit triggerObject;
        public GameObject cursorPrefab;
        public float maxCursorDistance = 30;
        public static LineRenderer line;

        public static GameObject cursorInstance;

        // Use this for initialization
        void Start()
        {
            cursorInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursorInstance.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            gameObject.AddComponent<LineRenderer>();
            line = gameObject.GetComponent<LineRenderer>();
            line.startColor = new Color(0f, 1f, 1f, 1f);
            line.endColor = new Color(1f, 0f, 0f, 1f);
            line.startWidth = 0.004f;
            line.endWidth = 0.005f;
            if (cursorInstance.GetComponent<SphereCollider>() != null)
                cursorInstance.GetComponent<SphereCollider>().enabled = false;
            DontDestroyOnLoad(cursorInstance);
        }

        // Update is called once per frame
        public static RaycastResult raycast;
        void Update()
        {
            UpdateCursor();
        }

        /// <summary>
        /// Updates the cursor based on what the camera is pointed at.
        /// </summary>
        /// 
        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            int count = candidates.Count;
            for (int i = 0; i < count; i++)
            {
                if (!(candidates[i].gameObject == null))
                {
                    return candidates[i];
                }
            }
            return default;
        }

        public static Vector2 GetRayCastMode()
        {

            if (!InventoryManager.AllowMouseControl)
            {
                return Input.mousePosition;
            }
            else
            {
                return new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f);
            }
        }


        private void UpdateCursor()
        {
            // Create a gaze ray pointing forward from the camera
            if (Camera.current != null)
            {
                Vector2 pos = GetRayCastMode();
                Vector3 lookAtPosition = Camera.current.ScreenToWorldPoint(new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, Camera.current.nearClipPlane));
                line.SetPosition(0, lookAtPosition);
                //This Raycast that hits the UI
                Ray ray = Camera.current.ScreenPointToRay(pos);
                if (raycast.gameObject != null && raycast.distance < InputMouse.MaxInteractDistance)
                {
                    line.SetPosition(1, raycast.worldPosition);
                    cursorInstance.transform.position = raycast.worldPosition;
                }
                //This Raycast hits switches,items any interactable but anything with UI
                else if (CursorManager._raycastHit.transform != null && CursorManager._raycastHit.distance < InputMouse.MaxInteractDistance)
                {

                    cursorInstance.transform.position = CursorManager._raycastHit.point;
                    line.SetPosition(1, CursorManager._raycastHit.point);
                }
            }
        }
    }
}