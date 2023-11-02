using System.Collections.Generic;
using UnityEngine;

namespace StationeersVR.Utilities
{
    static class CameraUtils
    {
        //TODO: Need to check all the camera names and delete the ones that's not needed.
        public const string MAIN_CAMERA = "Main Camera"; // This is the camera used in the loading screen and main menu. Possibly used somewhere else.
        public const string VR_CAMERA = "VRCamera"; // This is the new camera created by this mod
        public const string VR_UI_CAMERA = "VRUICamera";
        public const string HANDS_CAMERA = "VRHandsCamera";
        public const string SKYBOX_CAMERA = "SkyboxCamera";
        public const string VRSKYBOX_CAMERA = "VRSkyboxCamera";
        public const string VRGUI_SCREENSPACE_CAM = "VRGuiScreenSpace";
        public const string WORLD_SPACE_UI_CAMERA = "WorldSpaceUiCamera";

        private static Camera _worldSpaceUiCamera;
        private static Dictionary<string, Camera> _cameraCache = new();
        private static int worldSpaceUiDepth = 2;

        public static void CopyCamera(Camera from, Camera to)
        {
            if (from == null)
            {
                ModLog.Error("\"from\" camera is null!");
                return;
            }
            if (to == null)
            {
                ModLog.Error("\"to\" camera is null!");
                return;
            }
            to.farClipPlane = from.farClipPlane;
            to.clearFlags = from.clearFlags;
            to.renderingPath = from.renderingPath;
            to.clearStencilAfterLightingPass = from.clearStencilAfterLightingPass;
            to.depthTextureMode = from.depthTextureMode;
            to.layerCullDistances = from.layerCullDistances;
            to.layerCullSpherical = from.layerCullSpherical;
            to.cullingMask = from.cullingMask;
            to.useOcclusionCulling = from.useOcclusionCulling;
            to.allowHDR = false; // Force this to off for VR
            to.backgroundColor = from.backgroundColor;
        }

        public static Camera GetWorldspaceUiCamera()
        {
            if (_worldSpaceUiCamera != null)
            {
                return _worldSpaceUiCamera;
            }
            Camera vrCam = GetCamera(VR_CAMERA);
            if (vrCam == null || vrCam.gameObject == null)
            {
                ModLog.Info("VR Camera is null while creating world space UI camera.");
                return null;
            }
            GameObject worldSpaceUiCamParent = new(WORLD_SPACE_UI_CAMERA);
            worldSpaceUiCamParent.transform.SetParent(vrCam.transform);
            _worldSpaceUiCamera = worldSpaceUiCamParent.AddComponent<Camera>();
            _worldSpaceUiCamera.CopyFrom(vrCam);
            _worldSpaceUiCamera.clearFlags = CameraClearFlags.Depth;
            _worldSpaceUiCamera.depth = worldSpaceUiDepth;
            _worldSpaceUiCamera.renderingPath = RenderingPath.Forward;
            _worldSpaceUiCamera.cullingMask = LayerUtils.WORLDSPACE_UI_LAYER_MASK;
            _worldSpaceUiCamera.enabled = true;
            return _worldSpaceUiCamera;
        }

        public static Camera GetCamera(string name)
        {
            //Check cache
            if(_cameraCache.ContainsKey(name) && _cameraCache[name] != null) return _cameraCache[name];

            //Update cache
            foreach (var c in GameObject.FindObjectsOfType<Camera>())
            {
                if (c.name == name)
                {
                    _cameraCache.Remove(name);
                    _cameraCache.Add(name, c);
                    return c;
                }
            }

            return null;
        }


