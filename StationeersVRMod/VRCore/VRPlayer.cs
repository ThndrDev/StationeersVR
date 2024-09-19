using BepInEx;
using static StationeersVR.Utilities.VRAssetManager;
using StationeersVR.Utilities;
//using AmplifyOcclusion;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
//using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;
//using UnityStandardAssets.ImageEffects;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using Assets.Scripts.UI;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts;
using static Assets.Scripts.MovementController;
using Assets.Scripts.Inventory;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections;
using UnityEngine.UI;
using StationeersVR.Patches;
using ch.sycoforge.Flares;
using Util;
using Assets.Scripts.Util;
using SimpleSpritePacker;
using UnityEngine.TextCore.Text;
using Assets.Scripts.GridSystem;
using UnityEngine.EventSystems;
using StationeersVR.VRCore.UI;



/**
 * VRPlayer manages instantiating the SteamVR Player
 * prefab as well as controlling it's world position and rotation.
 * 
 * Also manages enabling and disabling the hand laser pointer
 * based on what hands are active and what the user sets as
 * their preferred hand.
 */


namespace StationeersVR.VRCore
{
    class VRPlayer : MonoBehaviour
    {

        private enum HeadZoomLevel
        {
            FirstPerson,
            ThirdPerson0,
            ThirdPerson1,
            ThirdPerson2,
            ThirdPerson3
        }



        private static readonly string PLAYER_PREFAB_NAME = "StationeersVRPlayer";
        private static readonly string START_SCENE = "Splash";
        private static readonly string CHARACTER_CUSTOMISATION = "CharacterCustomisation";
        public static readonly string RIGHT_HAND = "RightHand";
        public static readonly string LEFT_HAND = "LeftHand";
        // This layer must be set in the hand model prefab in the
        // Unity AssetBundle project too. If they don't match,
        // the hands won't be rendered by the handsCam.
        private static Vector3 FIRST_PERSON_OFFSET = Vector3.zero;
        private static float SIT_HEIGHT_ADJUST = -0.7f;
        private static float SIT_ATTACH_HEIGHT_ADJUST = -0.4f;
        private static Vector3 THIRD_PERSON_0_OFFSET = new(0f, 1.0f, -0.6f);
        private static Vector3 THIRD_PERSON_1_OFFSET = new(0f, 1.4f, -1.5f);
        private static Vector3 THIRD_PERSON_2_OFFSET = new(0f, 1.9f, -2.6f);
        private static Vector3 THIRD_PERSON_3_OFFSET = new(0f, 3.2f, -4.4f);
        private static Vector3 THIRD_PERSON_CONFIG_OFFSET = Vector3.zero;
        private static float NECK_OFFSET = 0.0f;
        public const float ROOMSCALE_STEP_ANIMATION_SMOOTHING = 0.3f;
        public const float ROOMSCALE_ANIMATION_WEIGHT = 2f;

        public static VRIK vrikRef { get { return _vrik; } }
        private static VRIK _vrik;

        private static float referencePlayerHeight;

        private static GameObject _prefab;
        private static Shader _fade_shader;
        private static GameObject _instance;
        private static VRPlayer _vrPlayerInstance;
        private static HeadZoomLevel _headZoomLevel = HeadZoomLevel.FirstPerson;

        private Camera _vrCam;
        private Camera _handsCam;
        private Camera _foregroundCam;
        private Camera _backgroundCam;

        //Roomscale movement variables
        public Transform _vrCameraRig;
        private Vector3 _lastCamPosition = Vector3.zero;
        private Vector3 _lastPlayerPosition = Vector3.zero;
        private Vector3 _lastPlayerAttachmentPosition = Vector3.zero;

        private float _forwardSmoothVel = 0.0f, _sideSmoothVel = 0.0f;
        private static float _roomscaleAnimationForwardSpeed = 0.0f;
        private static float _roomscaleAnimationSideSpeed = 0.0f;
        public static float roomscaleAnimationForwardSpeed { get { return _roomscaleAnimationForwardSpeed; } }
        public static float roomscaleAnimationSideSpeed { get { return _roomscaleAnimationSideSpeed; } }
        public static Vector3 roomscaleMovement { get; private set; }

        private static Hand _leftHand;
        private static SteamVR_LaserPointer _leftPointer;
        private static Hand _rightHand;
        private static SteamVR_LaserPointer _rightPointer;
        private string _preferredHand;

        private bool pausedMovement = false;

        private float timerLeft;
        private float timerRight;
        public static Hand leftHand { get { return _leftHand; } }
        public static Hand rightHand { get { return _rightHand; } }

        public static Hand dominantHand { get { return ConfigFile.LeftHanded ? leftHand : rightHand; } }
        //public static bool ShouldPauseMovement { get { return PlayerCustomizaton.IsBarberGuiVisible() || (Menu.IsVisible() && !ConfigFile.AllowMovementWhenInMenu()); } }
        public static bool ShouldPauseMovement { get { return IsPaused(); } }
        public static bool IsPaused() { return GameManager.GameState == Assets.Scripts.GridSystem.GameState.Paused;}
        private bool turnModeSet = false;
        public static bool IsClickableGuiOpen
        {
            get {
                // TODO: Need to find all clickable menu or interfaces opened and add it here. 
                InventoryManager inventoryManagerInstance = InventoryManager.Instance;
                //ModLog.Debug("IsClickableGuiOpen: ConsoleWindow.IsOpen: " + ConsoleWindow.IsOpen + " InGameMenuOpen: " +  inventoryManagerInstance.InGameMenuOpen);
                return ConsoleWindow.IsOpen || inventoryManagerInstance.InGameMenuOpen;

                    /* Examples from Valheim:
                     * 
                     * Hud.IsPieceSelectionVisible() ||
                    StoreGui.IsVisible() ||
                    InventoryGui.IsVisible() ||
                    Menu.IsVisible() ||
                    (TextViewer.instance && TextViewer.instance.IsVisible()) ||
                    Minimap.IsOpen();*/
            }
        }
        /*
                public static PhysicsEstimator leftHandPhysicsEstimator
                {
                    get
                    {
                        PhysicsEstimator value = leftHand.gameObject.GetComponent<PhysicsEstimator>();
                        if (value == null && attachedToPlayer)
                        {
                            value = leftHand.gameObject.AddComponent<PhysicsEstimator>();
                            value.refTransform = CameraUtils.getCamera(CameraUtils.VR_CAMERA)?.transform.parent;
                        }
                        return value;
                    }
                }

                public static PhysicsEstimator rightHandPhysicsEstimator
                {
                    get
                    {
                        PhysicsEstimator value = rightHand.gameObject.GetComponent<PhysicsEstimator>();
                        if (value == null && attachedToPlayer)
                        {
                            value = rightHand.gameObject.AddComponent<PhysicsEstimator>();
                            value.refTransform = CameraUtils.getCamera(CameraUtils.VR_CAMERA)?.transform.parent;
                        }
                        return value;
                    }
                }*/

