using UnityEngine;
using Valve.VR;

namespace StationeersVR.Utilities
{
    static class VROverlay
    {
        private static ulong overlayHandle = 0;
        private static bool isOverlayInitialized = false;
        public static void ShowLoadingScreenInVR(string overlayName, Texture loadingTexture, float overlayWidthInMeters, float overlayDistance, float overlayCurvature)
        {
            if (overlayHandle != 0)
            {
                Debug.LogError("Failed to create overlay: " + overlayName + " There's already an active LoadingScreen overlay.");
                return;
            }
            if (!isOverlayInitialized)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    EVROverlayError error = overlay.CreateOverlay("StationeersVR." + overlayName, "Loading Screen", ref overlayHandle);
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
                Vector3.forward * overlayDistance, // Position in front of the player
                Quaternion.identity      // No rotation
            );
            HmdMatrix34_t hmdMatrix = transform.ToHmdMatrix34();

            overlayInterface.SetOverlayTransformAbsolute(
                overlayHandle,
                ETrackingUniverseOrigin.TrackingUniverseStanding,
                ref hmdMatrix
            );

            overlayInterface.SetOverlayCurvature(overlayHandle, overlayCurvature);

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
