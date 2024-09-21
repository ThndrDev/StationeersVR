using HarmonyLib;
using Assets.Scripts.UI;
using StationeersVR.Utilities;

namespace StationeersVR
{
    [HarmonyPatch(typeof(ImGuiLoadingScreen), "SetActive")]
    public class ImGuiLoadingScreenPatch
    {
        static void Postfix(bool active)
        {
            if (active)
            {
                VROverlay.ShowLoadingScreenInVR();
            }
            else
            {
                VROverlay.HideLoadingScreenInVR();
            }
        }  
    }
}