        public static void PrintCamera(Camera c)
        {
            if (c == null)
            {
                ModLog.Info("Null camera, cannot print properties!");
                return;
            }
            ModLog.Debug("Camera: " + c.name);
            ModLog.Debug("  activeTexture: " + c.activeTexture);
            ModLog.Debug("  actualRenderingPath: " + c.actualRenderingPath);
            ModLog.Debug("  allowDynamicResolution: " + c.allowDynamicResolution);
            ModLog.Debug("  allowHDR: " + c.allowHDR);
            ModLog.Debug("  allowMSAA: " + c.allowMSAA);
            ModLog.Debug("  areVRStereoViewMatricesWithinSingleCullTolerance: " + c.areVRStereoViewMatricesWithinSingleCullTolerance);
            ModLog.Debug("  aspect: " + c.aspect);
            ModLog.Debug("  backgroundColor: " + c.backgroundColor);
            ModLog.Debug("  cameraToWorldMatrix: " + c.cameraToWorldMatrix);
            ModLog.Debug("  cameraType: " + c.cameraType);
            ModLog.Debug("  clearFlags: " + c.clearFlags);
            ModLog.Debug("  clearStencilAfterLightingPass: " + c.clearStencilAfterLightingPass);
            ModLog.Debug("  commandBufferCount: " + c.commandBufferCount);
            ModLog.Debug("  cullingMask: " + c.cullingMask);
            ModLog.Debug("  cullingMatrix: " + c.cullingMatrix);
            ModLog.Debug("  depth: " + c.depth);
            ModLog.Debug("  depthTextureMode: " + c.depthTextureMode);
            ModLog.Debug("  eventMask: " + c.eventMask);
            ModLog.Debug("  farClipPlane: " + c.farClipPlane);
            ModLog.Debug("  fieldOfView: " + c.fieldOfView);
            ModLog.Debug("  focalLength: " + c.focalLength);
            ModLog.Debug("  forceIntoRenderTexture: " + c.forceIntoRenderTexture);
            ModLog.Debug("  getFit: " + c.gateFit);
            ModLog.Debug("  layerCullDistances: " + c.layerCullDistances);
            ModLog.Debug("  layerCullSpherical: " + c.layerCullSpherical);
            ModLog.Debug("  lensShift: " + c.lensShift);
            ModLog.Debug("  nearClipPlane: " + c.nearClipPlane);
            ModLog.Debug("  nonJitteredProjectionMatrix: " + c.nonJitteredProjectionMatrix);
            ModLog.Debug("  opaqueSortMode: " + c.opaqueSortMode);
            ModLog.Debug("  orthographic: " + c.orthographic);
            ModLog.Debug("  orthographicSize: " + c.orthographicSize);
            ModLog.Debug("  orverrideSceneCullingMask: " + c.overrideSceneCullingMask);
            ModLog.Debug("  pixelHeight: " + c.pixelHeight);
            ModLog.Debug("  pixelRect: " + c.pixelRect);
            ModLog.Debug("  pixelWidth: " + c.pixelWidth);
            ModLog.Debug("  previousViewProjectionMatrix: " + c.previousViewProjectionMatrix);
            ModLog.Debug("  projectionMatrix: " + c.projectionMatrix);
            ModLog.Debug("  rect: " + c.rect);
            ModLog.Debug("  renderingPath: " + c.renderingPath);
            ModLog.Debug("  scaledPixelHeight: " + c.scaledPixelHeight);
            ModLog.Debug("  scaledPixelWidth: " + c.scaledPixelWidth);
            ModLog.Debug("  scene: " + c.scene);
            ModLog.Debug("  sensorSize: " + c.sensorSize);
            ModLog.Debug("  stereoActiveEye: " + c.stereoActiveEye);
            ModLog.Debug("  stereoConvergence: " + c.stereoConvergence);
            ModLog.Debug("  stereoEnabled: " + c.stereoEnabled);
            ModLog.Debug("  stereoSeparation: " + c.stereoSeparation);
            ModLog.Debug("  stereoTargetEye: " + c.stereoTargetEye);
            ModLog.Debug("  targetDisplay: " + c.targetDisplay);
            ModLog.Debug("  targetTexture: " + c.targetTexture);
            ModLog.Debug("  transparencySortAxis: " + c.transparencySortAxis);
            ModLog.Debug("  transparencySortMode: " + c.transparencySortMode);
            ModLog.Debug("  useJitteredProjectionMatrixForTransparentRendering: "
                + c.useJitteredProjectionMatrixForTransparentRendering);
            ModLog.Debug("  useOcclusionCulling: " + c.useOcclusionCulling);
            ModLog.Debug("  usePhysicalProperties: " + c.usePhysicalProperties);
            ModLog.Debug("  velocity: " + c.velocity);
            ModLog.Debug("  worldToCameraMatrix: " + c.worldToCameraMatrix);
            var skybox = c.GetComponent<Skybox>();
            if (skybox != null)
            {
                ModLog.Debug("Skybox : " + skybox.name + "  Material: " + skybox.material);
            }
        }

    }
}
