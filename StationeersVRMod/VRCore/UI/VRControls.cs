using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using StationeersVR.Utilities;
using System.Linq;
using UnityEngine.EventSystems;

namespace StationeersVR.VRCore.UI
{
    class VRControls : MonoBehaviour
    {

        public static bool vrcontrols_initialized = false;
        public static bool useContinuousTurn;
        // Time in seconds that Recenter pose must be held to recenter
        private static readonly float RECENTER_POSE_TIME = 3f;
        // Local Position relative to HMD that will trigger the Recenter action
        private static readonly Vector3 RECENTER_POSE_POSITION_L = new Vector3(-0.1f, 0f, 0.1f);
        private static readonly Vector3 RECENTER_POSE_POSITION_R = new Vector3(0.1f, 0f, 0.1f);
        // Tolerance for above pose
        private static readonly float RECENTER_POSE_TOLERANCE = 0.2f; // Magnitude

        private HashSet<KeyCode> ignoredKeys = new HashSet<KeyCode>();
        private HashSet<KeyCode> quickActionEnabled = new HashSet<KeyCode>(); // never ignore these
        private SteamVR_ActionSet mainActionSet = SteamVR_Actions.Stationeers;
        private SteamVR_ActionSet laserActionSet = SteamVR_Actions.LaserPointers;

        // Since some controllers have most actions on trackpads (Vive Wands),
        // SteamVR as of 22/09/2021 will still disable all the actions bound to 
        // a lower priority action set even if they are of a completely different type
        // This means that we have to duplicate actions in some actionsets so keyToBooleanAction
        // should map to an array that is the conjunction of the same action in different actionsets
        private Dictionary<KeyCode, SteamVR_Action_Boolean[]> keyToBooleanAction = new Dictionary<KeyCode, SteamVR_Action_Boolean[]>();

        private SteamVR_Action_Vector2 walk;
        private SteamVR_Action_Vector2 pitchAndYaw;
        private SteamVR_Action_Vector2 buildPitchAndYaw; //for the same logic as keyToBooleanAction, this is needed for controllers that have multiple actionsets using the trackpad
        private float combinedPitchAndYawX => buildPitchAndYaw.active ? buildPitchAndYaw.axis.x : pitchAndYaw.axis.x;

        private SteamVR_Action_Vector2 contextScroll;

        public SteamVR_Action_Pose poseL;
        public SteamVR_Action_Pose poseR;

        // Action for "Use" using the left hand controller
        private SteamVR_Action_Boolean _useLeftHand = SteamVR_Actions.stationeers_UseLeft;

        // An input where the user holds down the button when clicking for an alternate behavior (ie, stack split)
        private SteamVR_Action_Boolean _clickModifier = SteamVR_Actions.laserPointers_ClickModifier;

        public SteamVR_Action_Boolean useLeftHandAction
        {
            get
            {
                return _useLeftHand;
            }
        }

        private float recenteringPoseDuration;

        public static bool mainControlsActive
        {
            get
            {
                return _instance != null && _instance.mainActionSet.IsActive();
            }
        }

        public static bool laserControlsActive
        {
            get
            {
                return _instance != null && _instance.laserActionSet.IsActive();
            }
        }

        public static VRControls instance { get { return _instance; } }
        private static VRControls _instance;
        void Awake()
        {
            if (ConfigFile.UseVrControls)
                _instance = this;
        }

        void Update()
        {
            if (!ConfigFile.UseVrControls)
            {
                return;
            }
            updateMainActionSetState();
            updateLasersActionSetState();
                       if (mainActionSet.IsActive())
                       {
                           checkRecenterPose(Time.unscaledDeltaTime);
                       }           
        }


