using HarmonyLib;
using Assets.Scripts.UI;
using StationeersVR.Utilities;

namespace StationeersVR
{
    [HarmonyPatch(typeof(ImGuiLoadingScreen), "SetActive")]
    public class ImGuiLoadingScreen_SetActive_Patch
    {
        static void Postfix(bool active)
        {
            if (active)
            {
                
                VROverlay.ShowLoadingScreenInVR("LoadingScreen", ImGuiLoadingScreen.backgroundTexture, 10f, 3f, 0.2f);
            }
            else
            {
                VROverlay.HideLoadingScreenInVR();
            }
        }  
    }
}