using Assets.Scripts.UI;
using UnityEngine;
using Valve.VR;

namespace StationeersVR.Utilities
{

    static class VROverlay
    {
        private static ulong overlayHandle = 0;
        private static bool isOverlayInitialized = false;
        public static void ShowLoadingScreenInVR()
        {
            if (!isOverlayInitialized)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    EVROverlayError error = overlay.CreateOverlay("StationeersVR.LoadingScreen", "Loading Screen", ref overlayHandle);
                    if (error != EVROverlayError.None)
                    {
                        Debug.LogError("Failed to create overlay: " + error.ToString());
                        return;
                    }
                    isOverlayInitialized = true;
                }
                else
                {
                    Debug.LogError("OpenVR.Overlay is null");
                    return;
                }
            }

            // Get the background texture
            Texture loadingTexture = ImGuiLoadingScreen.backgroundTexture;
            if (loadingTexture == null)
            {
                Debug.LogError("Loading texture is null");
                return;
            }

            // Create Texture handle
            var tex = new Texture_t
            {
                handle = loadingTexture.GetNativeTexturePtr(),
                eColorSpace = EColorSpace.Auto
            };

            // Set the overlay texture
            var overlayInterface = OpenVR.Overlay;
            EVROverlayError texError = overlayInterface.SetOverlayTexture(overlayHandle, ref tex);
            if (texError != EVROverlayError.None)
            {
                Debug.LogError("Failed to set overlay texture: " + texError.ToString());
                return;
            }

            float overlayWidthInMeters = 10.0f; // Set the overlay width (increase to make the image larger)
            overlayInterface.SetOverlayWidthInMeters(overlayHandle, overlayWidthInMeters);

            // Flip the texture vertically by adjusting the texture bounds
            VRTextureBounds_t textureBounds = new VRTextureBounds_t
            {
                uMin = 0f,
                vMin = 1f,
                uMax = 1f,
                vMax = 0f
            };

            overlayInterface.SetOverlayTextureBounds(overlayHandle, ref textureBounds);

            // Position the overlay closer to the player
            var transform = new SteamVR_Utils.RigidTransform(
                Vector3.forward * 3f, // Position in front of the player
                Quaternion.identity      // No rotation
            );
            HmdMatrix34_t hmdMatrix = transform.ToHmdMatrix34();

            overlayInterface.SetOverlayTransformAbsolute(
                overlayHandle,
                ETrackingUniverseOrigin.TrackingUniverseStanding,
                ref hmdMatrix
            );

            overlayInterface.SetOverlayCurvature(overlayHandle, 0.2f);

            // Show the overlay
            overlayInterface.ShowOverlay(overlayHandle);
        }

        public static void HideLoadingScreenInVR()
        {
            if (overlayHandle != 0)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    overlay.HideOverlay(overlayHandle);
                    overlay.DestroyOverlay(overlayHandle);
                    overlayHandle = 0;
                    isOverlayInitialized = false;
                }
            }
        }
    }
}