        /*       
                private void checkQuickItems<T>(GameObject obj, SteamVR_Action_Boolean action, bool useRightClick) where T : QuickAbstract {

                    if (!obj) {
                        return;
                    }

                    // Due to complicated bindings/limited inputs, the QuickSwitch and Right click are sharing a button
                    // and when the hammer is equipped, the bindings conflict... so we'll share the right click button
                    // here to activate quick switch. This is hacky because rebinding things can break the controls, but
                    // it works and allows users to use the quick select while the hammer is equipped.
                    if (StaticObjects.rightHandQuickMenu != null)
                    {
                        StaticObjects.rightHandQuickMenu.GetComponent<RightHandQuickMenu>().refreshItems();
                        StaticObjects.leftHandQuickMenu.GetComponent<LeftHandQuickMenu>().refreshItems();
                    }
                    bool rightClickDown = false;
                    bool rightClickUp = false;
                    if (useRightClick && laserControlsActive && inPlaceMode())
                    {
                        rightClickDown = SteamVR_Actions.laserPointers_RightClick.GetState(SteamVR_Input_Sources.Any);
                        rightClickUp = SteamVR_Actions.laserPointers_RightClick.GetStateUp(SteamVR_Input_Sources.Any);
                        if(rightClickDown)
                            buildQuickActionTimer += Time.unscaledDeltaTime;
                    }

                    if (action.GetStateDown(SteamVR_Input_Sources.Any) || rightClickDown) {
                        if (inPlaceMode())
                        {
                            if (SteamVR_Actions.valheim_Grab.GetState(SteamVR_Input_Sources.RightHand))
                            {
                                buildQuickActionTimer = 1;
                            }
                            if ((buildQuickActionTimer >= 0.3f || !useRightClick))
                                obj.SetActive(true);
                        }
                        else
                            obj.SetActive(true);
                    }

                    if (action.GetStateUp(SteamVR_Input_Sources.Any) || rightClickUp) {
                        if (inPlaceMode() && (buildQuickActionTimer >= 0.3f || !useRightClick))
                            obj.GetComponent<T>().selectHoveredItem();
                        else if(!inPlaceMode())
                            obj.GetComponent<T>().selectHoveredItem();

                        if (useRightClick)
                            buildQuickActionTimer = 0;
                        obj.SetActive(false);
                    }
                }
        */
        
               private void checkRecenterPose(float dt)
               {
                   if (vrcontrols_initialized && isInRecenterPose())
                   {
                       recenteringPoseDuration += dt;
                       if (recenteringPoseDuration >= RECENTER_POSE_TIME)
                       {
                           ModLog.Debug("Triggered Recenter pose action.");
                           VRManager.TryRecenter();
                           recenteringPoseDuration = 0f;
                       }
                   } else
                   {
                       recenteringPoseDuration = 0f;
                   }
               }
        
        private bool isInRecenterPose()
        {
            var hmd = VRPlayer.instance.GetComponent<Valve.VR.InteractionSystem.Player>().hmdTransform;
            var targetLocationLeft = hmd.localPosition + hmd.localRotation * RECENTER_POSE_POSITION_L;
            var targetLocationRight = hmd.localPosition + hmd.localRotation * RECENTER_POSE_POSITION_R;
            var leftHand = poseL.localPosition;
            var rightHand = poseR.localPosition;
            var leftHandDiff = leftHand - targetLocationLeft;
            var rightHandDiff = rightHand - targetLocationRight;
            return leftHandDiff.magnitude <= RECENTER_POSE_TOLERANCE && rightHandDiff.magnitude <= RECENTER_POSE_TOLERANCE;
        }

        private void updateMainActionSetState()
        {
            if (!ConfigFile.UseVrControls && mainActionSet.IsActive())
            {
                mainActionSet.Deactivate();
            }
            else if (ConfigFile.UseVrControls && !mainActionSet.IsActive())
            {
                mainActionSet.Activate();
            }
        }

