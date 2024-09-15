using Assets.Scripts.UI;
﻿using Assets.Scripts.Util;
using CharacterCustomisation;
using HarmonyLib;
using ImGuiNET;
using StationeersVR.Utilities;
using StationeersVR.VRCore;
using System;
using System.Collections.Generic;
using System.Text;
using UI.ImGuiUi;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using static Assets.Scripts.Util.Defines;
using static RootMotion.FinalIK.InteractionTrigger;
using static UnityEngine.GraphicsBuffer;
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
                Vector2 pos = SimpleGazeCursor.GetRayCastMode();//new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f);
                __result = Camera.current.ScreenPointToRay(pos);
                return false;
            }
        }

       /* [HarmonyPatch(typeof(InputMouse), nameof(InputMouse.Idle))]
        public static class InputMouse_Idley_Patch
        {
            public static Ray result;
            [HarmonyPrefix]
            static bool Prefix(InputMouse __instance)
            {
                PassiveTooltip passiveTooltip = default(PassiveTooltip);
                __instance.DraggedThing = null;
                if (Physics.Raycast(CameraController.CurrentCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, InputMouse.MaxInteractDistance, CursorManager.Instance.CursorHitMask))
                {
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
}