        public static SteamVR_Input_Sources dominantHandInputSource { get { return ConfigFile.LeftHanded ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand; } }
        public static SteamVR_Input_Sources nonDominantHandInputSource { get { return ConfigFile.LeftHanded ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand; } }

        public static bool handsActive
        {
            get
            {
                return handIsActive(_leftHand, _leftPointer) && handIsActive(_rightHand, _rightPointer);
            }
        }

        public static SteamVR_LaserPointer leftPointer { get { return _leftPointer;} }
        public static SteamVR_LaserPointer rightPointer { get { return _rightPointer; } }
        public static SteamVR_LaserPointer activePointer
        {
            get
            {
                if (leftPointer != null && leftPointer.active)
                {
                    return leftPointer;
                }
                else if (rightPointer != null && rightPointer.active)
                {
                    return rightPointer;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool inFirstPerson { get
            {
                return (_headZoomLevel == HeadZoomLevel.FirstPerson) && attachedToPlayer;
            }
        }
/*      // TODO: Check if it's needed in Stationeers
        public static bool isMoving
        {
            get
            {
                if (Player.ClientData != "client" && attachedToPlayer)
                {
                    //Vector3 relativeVelocity = Player.m_localPlayer.GetVelocity();
                    Vector3 relativeVelocity = Player.controllingBody.velocity;
                    if (Player.m_localPlayer.m_lastGroundBody)
                        relativeVelocity -= Player.m_localPlayer.m_lastGroundBody.velocity;
                    return relativeVelocity.magnitude > 0.5f;
                }
                return false;
            }
        }
*/
        public static GameObject instance { get { return _instance; } }
        public static VRPlayer vrPlayerInstance => _vrPlayerInstance;
        public static bool attachedToPlayer = false;

        private static float FIRST_PERSON_HEIGHT_OFFSET = 0.0f;
        private static bool _headPositionInitialized = false;
        public static bool headPositionInitialized
        {
            get
            {
                return _headPositionInitialized;
            }
            set
            {
                _headPositionInitialized = value;
                if (!_headPositionInitialized)
                {
                    FIRST_PERSON_HEIGHT_OFFSET = 0.0f;
                    FIRST_PERSON_OFFSET = Vector3.zero;
                }
            }
        }

        void Awake()
        {
            _vrPlayerInstance = this;
            _prefab = VRAssetManager.GetAsset<GameObject>(PLAYER_PREFAB_NAME);
            //For some reason the fadeMaterial shade from SteamVR_Fade needs to be manually loaded to work:
            if (!SteamVR_Fade.fadeMaterial)
            {
                _fade_shader = VRAssetManager.GetAsset<Shader>("Custom/SteamVR_Fade");
                SteamVR_Fade.fadeMaterial = new Material(_fade_shader);
            }

            _preferredHand = ConfigFile.GetDominantHand();            
            headPositionInitialized = false;
            FIRST_PERSON_OFFSET = Vector3.zero;
            //THIRD_PERSON_CONFIG_OFFSET = ConfigFile.GetThirdPersonHeadOffset();
            ensurePlayerInstance();
            if (ConfigFile.UseVrControls)
            {
                gameObject.AddComponent<VRControls>();
            }               
        }

        void Update()
        {
            if (!ensurePlayerInstance())
            {
                return;
            }
            maybeUpdateHeadPosition();
            attachVrPlayerToWorldObject();
            enableCameras();
            checkAndSetHandsAndPointers();
            updateVrik();
            UpdateHud();
            if (Input.GetKey(KeyCode.Y))
            {
                ModLog.Debug("Triggered Recenter pose action.");
                VRManager.TryRecenter();
            }

            //UpdateAmplifyOcclusionStatus();


            if (timerLeft > 0)
            {
                timerLeft -= Time.deltaTime;
                leftHand.hapticAction.Execute(0f, 0.1f, 20f, 0.1f, SteamVR_Input_Sources.LeftHand);
            }
            if (timerRight > 0)
            {
                timerRight -= Time.deltaTime;
                rightHand.hapticAction.Execute(0f, 0.1f, 20f, 0.1f, SteamVR_Input_Sources.RightHand);
            }
        }

        public void UpdateHud()
        {
            List<GameObject> canvas = new List<GameObject>();
            GameObject gCanvas = GameObject.Find("GameCanvas");
            GameObject aCanvas = GameObject.Find("AlertCanvas");
            GameObject cCanvas = GameObject.Find("CursorCanvas");
            GameObject sCanvas = GameObject.Find("SystemCanvas");
            GameObject fCanvas = GameObject.Find("FadeCanvas");
            GameObject puCanvas = GameObject.Find("PopupsCanvas");
            //GameObject pCanvas = GameObject.Find("PingCanvas");
            GameObject phCanvas = GameObject.Find("PanelHelpMenu");
            GameObject pwCanvas = GameObject.Find("PanelInWorldToolTip");
            //GameObject piCanvas = GameObject.Find("PanelInternal");
            //GameObject pdCanvas = GameObject.Find("PanelDynamicThing");
            //GameObject imgui = GameObject.Find("ImGUI");
            //GameObject popupCanvas = GameObject.Find("PopupsCanvas");
            //GameObject tooltipCanvas = GameObject.Find("TooltipCanvas");
            //GameObject valucomp = GameObject.Find("ValueCompass");
            if (canvas.Count == 0)
            {
                if(gCanvas != null)
                    canvas.Add(gCanvas);

                if (aCanvas != null)
                    canvas.Add(aCanvas);

                if (cCanvas != null)
                    canvas.Add(cCanvas);

                if (sCanvas != null)
                    canvas.Add(sCanvas);

                if (fCanvas != null) 
                    canvas.Add(fCanvas);

                if (puCanvas != null)
                    canvas.Add(puCanvas);

               /* if (pCanvas != null)
                    canvas.Add(pCanvas);*/

                if (phCanvas != null)
                    canvas.Add(phCanvas);

                if (pwCanvas != null)
                    canvas.Add(pwCanvas);

                /*if (piCanvas != null)
                    canvas.Add(piCanvas);

                if (pdCanvas != null)
                    canvas.Add(pdCanvas);

                if (imgui != null)
                {
                    canvas.Add(imgui);
                    ModLog.Error("Imgui: " + imgui.name);
                }

                if (popupCanvas != null)
                    canvas.Add(popupCanvas);*/

                //if (tooltipCanvas != null)
                 //   canvas.Add(tooltipCanvas);
                /*

                if (valucomp != null)
                    canvas.Add(valucomp);*/
            }
                    
            if (canvas.Count > 0)
            {
                foreach (var can in canvas)
                {
                    if (can != null)
                    {
                        if (can.GetComponent<Canvas>() != null && Camera.current != null)
                        {
                            setCameraHudPosition(can.GetComponent<Canvas>());
                        }
                    }
                }
            }
        }

        private void setCameraHudPosition(Canvas canvas)
        {
            float scaleFactor = 3.5f / Camera.current.pixelWidth / 2;
            float scaleFactor1 = 2.0f / Camera.current.pixelWidth / 2;
            float hudDistance = 2;
            float hudVerticalOffset = +1.0f;
            float hudHorizontalOffset = 1.0f;
            canvas.renderMode = UnityEngine.RenderMode.WorldSpace;
            if (canvas.name == "AlertCanvas")
            {
                //ModLog.Error("RenderMode: " + canvas.renderMode);
                canvas.transform.SetParent(Camera.current.transform, false);
                canvas.worldCamera = Camera.current;
                canvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor1 * hudDistance * 1;
                canvas.transform.position = new Vector3(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f,hudDistance);
                canvas.transform.localPosition = new Vector3(0 + hudHorizontalOffset, 0 , hudDistance);
            }
            else
            {
                canvas.gameObject.transform.SetParent(Camera.current.transform, false);
                canvas.gameObject.transform.position = new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f);

                canvas.transform.localPosition = new Vector3(0, 0 + hudVerticalOffset, hudDistance);
                canvas.GetComponent<RectTransform>().localScale = Vector3.one * scaleFactor * hudDistance * 1;
                canvas.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        /*
                private void FixedUpdate() 
                {
                    if (ShouldPauseMovement)
                    {
                        if (vrikEnabled() && !pausedMovement)
                        {
                            VrikCreator.PauseLocalPlayerVrik();
                            pausedMovement = true;
                        }
                    }
                    else
                    {
                        if (vrikEnabled() && pausedMovement)
                        {
                            // Before unpausing, move the camera back to the position before the pause to prevent teleporting the player to the cuurent camera position.
                            _vrCameraRig.localPosition -= Vector3.ProjectOnPlane(_vrCam.transform.localPosition - _lastCamPosition, Vector3.up);
                            _lastCamPosition = _vrCam.transform.localPosition;
                            VrikCreator.UnpauseLocalPlayerVrik();
                            pausedMovement = false;
                        }
                        if (inFirstPerson)
                        {
                            DoRoomScaleMovement();
                        }
                        else
                        {
                            roomscaleMovement = Vector3.zero;
                        }
                    }
                }
        */

        // Fixes an issue on Pimax HMDs that causes rotation to be incorrect:
        // See: https://www.reddit.com/r/Pimax/comments/qhkrfp/pimax_unity_xr_plugin_issue/
        private static void UpdateTrackedPoseDriverPoseSource()
        {
            var hmd = Valve.VR.InteractionSystem.Player.instance.hmdTransform;
            var trackedPoseDriver = hmd.gameObject.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                ModLog.Info("Null TrackedPoseDriver on HMD transform.");
            }
            else
            {
                ModLog.Info("Setting TrackedPoseDriver.poseSource to Head.");
                trackedPoseDriver.SetPoseSource(trackedPoseDriver.deviceType, TrackedPoseDriver.TrackedPose.Head);
            }
        }


        void maybeUpdateHeadPosition()
        {
            //if (ConfigFile.AllowHeadRepositioning())
            //{
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    updateHeadOffset(new Vector3(0f, 0f, 0.1f));
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    updateHeadOffset(new Vector3(0f, 0f, -0.1f));
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    updateHeadOffset(new Vector3(-0.1f, 0f, 0f));
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    updateHeadOffset(new Vector3(0.1f, 0f, 0f));
                }
                if (Input.GetKeyDown(KeyCode.RightBracket))
                {
                    updateHeadOffset(new Vector3(0f, 0.1f, 0f));
                }
                if (Input.GetKeyDown(KeyCode.LeftBracket))
                {
                    updateHeadOffset(new Vector3(0f, -0.1f, 0f));
                }
            //}
        }


        private void updateHeadOffset(Vector3 offset)
        {
            if (!attachedToPlayer)
            {
                return;
            }
            if (inFirstPerson)
            {
                FIRST_PERSON_OFFSET += offset;
            } else
            {
                THIRD_PERSON_CONFIG_OFFSET += offset;
                //ConfigFile.UpdateThirdPersonHeadOffset(THIRD_PERSON_CONFIG_OFFSET);
            }
        }

/*
        void UpdateAmplifyOcclusionStatus()
        {
            if (_vrCam == null || _vrCam.gameObject.GetComponent<AmplifyOcclusionEffect>() == null)
            {
                return;
            }
            var effect = _vrCam.gameObject.GetComponent<AmplifyOcclusionEffect>();
            effect.SampleCount = SampleCountLevel.Medium;
            effect.enabled = ConfigFile.UseAmplifyOcclusion();
        }
*/
        private void checkAndSetHandsAndPointers()
        {
            tryInitializeHands();
            if (_leftHand != null)
            {
                //_leftHand.enabled = ConfigFile.UseVrControls();
                _leftHand.enabled = true;
                _leftHand.SetVisibility(_leftHand.enabled && !vrikEnabled());
            }
            if (_rightHand != null)
            {
                //_rightHand.enabled = ConfigFile.UseVrControls();
                _rightHand.enabled = true;
                _rightHand.SetVisibility(_rightHand.enabled&& !vrikEnabled()) ;
            }
            // Next check whether the hands are active, and enable the appropriate pointer based
            // on what is available and what the options set as preferred. Disable the inactive pointer(s).
            if (handIsActive(_leftHand, _leftPointer) && handIsActive(_rightHand, _rightPointer))
            {
                // Both hands active, so choose preferred hand
                if (_preferredHand == LEFT_HAND)
                {
                    setPointerActive(_leftPointer, true);
                    setPointerActive(_rightPointer, false);
                } else
                {
                    setPointerActive(_rightPointer, true);
                    setPointerActive(_leftPointer, false);
                }
            } else if (handIsActive(_rightHand, _rightPointer))
            {
                setPointerActive(_rightPointer, true);
                setPointerActive(_leftPointer, false);
            } else if (handIsActive(_leftHand, _leftPointer))
            {
                setPointerActive(_leftPointer, true);
                setPointerActive(_rightPointer, false);
            } else
            {
                setPointerActive(_leftPointer, false);
                setPointerActive(_rightPointer, false);
            }
        }

        private void tryInitializeHands()
        {
            // First try and initialize both hands and pointer scripts
            if (_leftHand == null || _leftPointer == null)
            {
                //ModLog.Debug("LeftHand or LeftPointer is null, trying to initialize ");
                _leftHand = getHand(LEFT_HAND, _instance);
                if (_leftHand != null)
                {
                    _leftPointer = _leftHand.GetComponent<SteamVR_LaserPointer>();
                    if (_leftPointer != null)
                    {
                        //_leftPointer.raycastLayerMask = LayerUtils.UI_PANEL_LAYER_MASK;
                    }
                }
            }
            if (_rightHand == null || _rightPointer == null)
            {
                //ModLog.Debug("RighttHand or RightPointer is null, trying to initialize ");
                _rightHand = getHand(RIGHT_HAND, _instance);
                if (_rightHand != null)
                {
                    _rightPointer = _rightHand.GetComponent<SteamVR_LaserPointer>();
                    if (_rightPointer != null)
                    {
                        //_rightPointer.raycastLayerMask = LayerUtils.UI_PANEL_LAYER_MASK;
                    }
                }
            }
        }

        // Sets the given pointer active if "active" parameter is true
        // and laser pointers should currently be active.
        private void setPointerActive(SteamVR_LaserPointer p, bool active)
        {
            if (p == null)
            {
                return;
            }
            p.active = active; //&& shouldLaserPointersBeActive();
            //p.setVisible(p.pointerIsActive() && Cursor.visible);
        }
/*
        private bool shouldLaserPointersBeActive()
        {
            bool isInPlaceMode = (getPlayerCharacter() != null) && getPlayerCharacter().InPlaceMode();
            return ConfigFile.UseVrControls() && (Cursor.visible || isInPlaceMode);
        }
*/

        // Returns true if both the hand and pointer are not null
        // and the hand is active
        private static bool handIsActive(Hand h, SteamVR_LaserPointer p)
        {
            if (h == null || p == null)
            {
                return false;
            }
            return h.enabled && h.isActive && h.isPoseValid;
        }

        private Hand getHand(string hand, GameObject playerInstance)
        {
            foreach (Hand h in playerInstance.GetComponentsInChildren<Hand>())
            {
                if (h.gameObject.name == hand)
                {
                    return h;
                }
            }
            return null;
        }

        private bool ensurePlayerInstance()
        {
            if (_instance == null)
            {
                // Need to create an instance of the Player prefab
                if (_prefab == null)
                {
                    ModLog.Error("SteamVR Player Prefab is not loaded!");
                    return false;
                }
                _instance = Instantiate(_prefab);
                // Rigid bodies built into the SteamVR Player prefab will
                // cause problems and we don't actually need them for anything,
                // so disable all of them.
                if (_instance != null)
                {
                    DisableRigidBodies(_instance);
                    UpdateTrackedPoseDriverPoseSource();
                }
            }
            return _instance != null;
        }

        private void enablebackgroundCamera()
        {
            if (!(GameObject.Find("BackgroundCamera") != null))
            {
                return;
            }
            Camera originalbackgroundCamera = GameObject.Find("BackgroundCamera").GetComponent<Camera>();
            if (!(originalbackgroundCamera == null) && !(originalbackgroundCamera.gameObject == null))
            {
                Camera vrCam = CameraUtils.GetCamera("VRCamera");
                if (!(vrCam == null) && !(vrCam.gameObject == null))
                {
                    GameObject vrbackground = new GameObject("VRBackgroundCamera");
                    Camera vrBackgroundCam = vrbackground.AddComponent<Camera>();
                    vrBackgroundCam.CopyFrom(originalbackgroundCamera);
                    vrBackgroundCam.depth = -2f;
                    vrBackgroundCam.transform.SetParent(vrCam.transform);
                    originalbackgroundCamera.enabled = false;
                    vrBackgroundCam.enabled = true;
                    _backgroundCam = vrBackgroundCam;
                    ModLog.Error("_backgroundCam: " + _backgroundCam);
                }
            }
        }

        private void enableforegroundCamera()
        {
            if (!(GameObject.Find("ForegroundCamera") != null))
            {
                return;
            }
            Camera originalforegroundCamera = GameObject.Find("ForegroundCamera").GetComponent<Camera>();
            if (!(originalforegroundCamera == null) && !(originalforegroundCamera.gameObject == null))
            {
                Camera vrCam = CameraUtils.GetCamera("VRCamera");
                if (!(vrCam == null) && !(vrCam.gameObject == null))
                {
                    GameObject vrforeground = new GameObject("VRForegroundCamera");
                    Camera vrForegroundCam = vrforeground.AddComponent<Camera>();
                    vrForegroundCam.CopyFrom(originalforegroundCamera);
                    vrForegroundCam.depth = -2f;
                    vrForegroundCam.transform.SetParent(vrCam.transform);
                    originalforegroundCamera.enabled = false;
                    vrForegroundCam.enabled = true;
                    _foregroundCam = vrForegroundCam;
                    ModLog.Error("_foregroundCam: " + _foregroundCam);
                }
            }
        }

        private void enableCameras()
        {
            if (_vrCam == null || !_vrCam.enabled)
            {
                enableVrCamera();
            } else
            {
                _vrCam.nearClipPlane = ConfigFile.nearClipPlane;
            }
            if (_handsCam == null || !_handsCam.enabled)
            {
                enableHandsCamera();
            }
            if (_backgroundCam == null || !_backgroundCam.enabled)
            {
                enablebackgroundCamera();
        }
            if (_foregroundCam == null || !_foregroundCam.enabled)
            {
                enableforegroundCamera();
            }
        }

        private void enableVrCamera()
        {
            if (_instance == null)
            {
                ModLog.Error("Cannot enable VR Camera with null SteamVR Player instance.");
                return;
            }
            Camera mainCamera = CameraUtils.GetCamera(CameraUtils.MAIN_CAMERA);
            if (mainCamera == null)
            {
                ModLog.Error("Main Camera is null.");
                return;
            }
            Camera vrCam = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
            vrCam.GetOrAddComponent<FlareReceiver>().enabled = true;
            CameraUtils.CopyCamera(mainCamera, vrCam);
            //maybeCopyPostProcessingEffects(vrCam, mainCamera);
            //maybeAddAmplifyOcclusion(vrCam);
            // Prevent visibility of the head
            vrCam.nearClipPlane = ConfigFile.nearClipPlane;
            // Turn off rendering the UI panel layer. We need to capture
            // it in a camera of higher depth so that it
            // is rendered on top of everything else. (except hands)
//            vrCam.cullingMask &= ~(1 << LayerUtils.getUiPanelLayer());
//            vrCam.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
//            vrCam.cullingMask &= ~(1 << LayerUtils.getHandsLayer());
//            vrCam.cullingMask &= ~(1 << LayerUtils.getWorldspaceUiLayer());
            mainCamera.enabled = false;
            AudioListener mainCamListener = mainCamera.GetComponent<AudioListener>();
            if (mainCamListener != null)
            {
                ModLog.Debug("Destroying MainCamera AudioListener");
                DestroyImmediate(mainCamListener);
            }
            //Add fade component to camera for transition handling
//            _fadeManager = vrCam.gameObject.AddComponent<FadeToBlackManager>();
            _instance.SetActive(true);
            vrCam.enabled = true;
            //ModLog.Error("Scale: " + Human.LocalHuman.transform.localScale);
            vrCam.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            _vrCam = vrCam;
            _vrCameraRig = vrCam.transform.parent;
/*
            _fadeManager.OnFadeToWorld += () => {
                //Recenter
                VRPlayer.headPositionInitialized = false;
                VRPlayer.vrPlayerInstance?.ResetRoomscaleCamera();
            };
*/
        }

        // Create a camera and assign its culling mask
        // to the unused layer. Assign depth to be higher
        // than the UI panel depth to ensure they are drawn
        // on top of the GUI.
        private void enableHandsCamera()
        {
            Camera vrCam = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
            if (vrCam == null || vrCam.gameObject == null)
            {
                return;
            }
            ModLog.Debug("Enabling Hands Camera");
            GameObject handsCameraObject = new(CameraUtils.HANDS_CAMERA);
            Camera handsCamera = handsCameraObject.AddComponent<Camera>();
            handsCamera.CopyFrom(CameraUtils.GetCamera(CameraUtils.VR_CAMERA));
            handsCamera.depth = 4;
            handsCamera.clearFlags = CameraClearFlags.Depth;
            handsCamera.cullingMask = LayerUtils.HANDS_LAYER_MASK;
            handsCamera.transform.SetParent(vrCam.transform);
            handsCamera.enabled = true;
            _handsCam = handsCamera;
        }

        // Search for the original skybox cam, if found, copy it, disable it,
        // and make new camera child of VR camera
        // TODO: Not yet tested/enabled for VR.
/*
        private void enableSkyboxCamera()
        {
            Camera originalSkyboxCamera = CameraUtils.GetCamera(CameraUtils.SKYBOX_CAMERA);
            if (originalSkyboxCamera == null || originalSkyboxCamera.gameObject == null)
            {
                return;
            }
            Camera vrCam = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
            if(vrCam == null || vrCam.gameObject == null)
            {
                return;
            }
            GameObject vrSkyboxCamObj = new(CameraUtils.VRSKYBOX_CAMERA);
            Camera vrSkyboxCam = vrSkyboxCamObj.AddComponent<Camera>();
            vrSkyboxCam.CopyFrom(originalSkyboxCamera);
            vrSkyboxCam.depth = -2;
            vrSkyboxCam.transform.SetParent(vrCam.transform);
            originalSkyboxCamera.enabled = false;
            vrSkyboxCam.enabled = true;
            //_skyboxCam = vrSkyboxCam;
        }
*/

        private void attachVrPlayerToWorldObject()
        {
            if (shouldAttachToPlayerCharacter())
            {
                updateZoomLevel();
                attachVrPlayerToPlayerCharacter();
            }
            else
            {
                attachVrPlayerToMainCamera();
            }
        }

        private void updateZoomLevel()
        {
            if (!canAdjustCameraDistance())
            {
                return;
            }
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll > 0f)
            {
                zoomCamera(true);
            } else if (mouseScroll < 0f)
            {
                zoomCamera(false);
            }
        }

        //TODO: For now, the mod only works in single player. Need to work on this for 3rd person view
        private void zoomCamera(bool zoomIn)
        {
            switch(_headZoomLevel)
            {
                case HeadZoomLevel.FirstPerson:
                    _headZoomLevel = zoomIn ? HeadZoomLevel.FirstPerson : HeadZoomLevel.ThirdPerson0;
                    break;
                case HeadZoomLevel.ThirdPerson0:
                    _headZoomLevel = zoomIn ? HeadZoomLevel.FirstPerson : HeadZoomLevel.ThirdPerson1;
                    break;
                case HeadZoomLevel.ThirdPerson1:
                    _headZoomLevel = zoomIn ? HeadZoomLevel.ThirdPerson0 : HeadZoomLevel.ThirdPerson2;
                    break;
                case HeadZoomLevel.ThirdPerson2:
                    _headZoomLevel = zoomIn ? HeadZoomLevel.ThirdPerson1 : HeadZoomLevel.ThirdPerson3;
                    break;
                case HeadZoomLevel.ThirdPerson3:
                    _headZoomLevel = zoomIn ? HeadZoomLevel.ThirdPerson2 : HeadZoomLevel.ThirdPerson3;
                    break;
            }
        }

        //TODO: For now, the mod only works in single player. Need to work on this for 3rd person view
        private static Vector3 getHeadOffset(HeadZoomLevel headZoomLevel)
        {
            switch (headZoomLevel)
            {
                case HeadZoomLevel.FirstPerson:
                    return FIRST_PERSON_OFFSET;
                case HeadZoomLevel.ThirdPerson0:
                    return THIRD_PERSON_0_OFFSET + THIRD_PERSON_CONFIG_OFFSET;
                case HeadZoomLevel.ThirdPerson1:
                    return THIRD_PERSON_1_OFFSET + THIRD_PERSON_CONFIG_OFFSET;
                case HeadZoomLevel.ThirdPerson2:
                    return THIRD_PERSON_2_OFFSET + THIRD_PERSON_CONFIG_OFFSET;
                case HeadZoomLevel.ThirdPerson3:
                    return THIRD_PERSON_3_OFFSET + THIRD_PERSON_CONFIG_OFFSET;
                default:
                    return FIRST_PERSON_OFFSET;
            }
        }

        // Some logic from GameCamera class
        private bool canAdjustCameraDistance()
        {
            //ModLog.Debug("canAdjustCameraDistance: IsClickableGuiOpen: " + IsClickableGuiOpen + " getPlayerCharacter().ChatPanel.isActiveAndEnabled: " + getPlayerCharacter().ChatPanel.isActiveAndEnabled + " attachedToPlayer: " + attachedToPlayer);
            return !IsClickableGuiOpen && 
                   !getPlayerCharacter().ChatPanel.isActiveAndEnabled &&
                   attachedToPlayer;
            /* Valheim examples:
             * return !IsClickableGuiOpen &&
                (!Chat.instance || !Chat.instance.HasFocus()) &&
                !Console.IsVisible() &&
                attachedToPlayer &&
                !getPlayerCharacter().InCutscene() &&
                !getPlayerCharacter().InPlaceMode();*/
        }

        private bool shouldAttachToPlayerCharacter()
        {
            return getPlayerCharacter() != null &&
                   SceneManager.GetActiveScene().name != START_SCENE &&
                   SceneManager.GetActiveScene().name != CHARACTER_CUSTOMISATION &&
                   ensurePlayerInstance() &&
                   !getPlayerCharacter().Dead;
                    //TODO: Add programming in computer, in stationpedia

 /*          Valheim examples:
  *             return getPlayerCharacter() != null &&
                SceneManager.GetActiveScene().name != START_SCENE &&
                ensurePlayerInstance() &&
                !getPlayerCharacter().InCutscene() &&
                !getPlayerCharacter().IsDead() &&
                !getPlayerCharacter().InBed() &&
                !PlayerCustomizaton.IsBarberGuiVisible();
 */
         }

        private void attachVrPlayerToPlayerCharacter()
        {
            Human playerCharacter = getPlayerCharacter();
            if (playerCharacter == null)
            {
                ModLog.Error("Player character is null. Cannot attach!");
                return;
            }
            if (!ensurePlayerInstance())
            {
                ModLog.Error("SteamVR Player instance is null. Cannot attach!");
                return;
            }
            _instance.transform.SetParent(playerCharacter.transform, false);
            attachedToPlayer = true;
            //ModLog.Debug("Player character and SteamVR instance found! Attaching to player.");

            maybeInitHeadPosition(playerCharacter);
            float firstPersonAdjust = inFirstPerson ? FIRST_PERSON_HEIGHT_OFFSET : 0.0f;
            setHeadVisibility(!inFirstPerson);
            // Update the position with the first person adjustment calculated in init phase
            Vector3 desiredPosition = getDesiredPosition(playerCharacter);
            
            _instance.transform.localPosition = desiredPosition - playerCharacter.transform.position  // Base Positioning
                                               + Vector3.up * getHeadHeightAdjust(playerCharacter)
                                               + Vector3.up * firstPersonAdjust; // Offset from calibration on tracking recenter
                                               
            if(_headZoomLevel != HeadZoomLevel.FirstPerson)
            {
                _instance.transform.localPosition += getHeadOffset(_headZoomLevel) // Player controlled offset (zeroed on tracking reset)
                            + Vector3.forward * NECK_OFFSET; // Move slightly forward to position on neck
                setPlayerVisualsOffset(playerCharacter.transform, Vector3.zero);
            }
            else
                setPlayerVisualsOffset(playerCharacter.transform,
                                -getHeadOffset(_headZoomLevel) // Player controlled offset (zeroed on tracking reset)
                                -Vector3.forward * NECK_OFFSET // Move slightly forward to position on neck
            );
            if (!turnModeSet)
            {
                setPlayerTurnMode();
            }
        }

        //Choose between characterContinuousTurn or SnapTurn
        public void setPlayerTurnMode()
        {            
            if (!ConfigFile.UseSnapTurn)
            {
                ModLog.Debug("Continuous turn mode Enabled");
                //VRControls.useContinuousTurn = true;
            }
            else
            {
                // Find the StationeersVRPlayer(Clone) GameObject in the scene.
                GameObject vrPlayerClone = GameObject.Find("StationeersVRPlayer(Clone)");
                if (vrPlayerClone != null)
                {
                    // Find the SnapTurn component in the children of the StationeersVRPlayer(Clone) GameObject.
                    SnapTurn snapTurnComponent = vrPlayerClone.GetComponentInChildren<SnapTurn>();
                    if (snapTurnComponent != null)
                    {
                        // Enable the SnapTurn component.
                        snapTurnComponent.enabled = true;
                        ModLog.Info("SnapTurn mode Enabled");
                    }
                    else
                    {
                        Debug.LogError("SnapTurn component not found on StationeersVRPlayer.");
                    }
                }
                else
                {
                    Debug.LogError("StationeersVRPlayer GameObject not found in the scene.");
                }
                //VRControls.useContinuousTurn = false;
            }
            turnModeSet = true;
        }

        //Moves all the effects and the meshes that compose the player, doesn't move the Rigidbody
        private void setPlayerVisualsOffset(Transform playerTransform, Vector3 offset)
        {
            for(int i = 0; i < playerTransform.childCount; i++)
            {
                Transform child = playerTransform.GetChild(i);
                if(child == _instance.transform || child.name == "EyePos") continue;
                playerTransform.GetChild(i).localPosition = offset;                          
            }
        }

        private float getHeadHeightAdjust(Human player)
        {
            if (player.MovementController.ControlMode == Mode.Seated)            
            {
                return SIT_HEIGHT_ADJUST;
                /*if (player.IsAttached())
                {
                    return SIT_ATTACH_HEIGHT_ADJUST;
                }
                else
                {
 
                }*/
            }
            return 0f;
        }


        private void updateVrik()
        {
            var player = getPlayerCharacter();
            if (player == null)
            {
                return;
            }
            maybeAddVrik(player);
            if (_vrik != null) {
                _vrik.enabled = ConfigFile.UseVrControls &&
                    inFirstPerson &&
                    /*!player.InDodge() &&
                    !player.IsStaggering() &&
                    !player.IsSleeping() &&*/
                    validVrikAnimatorState(player.GetComponentInChildren<Animator>());
                //LeftHandQuickMenu.instance.UpdateWristBar();
                //RightHandQuickMenu.instance.UpdateWristBar();
            }
        }


        private bool validVrikAnimatorState(Animator animator)
        {
            if (animator == null)
            {
                return false;
            }
            return !animator.GetBool("wakeup");
        }


        private void maybeAddVrik(Human player)
        {
            if (!ConfigFile.UseVrControls || player.gameObject.GetComponent<VRIK>() != null)
            {
                return;
            }
            var cam = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
            if (ConfigFile.UseVrControls)
            {
                _vrik = VrikCreator.initialize(player.gameObject, leftHand.transform, rightHand.transform, cam.transform);
            }
            /*var vrPlayerSync = player.gameObject.GetComponent<VRPlayerSync>();
            vrPlayerSync.camera = cam.gameObject;
            vrPlayerSync.leftHand = _vrik.solver.leftArm.target.parent.gameObject;
            vrPlayerSync.rightHand = _vrik.solver.rightArm.target.parent.gameObject;*/
            VrikCreator.resetVrikHandTransform(player);
            /*_vrik.references.leftHand.gameObject.AddComponent<HandGesture>().sourceHand = leftHand;
            _vrik.references.rightHand.gameObject.AddComponent<HandGesture>().sourceHand = rightHand;
            StaticObjects.leftFist().setColliderParent(_vrik.references.leftHand, false);
            StaticObjects.rightFist().setColliderParent(_vrik.references.rightHand, true);
            Player.m_localPlayer.gameObject.AddComponent<FistBlock>();
            StaticObjects.mouthCollider(cam.transform);
            StaticObjects.addQuickMenus();
            LeftHandQuickMenu.instance.refreshItems();
            RightHandQuickMenu.instance.refreshItems();*/
        }

        private bool vrikEnabled()
        {
            var player = getPlayerCharacter();
            if (player == null)
            {
                return false;
            }
            var vrik = player.gameObject.GetComponent<VRIK>();
            if (vrik != null && vrik != null)
            {
                return vrik.enabled && !IsPaused();
            }
            return false;
        }

        private void maybeInitHeadPosition(Human playerCharacter)
        {
            if (!headPositionInitialized && inFirstPerson)
            {
                // First set the position without any adjustment
                Vector3 desiredPosition = getDesiredPosition(playerCharacter);
                _instance.transform.localPosition = desiredPosition - playerCharacter.transform.position;

                if(_headZoomLevel != HeadZoomLevel.FirstPerson)
                    _instance.transform.localPosition += getHeadOffset(_headZoomLevel);
                else
                    setPlayerVisualsOffset(playerCharacter.transform, -getHeadOffset(_headZoomLevel));

                var hmd = Valve.VR.InteractionSystem.Player.instance.hmdTransform;
                // Measure the distance between HMD and desires location, and save it.
                FIRST_PERSON_HEIGHT_OFFSET = desiredPosition.y - hmd.position.y;
                if (ConfigFile.UseLookLocomotion)
                {
                    _instance.transform.localRotation = Quaternion.Euler(0f, -hmd.localRotation.eulerAngles.y, 0f);
                }
                headPositionInitialized = true;

                referencePlayerHeight = Valve.VR.InteractionSystem.Player.instance.eyeHeight;
            }
        }

        private static Vector3 getDesiredPosition(Human playerCharacter)
        {
            if (playerCharacter == null)
            {
                return Vector3.zero;
            }
            return new Vector3(playerCharacter.transform.position.x,
                    playerCharacter.CameraController.MainCameraPosition.y - 0.095f, playerCharacter.transform.position.z);
            /*Valheim Example:
             * return new Vector3(playerCharacter.transform.position.x,
                    playerCharacter.GetEyePoint().y, playerCharacter.transform.position.z);*/
        }

        private void attachVrPlayerToMainCamera()
        {
            if (_instance == null)
            {
                ModLog.Error("SteamVR Player instance is null while attaching to main camera!");
                return;
            }
            Camera mainCamera = CameraUtils.GetCamera(CameraUtils.MAIN_CAMERA);
            if (mainCamera == null)
            {
                ModLog.Error("Main camera not found.");
                return;
            }
            setHeadVisibility(true);
            // Orient the player with the main camera
            _instance.transform.parent = mainCamera.gameObject.transform;
            _instance.transform.position = mainCamera.gameObject.transform.position;
            _instance.transform.rotation = mainCamera.gameObject.transform.rotation;
            attachedToPlayer = false;
            headPositionInitialized = false;
        }
        
        // Used to turn off the head model when player is currently occupying it.
        private void setHeadVisibility(bool isVisible)
        {
 /*           if (ConfigFile.UseVrControls) {
                return;
            }
 */           
 /*
            var headBone = getHeadBone();
            if (headBone != null)
            {
                headBone.localScale = isVisible ? new Vector3(1f, 1f, 1f) : new Vector3(0.001f, 0.001f, 0.001f);
            }
 */
        }
/*
        private Transform getHeadBone()
        {
            var playerCharacter = getPlayerCharacter();
            if (playerCharacter == null)
            {
                return null;
            }
            var animator = playerCharacter.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                return null;
            }
            return animator.GetBoneTransform(HumanBodyBones.Head);
        }
*/
        // This will, if it hasn't already done so, try and find the PostProcessingBehaviour
        // script attached to the main camera and copy it over to the VR camera and then
        // add the DepthOfField & CameraEffects components. This enables all the post
        // processing effects to work. Bloom and anti-aliasing have strange artifacts
        // and don't work well. Depth of Field doesn't seem to actually generate depth - others
        // work okay and the world looks much nicer. SSAO is a significant boost in image
        // quality, but it comes at a heavy performance cost.
 /*       private void maybeCopyPostProcessingEffects(Camera vrCamera, Camera mainCamera)
        {
            if (vrCamera == null || mainCamera == null)
            {
                return;
            }
            if (vrCamera.gameObject.GetComponent<PostProcessingBehaviour>() != null)
            {
                return;
            }
            PostProcessingBehaviour postProcessingBehavior = null;
            bool foundMainCameraPostProcesor = false;
            foreach (var ppb in GameObject.FindObjectsOfType<PostProcessingBehaviour>())
            {
                if (ppb.name == CameraUtils.MAIN_CAMERA)
                {
                    foundMainCameraPostProcesor = true;
                    postProcessingBehavior = vrCamera.gameObject.AddComponent<PostProcessingBehaviour>();
                    ModLog.Debug("Copying Main Camera PostProcessingBehaviour");
                    var profileClone = Instantiate(ppb.profile);
                    //Need to copy only the profile and jitterFuncMatrix, everything else will be instanciated when enabled
                    postProcessingBehavior.profile = profileClone;
                    postProcessingBehavior.jitteredMatrixFunc = ppb.jitteredMatrixFunc;
                    if(ppb.enabled) ppb.enabled = false;
                }
            }
            if (!foundMainCameraPostProcesor)
            {
                return;
            }
            var mainCamDepthOfField = mainCamera.gameObject.GetComponent<DepthOfField>();
            var vrCamDepthOfField =  vrCamera.gameObject.AddComponent<DepthOfField>();
            if (mainCamDepthOfField != null)
            {
                CopyClassFields(mainCamDepthOfField, ref vrCamDepthOfField);
            }
            var vrCamSunshaft = vrCamera.gameObject.AddComponent<SunShafts>();
            var mainCamSunshaft = mainCamera.gameObject.GetComponent<SunShafts>();
            if (mainCamSunshaft != null)
            {
                CopyClassFields(mainCamSunshaft, ref vrCamSunshaft);
            }
            var vrCamEffects = vrCamera.gameObject.AddComponent<CameraEffects>();
            var mainCamEffects = mainCamera.gameObject.GetComponent<CameraEffects>();
            if (mainCamEffects != null)
            {
                // Need to copy over only the DOF fields
                vrCamEffects.m_forceDof = mainCamEffects.m_forceDof;
                vrCamEffects.m_dofRayMask = mainCamEffects.m_dofRayMask;
                vrCamEffects.m_dofAutoFocus = mainCamEffects.m_dofAutoFocus;
                vrCamEffects.m_dofMinDistance = mainCamEffects.m_dofMinDistance;
                vrCamEffects.m_dofMaxDistance = mainCamEffects.m_dofMaxDistance;
            }
        }
 */
        private void CopyClassFields<T>(T source, ref T dest)
        {
            FieldInfo[] fieldsToCopy = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fieldsToCopy)
            {
                var value = field.GetValue(source);
                field.SetValue(dest, value);
            }
        }