        private void updateLasersActionSetState()
        {
            if (!mainActionSet.IsActive())
            {
                laserActionSet.Deactivate();
                return;
            }
            if (laserActionSet.IsActive() && VRPlayer.activePointer == null)
            {
                laserActionSet.Deactivate();
            }
            else if (!laserActionSet.IsActive() && VRPlayer.activePointer != null)
            {
                laserActionSet.Activate(SteamVR_Input_Sources.Any, 1 /* Higher priority than main action set */);
            }
        }

        public bool GetButtonDown(KeyCode key)
        {
            if (!mainActionSet.IsActive() || ignoredKeys.Contains(key))
            {
                return false;
            }
            /*          //Here we add in what situations the character should be blocked from jumping or other actions
                        if (key == "Jump" && (shouldEnableRemove() || shouldDisableJumpRemove()))
                        {
                            return false;
                        }
                        if (key == "Remove" && (!shouldEnableRemove() || shouldDisableJumpRemove()))
                        {
                            return false;
                        }
            */
            SteamVR_Action_Boolean[] action;
            keyToBooleanAction.TryGetValue(key, out action);
            if (action == null)
            {
                if (!quickActionEnabled.Contains(key))
                {
                    ModLog.Warning("Unmapped Key:" + key);
                    ignoredKeys.Add(key); // Don't check for this input again
                }
                return false;
            }
            return action.Any(x => x.GetStateDown(SteamVR_Input_Sources.Any));
        }

        public bool GetButton(KeyCode key)
        {
            if (!mainActionSet.IsActive() || ignoredKeys.Contains(key))
            {
                return false;
            }
            /*
                        if (key == "Jump" && (shouldEnableRemove() || shouldDisableJumpRemove()))
                        {
                            return false;
                        }
            */
            SteamVR_Action_Boolean[] action;
            keyToBooleanAction.TryGetValue(key, out action);
            if (action == null)
            {
                if (!quickActionEnabled.Contains(key))
                {
                    ModLog.Warning("Unmapped key Key:" + key);
                    ignoredKeys.Add(key); // Don't check for this input again
                }
                return false;
            }
            return action.Any(x => x.GetState(SteamVR_Input_Sources.Any));
        }
        /* 
                private bool CheckAltButton()
                {
                    //If both triggers are pressed during this check, the alternate action is enabled
                    return (SteamVR_Actions.valheim_Use.GetState(SteamVR_Input_Sources.Any) && SteamVR_Actions.valheim_UseLeft.GetState(SteamVR_Input_Sources.Any))
                        || (SteamVR_Actions.valheim_Grab.GetState(SteamVR_Input_Sources.RightHand));
                }
        */
        public bool GetButtonUp(KeyCode key)
        {
            if (!mainActionSet.IsActive() || ignoredKeys.Contains(key))
            {
                return false;
            }
            /*
                        if (key == "Jump" && (shouldEnableRemove() || shouldDisableJumpRemove()))
                        {
                            return false;
                        }
                        if (key == "Remove" && (!shouldEnableRemove() || shouldDisableJumpRemove()))
                        {
                            return false;
                        }
                        if (key == "Jump" && shouldDisableJumpEvade())
                        {
                            return false;
                        }
            */
            SteamVR_Action_Boolean[] action;
            keyToBooleanAction.TryGetValue(key, out action);
            if (action == null)
            {
                if (!quickActionEnabled.Contains(key))
                {
                    ModLog.Warning("Unmapped key Key:" + key);
                    ignoredKeys.Add(key); // Don't check for this input again
                }
                return false;
            }
            return action.Any(x => x.GetStateUp(SteamVR_Input_Sources.Any));
        }

        public float GetJoyLeftStickX()
        {
            if (!mainActionSet.IsActive())
            {
                return 0.0f;
            }
            return walk.axis.x;
        }

        public float GetJoyLeftStickY()
        {
            if (!mainActionSet.IsActive())
            {
                return 0.0f;
            }
            return walk.axis.y;
        }

