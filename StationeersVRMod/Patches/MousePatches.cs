using Assets.Scripts.Util;
using HarmonyLib;
using StationeersVR.VRCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
                result = new Ray(VRPlayer.rightHand.transform.position, VRPlayer.rightHand.transform.forward);
                __result = result;
                return false;
            }
        }
    }
}
