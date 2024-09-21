using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using StationeersVR.VRCore.UI;

namespace StationeersVR.Utilities
{
    public class GazeBasicInputModule : PointerInputModule
    {
        private readonly MouseState m_MouseState = new MouseState();

        protected GazeBasicInputModule()
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
            GazeControl();
        }


        protected static PointerEventData.FramePressState StateForButton(int buttonCode)
        {
            var pressed = Input.GetMouseButtonDown(buttonCode);
            var released = Input.GetMouseButtonUp(buttonCode);
            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        protected MouseState CreateGazePointerEvent(int id)
        {
            PointerEventData data;
            if (Camera.current != null)
            {
                Vector2 pos = SimpleGazeCursor.GetRayCastMode();
                bool pointerData = GetPointerData(-1, out data, create: true);
                data.Reset();
                if (pointerData)
                {
                    data.position = pos;
                }
                Vector2 mousePosition = pos;
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    data.delta = mousePosition - data.position;
                    data.position = mousePosition;
                }
                else
                {
                    data.delta = mousePosition - data.position;
                    data.position = mousePosition;
                }
                data.scrollDelta = base.input.mouseScrollDelta;
                data.button = PointerEventData.InputButton.Left;
                base.eventSystem.RaycastAll(data, m_RaycastResultCache);
                RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
                data.pointerCurrentRaycast = pointerCurrentRaycast;
                SimpleGazeCursor.raycast = pointerCurrentRaycast;
                m_RaycastResultCache.Clear();
                GetPointerData(-2, out var data2, create: true);
                data2.Reset();
                CopyFromTo(data, data2);
                data2.button = PointerEventData.InputButton.Right;
                GetPointerData(-3, out var data3, create: true);
                data3.Reset();
                CopyFromTo(data, data3);
                data3.button = PointerEventData.InputButton.Middle;
                m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), data);
                m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), data2);
                m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data3);
            }
            return m_MouseState;
        }


        private void GazeControl()
        {
            var pointerData = CreateGazePointerEvent(0);

            var leftPressData = pointerData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if(ConfigFile.UseVrControls)
                ProcessPress(leftPressData.buttonData, leftPressData.PressedThisFrame() || VRControls.instance.GetButtonDown(KeyMap.PrimaryAction), leftPressData.ReleasedThisFrame() || VRControls.instance.GetButtonUp(KeyMap.PrimaryAction));
            else
                ProcessPress(leftPressData.buttonData, leftPressData.PressedThisFrame(), leftPressData.ReleasedThisFrame());
            ProcessMove(leftPressData.buttonData);

            if (Input.GetMouseButton(0))
            {
                ProcessDrag(leftPressData.buttonData);
            }
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

                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

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
            sb.AppendLine("Input: GazeBasicInputModule");
            var pointerData = GetLastPointerEventData(kMouseLeftId);
            if (pointerData != null)
                sb.AppendLine(pointerData.ToString());

            return sb.ToString();
        }
    }
}