        public float GetJoyRightStickX()
        {
            // Disable rotation if "altPieceRotationControlsActive" is true
            if (!mainActionSet.IsActive() || altPieceRotationControlsActive())
            {
                return 0.0f;
            }
            return combinedPitchAndYawX;
        }

        public float GetJoyRightStickY()
        {
            // Even though Y axis is not used for piece rotation with alternative
            // controls, disable it to avoid situations where the player is angling
            // the joystick up/down while trying to rotate causing unintended actions
            if (!mainActionSet.IsActive() || altPieceRotationControlsActive())
            {
                return 0.0f;
            }
            return pitchAndYaw.axis.y;
        }
        // This is used in Valheim to rotate the building pieces. Later we should make something similar to cycle/rotate stuff when building
        public int getDirectPieceRotation()
        {
            if (!altPieceRotationControlsActive())
            {
                return 999;
            }
            if (-pitchAndYaw.axis.y > 0.5f)
            {
                //return BuildingManager.instance.TranslateRotation() + 8;
            }
            else if (-pitchAndYaw.axis.y < -0.5f)
            {
                //return BuildingManager.instance.TranslateRotation();
            }
            return 999;
        }

        public int getDirectRightYAxis()
        {
            float yAxis = -pitchAndYaw.axis.y;
            if (yAxis > 0.5f)
            {
                return -1;
            }
            else if (yAxis < -0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public int getDirectRightXAxis()
        {
            float xAxis = -pitchAndYaw.axis.x;
            if (xAxis > 0.5f)
            {
                return -1;
            }
            else if (xAxis < -0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /*       public int getPieceRotation()
               {
                   if (altPieceRotationControlsActive())
                   {
                       return getAltPieceRotation();
                   }
                   return 0;
                   //context scrolling backup in case needed
                   //if (!contextScroll.activeBinding)
                   //{
                   //    // Since we don't have a context scroll bound (becaus of limited input
                   //    // options), we need to control rotation using the right joystick
                   //    // when a special button is held - we are using the Map button for this purpose.
                   //    // As a result, when in "build mode", the map button is disabled for the purpose
                   //    // of bringing up the map and when the player is holding down the map button,
                   //    // then they cannot rotate their character.
                   //    if (altPieceRotationControlsActive())
                   //    {
                   //        return getAltPieceRotation();
                   //    } else
                   //    {
                   //        return 0;
                   //    }
                   //}
                   //if (contextScroll.axis.y > 0)
                   //{
                   //    return 1;
                   //} else if (contextScroll.axis.y < 0)
                   //{
                   //    return -1;
                   //} else
                   //{
                   //    return 0;
                   //}
               }
        */
        /*
        public bool getClickModifier()
        {
            // TODO: update _clickModifier in the action set to use grab buttons. It is obsoletely bound to left controller trigger now and cannot be used here.
            if (VRPlayer.leftPointer.isActive && SteamVR_Actions.stationeers_Grab.GetState(SteamVR_Input_Sources.LeftHand))
            {
                return true;
            }
            if (VRPlayer.rightPointer.isActive && SteamVR_Actions.stationeers_Grab.GetState(SteamVR_Input_Sources.RightHand))
            {
                return true;
            }
            return _clickModifier.GetState(SteamVR_Input_Sources.Any);
        }*/

        /*       private int getAltPieceRotation()
               {
                   if (!altPieceTriggered)
                   {
                       return 0;
                   }
                   altPieceTriggered = false;
                   float rightStickXAxis = combinedPitchAndYawX;
                   if (rightStickXAxis > 0.1f)
                   {
                       return -1;
                   }
                   else if (rightStickXAxis < -0.1f)
                   {
                       return 1;
                   }
                   else
                   {
                       return 0;
                   }
               }
        */
        /*
               public int getPieceRefModifier()
               {
                   float yAxis = GetJoyRightStickY();
                   if(yAxis > 0.5f)
                   {
                       return -1;
                   } else if (yAxis < -0.5f)
                   {
                       return 1;
                   }
                   else
                   {
                       return 0;
                   }
               }
        */
        /*
               private bool inPlaceMode()
               {
                   return Player.m_localPlayer != null && Player.m_localPlayer.InPlaceMode();
               }

               private bool hasPlacementGhost()
               {
                   if (Player.m_localPlayer == null)
                   {
                       return false;
                   }
                   var ghost = Player.m_localPlayer.m_placementGhost;
                   return ghost != null && ghost.activeSelf;
               }

               private bool hasHoverObject()
               {
                   if (Player.m_localPlayer == null)
                   {
                       return false;
                   }
                   return Player.m_localPlayer.m_hovering != null;
               }
       */
        // Used to determine when the player is in a mode where the right joystick should
        // be used for rotation of an object while building rather than rotating the
        // player character
        // disable context scrolling for now 
        private bool altPieceRotationControlsActive()
        {
            //return inPlaceMode() && !Hud.IsPieceSelectionVisible() && SteamVR_Actions.valheim_Grab.GetState(SteamVR_Input_Sources.RightHand);
            //Later we need to make this work, for example, to place cables or pipes
            return false;
        }

        // disable Jump input under certain conditions
        // * In placement mode
        // * Grab Modifier is Pressed

        /*        private bool shouldEnableRemove()
                {
                    return inPlaceMode() && SteamVR_Actions.valheim_Grab.GetState(SteamVR_Input_Sources.RightHand);
                }

                private bool shouldDisableJumpRemove()
                {
                    return BuildingManager.instance && (BuildingManager.instance.isCurrentlyMoving() || BuildingManager.instance.isCurrentlyPreciseMoving() || BuildingManager.instance.isHoldingPlace());
                }

                private bool shouldDisableJumpEvade()
                {
                    return SteamVR_Actions.valheim_UseLeft.state;
                }
        */

        public void init()
        {
            if (!vrcontrols_initialized)
            {
                SteamVR_Actions.Stationeers.Activate(SteamVR_Input_Sources.Any, 0, true);
                SteamVR_Actions.LaserPointers.Activate();

                keyToBooleanAction.Add(KeyMap.PrimaryAction, new[] { SteamVR_Actions.stationeers_UseRight, SteamVR_Actions.laserPointers_RightClick });
                keyToBooleanAction.Add(KeyMap.SecondaryAction, new[] { SteamVR_Actions.stationeers_Grab });
                keyToBooleanAction.Add(KeyMap.Jetpack, new[] { SteamVR_Actions.stationeers_Jetpack });
                keyToBooleanAction.Add(KeyMap.Ascend, new[] { SteamVR_Actions.stationeers_Ascend });
                keyToBooleanAction.Add(KeyMap.Descend, new[] { SteamVR_Actions.stationeers_Descend });
                keyToBooleanAction.Add(KeyMap.Cancel, new[] { SteamVR_Actions.stationeers_ToggleMenu });
                keyToBooleanAction.Add(KeyMap.MouseControl, new[] { SteamVR_Actions.stationeers_MouseControl });
                

                // Print all the added keys/Actions
                foreach (var entry in keyToBooleanAction)
                {
                    KeyCode keyvalue = entry.Key;
                    SteamVR_Action_Boolean[] actions = entry.Value;
                    string actionsDescriptions = actions != null ? string.Join(", ", actions.Select(a => a.ToString())) : "null";

                    ModLog.Debug($"KeyCode: {keyvalue} -> Actions: [{actionsDescriptions}]");
                }

                contextScroll = SteamVR_Actions.stationeers_ContextScroll;

                walk = SteamVR_Actions.stationeers_Walk;
                pitchAndYaw = SteamVR_Actions.stationeers_PitchAndYaw;
                buildPitchAndYaw = SteamVR_Actions.laserPointers_PitchAndYaw;
                poseL = SteamVR_Actions.stationeers_PoseL;
                poseR = SteamVR_Actions.stationeers_PoseR;
                initignoredKeys();
                //initQuickActionOnly();
                recenteringPoseDuration = 0f;
                vrcontrols_initialized = true;
                ModLog.Debug("VRControls Initialized");
            }
        }

        //private void initQuickActionOnly()
        //{
        //    quickActionEnabled.Add(KeyMap.PrecisionPlace);
        //}

        private void initignoredKeys()
        {
            ignoredKeys.Add(KeyMap.Forward);
            ignoredKeys.Add(KeyMap.Backward);
            ignoredKeys.Add(KeyMap.Left);
            ignoredKeys.Add(KeyMap.Right);
            ignoredKeys.Add(KeyMap.HelmetSlot);               // Inventory UI Rework  (pose to add/remove it)
            ignoredKeys.Add(KeyMap.GlassesSlot);              // Inventory UI Rework  (pose to turn it on/off)
            ignoredKeys.Add(KeyMap.SuitSlot);                 // Inventory UI Rework  (pose to wear/remove it
            ignoredKeys.Add(KeyMap.BackSlot);                 // Inventory UI Rework  
            ignoredKeys.Add(KeyMap.UniformSlot);              // Inventory UI Rework  
            ignoredKeys.Add(KeyMap.ToolBeltSlot);             // Inventory UI Rework       
            ignoredKeys.Add(KeyMap.Grab);                     // not needed
            ignoredKeys.Add(KeyMap.ShowScoreBoard);           // Keyboard
            ignoredKeys.Add(KeyMap.ShowDynamicPanel);         // Keyboard
            ignoredKeys.Add(KeyMap.PreviousItem);             // Keyboard
            ignoredKeys.Add(KeyMap.NextItem);                 // Keyboard
            ignoredKeys.Add(KeyMap.SpawnItem);                // Keyboard
            ignoredKeys.Add(KeyMap.ToggleUi);                 // Keyboard
            ignoredKeys.Add(KeyMap.ToggleConsole);            // Keyboard 
            ignoredKeys.Add(KeyMap.ToggleInfo);               // ?
            ignoredKeys.Add(KeyMap.ScreenShot);               // not needed
            ignoredKeys.Add(KeyMap.Internals);                // not needed
            ignoredKeys.Add(KeyMap.ToggleHandPower);          // not needed
            ignoredKeys.Add(KeyMap.Chatting);                 // not needed (for now)
            ignoredKeys.Add(KeyMap.PrecisionPlace);           // not needed
            ignoredKeys.Add(KeyMap.InstantStop);              // not needed
            ignoredKeys.Add(KeyMap.Teleport);                 // ?
            ignoredKeys.Add(KeyMap.FoVUp);                    // will be added in mod config
            ignoredKeys.Add(KeyMap.FoVDown);                  // will be added in mod config
            ignoredKeys.Add(KeyMap.EmoteWave);                // not needed
            ignoredKeys.Add(KeyMap.ThirdPersonControl);       // not needed
            ignoredKeys.Add(KeyMap.HideAllWindows);           // not needed
            ignoredKeys.Add(KeyMap.SuitPressureIncrease);     // control tablet
            ignoredKeys.Add(KeyMap.SuitPressureIncrease);     // control tablet
            ignoredKeys.Add(KeyMap.SuitTemperatureIncrease);  // control tablet
            ignoredKeys.Add(KeyMap.SuitTemperatureDecrease);  // control tablet
            ignoredKeys.Add(KeyMap.JetpackThrustIncrease);    // control tablet
            ignoredKeys.Add(KeyMap.JetpackThrustDecrease);    // control tablet
            ignoredKeys.Add(KeyMap.FovReset);                 // not needed
            ignoredKeys.Add(KeyMap.SmartTool);                // ?
            ignoredKeys.Add(KeyMap.PingHighlight);            // not needed
        }

    }
}