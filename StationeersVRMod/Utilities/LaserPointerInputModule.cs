using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using ImGuiNET;
using StationeersVR.Utilities;
using StationeersVR.VRCore;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UI.RocketImGuiWrapper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Valve.VR;
using static UnityEngine.SendMouseEvents;


//this is the ppointer input, there is a buncha of stuff that does not need to be here but I put here for testing, will change in the future
namespace StationeersVR.Patches
{
    public class LaserPointerInputModule : StandaloneInputModule
    {
        private readonly MouseState m_MouseState = new MouseState();

        protected LaserPointerInputModule()
        { }

        [SerializeField]
        private bool m_ForceModuleActive;

        public bool forceModuleActive
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        public override bool IsModuleSupported()
        {
            return forceModuleActive;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            if (m_ForceModuleActive)
                return true;

            return false;
        }


        public override void Process()
        {
            LaserControl();
        }

        new void Awake()
        {
            gameObject.AddComponent<LineRenderer>();
            line = gameObject.GetComponent<LineRenderer>();
            line.startColor = new Color(0f, 1f, 1f, 1f);
            line.endColor = new Color(1f, 0f, 0f, 1f);
            line.startWidth = 0.004f;
            line.endWidth = 0.005f;
        }

        protected static PointerEventData.FramePressState StateForButton(string buttonCode)
        {
            var pressed = GetButtonDown(buttonCode, SteamVR_Input_Sources.Any);
            var released = GetButtonUp(buttonCode, SteamVR_Input_Sources.Any);
            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        public static float rotationX = 0;
        public static float rotationY = 0;
        public static float rotationZ = 0;
        public static float rotationW = 0;

        public static LineRenderer line;

        protected MouseState CreateLaserPointerEvent(int id)
        {
            PointerEventData pointerData;
            var created = GetPointerData(-1, out pointerData, true);

            pointerData.Reset();
            pointerData.delta = Vector2.zero;
            if (VRPlayer.rightPointer != null)
                pointerData.position = Camera.current.WorldToScreenPoint(VRPlayer.rightPointer.transform.position + VRPlayer.rightPointer.transform.forward * InputMouse.MaxInteractDistance);
            pointerData.scrollDelta = Vector2.zero;
            pointerData.button = PointerEventData.InputButton.Left;

            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

            var raycast = FindFirstRaycast(m_RaycastResultCache);

            if (VRPlayer.rightPointer != null)
                line.SetPosition(0, VRPlayer.rightPointer.transform.position);
            if(raycast.isValid)
                line.SetPosition(1, raycast.worldPosition);
            else
                line.SetPosition(1, VRPlayer.rightPointer.transform.forward);
            pointerData.pointerCurrentRaycast = raycast;
            var can = GameObject.FindObjectsOfType<Canvas>();
            m_RaycastResultCache.Clear();

            PointerEventData rightData;
            GetPointerData(-2, out rightData, true);
            CopyFromTo(pointerData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            m_MouseState.SetButtonState(
                PointerEventData.InputButton.Left,
                StateForButton("LeftClick"),
                pointerData);
            m_MouseState.SetButtonState(
                PointerEventData.InputButton.Right,
                StateForButton("RightClick"),
                rightData);

            return m_MouseState;
        }


        private void LaserControl()
        {
            // ModLog.Error("GazeControl");
            var pointerData = CreateLaserPointerEvent(0);
            var leftPressData = pointerData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var rightPressData = pointerData.GetButtonState(PointerEventData.InputButton.Right).eventData;

            ProcessPress(leftPressData.buttonData, leftPressData.PressedThisFrame(), leftPressData.ReleasedThisFrame());
            ProcessMove(leftPressData.buttonData);
            ProcessDrag(leftPressData.buttonData);

            ProcessPress(rightPressData.buttonData, rightPressData.PressedThisFrame(), rightPressData.ReleasedThisFrame());
            ProcessMove(rightPressData.buttonData);
            ProcessDrag(rightPressData.buttonData);
        }

        public static bool GetButtonDown(string buttonName, SteamVR_Input_Sources source)
        {
            return SteamVR_Input.GetStateDown(buttonName.ToString(), source);
        }

        public static bool GetButtonUp(string buttonName, SteamVR_Input_Sources source)
        {
            return SteamVR_Input.GetStateUp(buttonName.ToString(), source);
        }

        public static bool GetButton(string buttonName, SteamVR_Input_Sources source)
        {
            return SteamVR_Input.GetState(buttonName.ToString(), source);
        }

        private void ProcessPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;


                //pointerEvent.dragging = true;
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
                //ModLog.Error("PointerDrag: " + pointerEvent.pointerDrag);
                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }


            if (released)
            {

                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.pointerDrag = null;

                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Input: LaserPointerInputModule");
            var pointerData = GetLastPointerEventData(0);
            if (pointerData != null)
                sb.AppendLine(pointerData.ToString());

            return sb.ToString();
        }
    }
}