using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
﻿using Assets.Scripts.Util;
using HarmonyLib;
using StationeersVR.Utilities;
using StationeersVR.VRCore;
using StationeersVR.VRCore.UI;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace StationeersVR.Patches
{
    internal class MousePatches
    {
        [HarmonyPatch(typeof(InputHelpers), nameof(InputHelpers.GetCameraRay))]
        public static class InputHelpers_GetCameraRay_Patch
        {
            public static Ray result;
            [HarmonyPrefix]
            static bool Prefix(ref Ray __result)
            {
                Vector2 pos = SimpleGazeCursor.GetRayCastMode();
                __result = Camera.current.ScreenPointToRay(pos);
                return false;
            }
        }
       
        [HarmonyPatch(typeof(InputMouse), nameof(InputMouse.GetHoverWorldSlot))]
        public static class InputMouse_GetHoverWorldSlot_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ref Slot __result)
            {
                if (Physics.Raycast(Camera.current.ScreenPointToRay(SimpleGazeCursor.GetRayCastMode()), out var hitInfo, InputMouse.MaxInteractDistance, CursorManager.Instance.CursorHitMask))
                {
                    Thing componentInParent = hitInfo.transform.GetComponentInParent<Thing>();
                    if (componentInParent != null)
                    {
                        Interactable interactable = componentInParent.GetInteractable(hitInfo.collider);
                        if (interactable != null && interactable.Slot != null)
                        {
                            __result = interactable.Slot;
                            return false;
                        }
                    }
                }
                __result = null;
                return false;
            }
        }

        [HarmonyPatch(typeof(DraggableWindow), nameof(DraggableWindow.OnDrag))]
        public static class DraggableWindow_OnDrag_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(PointerEventData eventData, DraggableWindow __instance)
            {
                Vector3 worldPoint = Vector3.zero;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(__instance.RectTransform, eventData.position, eventData.pressEventCamera, out worldPoint))
                {
                    Vector2 test = new Vector2(Input.mousePosition.x / Screen.width * Camera.current.pixelWidth, Input.mousePosition.y / Screen.height * Camera.current.pixelHeight);
                    Vector3 posi = Camera.current.ScreenPointToRay(test).GetPoint(2);
                    posi.z = GUIVR.gameCanvas.position.z;
                    __instance.RectTransform.position = posi;
                    __instance.ClampToScreen();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(InputMouse), nameof(InputMouse.Idle))]
        public static class InputMouse_Idley_Patch
        {
            public static Ray result;
            [HarmonyPrefix]
            static bool Prefix(InputMouse __instance)
            {
                PassiveTooltip passiveTooltip = default(PassiveTooltip);
                __instance.DraggedThing = null;
                if (Physics.Raycast(Camera.current.ScreenPointToRay(SimpleGazeCursor.GetRayCastMode()), out var hitInfo, InputMouse.MaxInteractDistance, CursorManager.Instance.CursorHitMask))
                {
                   // ModLog.Error("Idle()");
                    __instance.CursorTransform = hitInfo.transform;
                    __instance.CursorThing = Thing.Find(hitInfo.collider);
                    __instance.CursorItem = __instance.CursorThing as Item;
                    Interactable interactable = null;
                    if (__instance.CursorThing != null && !InputMouse.IsMouseOverUi)
                    {
                        passiveTooltip = __instance.CursorThing.GetPassiveTooltip(hitInfo.collider);
                        interactable = __instance.CursorThing.GetInteractable(hitInfo.collider);
                        if (interactable != null && interactable.Slot == null && !InputMouse.IsMouseOverUi)
                        {
                            __instance.HandleMouseInteraction(interactable);
                            InputMouse.WorldInteractable = interactable;
                        }
                        else if (interactable != null && interactable.Slot != null && !InputMouse.IsMouseOverUi)
                        {
                            InputMouse.WorldInteractable = interactable;
                            __instance.HandleMouseInteraction(interactable);
                        }
                        else if (__instance.CursorItem != null)
                        {
                            Color color = (InventoryManager.ActiveHandSlot.Occupant ? Color.red : Color.green);
                            CursorManager.SetSelection(__instance.CursorItem.GetSelection(), color);
                            InputMouse.WorldInteractable = null;
                            Tooltip.SetColorForItemAction(ref passiveTooltip, __instance.CursorThing);
                        }
                        else
                        {
                            CursorManager.SetSelectionVisibility(isVisible: false);
                            CursorManager.ClearLastSelectionId();
                            InputMouse.WorldInteractable = null;
                        }
                        if (interactable != null)
                        {
                            Tooltip.SetValuesForInteractable(ref passiveTooltip, __instance.CursorThing, interactable);
                        }
                        passiveTooltip.FollowMouseMovement = true;
                        InventoryManager.Instance.TooltipRef.HandleToolTipDisplay(passiveTooltip);
                    }
                    else
                    {
                        CursorManager.SetSelectionVisibility(isVisible: false);
                        CursorManager.ClearLastSelectionId();
                        __instance.ClearTooltip();
                        InputMouse.WorldInteractable = null;
                    }
                    if (KeyManager.GetMouseDown("Primary") && !InputMouse.IsMouseOverUi)
                    {
                        if (interactable != null && interactable.Slot == null)
                        {
                            interactable.PlayerInteractWith(InventoryManager.ActiveHandSlot);
                        }
                        else if (interactable != null && interactable.Slot != null && SlotDisplayButton.CurrentSlot == null && InventoryWindow.CurrentWindow == null)
                        {
                            __instance.WorldMode = WorldMouseMode.Click;
                            __instance.MousePosition = Input.mousePosition;
                        }
                        else if ((bool)__instance.CursorItem)
                        {
                            __instance.WorldMode = WorldMouseMode.Click;
                            __instance.MousePosition = Input.mousePosition;
                            return false;
                        }
                    }
                }
                return false;
            }
        }
    }
}