        private Human getPlayerCharacter()
        {
            return Human.LocalHuman;
        }

        private static void DisableRigidBodies(GameObject root)
        {
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>())
            {
                rb.gameObject.SetActive(false);
            }
            foreach (var sc in root.GetComponentsInChildren<SphereCollider>())
            {
                sc.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Moves the physics player to the head position and cancels the movement of the VRCamera by moving the VRRig
        /// </summary>
 /*       void DoRoomScaleMovement()
        {
            var player = getPlayerCharacter();
            if (_vrCam == null || player == null || player.gameObject == null || player.IsAttached())
            {
              return;
            }
            Vector3 deltaPosition = _vrCam.transform.localPosition - _lastCamPosition;
            deltaPosition.y = 0;
            bool shouldMove = deltaPosition.magnitude > 0.005f;
            if(shouldMove)
            {
                //Check for motion discrepancies
                if(ConfigFile.RoomscaleFadeToBlack() && !_fadeManager.IsFadingToBlack)
                {
                    var lastDeltaMovement = player.m_body.position - _lastPlayerPosition;
                    if(player.m_lastAttachBody && _lastPlayerAttachmentPosition != Vector3.zero)
                    {
                        //Account for ships, and moving attachments
                        lastDeltaMovement -= (player.m_lastAttachBody.position - _lastPlayerAttachmentPosition);
                    }
                    lastDeltaMovement.y = 0;

                    if(roomscaleMovement.magnitude * 0.6f > lastDeltaMovement.magnitude)
                    {
                        SteamVR_Fade.Start(Color.black, 0);
                        SteamVR_Fade.Start(Color.clear, 1.5f); 
                    }

                    _lastPlayerPosition = player.m_body.position;
                    _lastPlayerAttachmentPosition = player.m_lastAttachBody ? player.m_lastAttachBody.position : Vector3.zero;
                }

                //Calculate new postion
                _lastCamPosition = _vrCam.transform.localPosition;
                var globalDeltaPosition = _instance.transform.TransformVector(deltaPosition);
                globalDeltaPosition.y = 0;
                roomscaleMovement = globalDeltaPosition;
                _vrCameraRig.localPosition -= deltaPosition; // Since we move the VR camera rig with the player character elsewhere, we counteract that here to prevent it from moving.
            } else roomscaleMovement = Vector3.zero;

            //Set animation parameters
            _roomscaleAnimationForwardSpeed =  Mathf.SmoothDamp(_roomscaleAnimationForwardSpeed, shouldMove ? deltaPosition.z / Time.fixedDeltaTime : 0, ref _forwardSmoothVel, ROOMSCALE_STEP_ANIMATION_SMOOTHING, 99f);
            _roomscaleAnimationSideSpeed =  Mathf.SmoothDamp(_roomscaleAnimationSideSpeed, shouldMove ? deltaPosition.x / Time.fixedDeltaTime : 0, ref _sideSmoothVel, ROOMSCALE_STEP_ANIMATION_SMOOTHING, 99f);
        }
 */
        public void ResetRoomscaleCamera()
        {
            if(_vrCameraRig != null)
            {
                Vector3 vrCamPosition = _vrCam.transform.localPosition;
                vrCamPosition.y = 0;
                _vrCameraRig.localPosition = -vrCamPosition;
            }
        }

        public void TriggerHandVibration(float time)
        {
            timerLeft = time;
            timerRight = time;
        }
    }
}
