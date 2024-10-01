using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using SimpleSpritePacker;
using StationeersVR.Utilities;
using StationeersVR.VRCore;
using System;
using System.Collections.Generic;
using System.Text;
using Trading;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace StationeersVR.Patches
{
    internal class ToolTipPatches
    {
        public static GameObject go;

        [HarmonyPatch(typeof(Tooltip), nameof(Tooltip.Start))]
        public static class Tooltip_Start_Patch
        {
            [HarmonyPrefix]
            static void Prefix(Tooltip __instance)
            {
                go = new GameObject("ToolTip");
                if (go.GetComponent<Canvas>() == null)
                    go.AddComponent<Canvas>();
                __instance.transform.SetParent(go.transform, false);
                go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                __instance.transform.Rotate(0, 180, 0);
                __instance.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                go.GetComponent<Canvas>().sortingOrder = __instance.transform.GetComponentInParent<Canvas>().sortingOrder + 1;
            }
        }

        [HarmonyPatch(typeof(PanelToolTipScreenSpace), nameof(PanelToolTipScreenSpace.Initialize))]
        public static class PanelToolTipScreenSpace_Initialize_Patch
        {
            [HarmonyPrefix]
            static void Prefix(PanelToolTipScreenSpace __instance)
            {
                go = new GameObject("ToolTipScreenSpace");
                if (go.GetComponent<Canvas>() == null)
                    go.AddComponent<Canvas>();
                __instance.Transform.SetParent(go.transform, false);
                __instance.Transform.GetComponent<RectTransform>().localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                go.GetComponent<Canvas>().sortingOrder = __instance.Transform.GetComponentInParent<Canvas>().sortingOrder + 1;
            }
        }

        [HarmonyPatch(typeof(PanelToolTipScreenSpace), nameof(PanelToolTipScreenSpace.LateUpdate))]
        public static class PanelToolTipScreenSpace_LateUpdate_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(PanelToolTipScreenSpace __instance)
            {
                if (__instance.IsVisible)
                {
                    if (__instance._tooltipToUpdate != null && !__instance._tooltipToUpdate.TooltipIsVisible)
                    {
                        __instance.ClearToolTip();
                        return false;
                    }
                    __instance._tooltipToUpdate?.DoUpdate();
                    Vector3 mousePosition = SimpleGazeCursor.cursorInstance.transform.position;
                   // Vector3 quadrant = UITooltipPanel.GetQuadrant(mousePosition);
                  //  Vector3 vector = new Vector3( __instance._offset,  __instance._offset);
                    __instance.Transform.LookAt(Camera.current.transform, Vector3.up);
                    __instance.Transform.position = mousePosition + new Vector3(0,0.02f,0);// + vector;// - vector;// * __instance._canvas.scaleFactor;
                    __instance.Transform.Rotate(0, 180, 0);
                    __instance.RefreshText();
                }
            
                return false;
            }
        }

        [HarmonyPatch(typeof(Tooltip), nameof(Tooltip.LateUpdate))]
        public static class Tooltip_LateUpdate_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(Tooltip __instance)
            {
                __instance.WantDraw = false;
                __instance.Dirty = false;
                __instance.Mode = TooltipMode.Hidden;
                if (__instance.gameObject.activeSelf && __instance.TooltipFollowMouse)
                {
                    __instance.transform.LookAt(Camera.current.transform, Vector3.up);
                    __instance.transform.position = SimpleGazeCursor.cursorInstance.transform.position + new Vector3(0, 0.2f, 0);
                    __instance.transform.Rotate(0, 180, 0);

                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Tooltip), nameof(Tooltip.HandleToolTipDisplay))]
        public static class Tooltip_HandleToolTipDisplay_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(PassiveTooltip cursorPassiveTooltip, Tooltip __instance)
            {
                __instance.WantDraw = true;
                if ((bool)InventoryManager.ParentHuman && (bool)InventoryManager.Instance.ActiveHand.Slot.Occupant && InventoryManager.Instance.ActiveHand.Slot.Occupant is Tablet)
                {
                    return false;
                }
                __instance.SetUpToolTip(cursorPassiveTooltip.Action, cursorPassiveTooltip);
                bool flag = !InventoryManager.Instance.UIProgressionBar.IsVisible && (cursorPassiveTooltip.ShowRotate || cursorPassiveTooltip.ShowScroll || (cursorPassiveTooltip.ShowAction && __instance._hasAction));
                bool flag2 = !InventoryManager.Instance.UIProgressionBar.IsVisible && (__instance._hasAction || __instance._hasConstruction || __instance._hasDeconstruction || __instance._hasPlacement || __instance._hasState || __instance._hasTitle || __instance._hasRepair || __instance._hasExtended);
                if (!flag2 && !flag)
                {
                    __instance.UiComponentRenderer.SetVisible(isVisble: false);
                    return false;
                }
                if (!__instance.UiComponentRenderer.IsVisible)
                {
                    __instance.UiComponentRenderer.SetVisible(isVisble: true);
                }
                __instance.FullToolTipHotKeys.SetVisible(flag);
                __instance.FullToolTipPanel.SetVisible(flag2);
                if (flag2)
                {
                    __instance.InfoConstructionGameObject.SetVisible(__instance._hasConstruction);
                    __instance.InfoRepairGameObject?.SetVisible(__instance._hasRepair);
                    __instance.InfoDeconstructGameObject.SetVisible(__instance._hasDeconstruction);
                    __instance.InfoPlacementGameObject.SetVisible(__instance._hasPlacement);
                    __instance.StateRenderer.SetVisible(__instance._hasState);
                    __instance.ExtendedRenderer.SetVisible(__instance._hasExtended);
                    __instance.TitleRenderer.SetVisible(__instance._hasTitle);
                    if (__instance.Slider >= 0f)
                    {
                        __instance.ToolTipSliderRenderer.SetVisible(isVisble: true);
                        __instance.TooltipSlider.value = __instance.Slider;
                        __instance.TooltipSliderFill.color = StatusUpdates.GetDamageColor(1f - __instance.Slider);
                    }
                    else
                    {
                        __instance.ToolTipSliderRenderer.SetVisible(isVisble: false);
                    }
                }
                if (flag)
                {
                    __instance.ActionRotateObject.SetVisible(cursorPassiveTooltip.ShowRotate);
                    __instance.ActionScrollCycle.SetVisible(cursorPassiveTooltip.ShowScroll);
                    __instance.ActionConstructionRotate.SetVisible(cursorPassiveTooltip.ShowConstructionRotate);
                    __instance.ActionBuildObject.SetVisible(cursorPassiveTooltip.ShowAction && __instance._hasAction);
                }
                __instance.TooltipFollowMouse = cursorPassiveTooltip.FollowMouseMovement;
                if (!__instance.TooltipFollowMouse)
                {
                    __instance.RectTransform.LookAt(Camera.current.transform, Vector3.up);
                    __instance.transform.position = SimpleGazeCursor.cursorInstance.transform.position + new Vector3(0, 0.2f, 0);
                    __instance.RectTransform.Rotate(0, 180, 0);
                }
                return false;
            }
        }
    }
}
