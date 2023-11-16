using Assets.Scripts.Objects.Entities;
using RootMotion.FinalIK;
using UnityEngine;
using Valve.VR.InteractionSystem;
using StationeersVR.VRCore;

namespace StationeersVR.Utilities
{
    public class VrikCreator
    {
        private static readonly Vector3 leftUnequippedPosition = new Vector3(-0.027f, 0.05f, -0.18f);
        private static readonly Quaternion leftUnequippedRotation = new Quaternion(8, 5, -5, 0);
        private static readonly Vector3 leftUnequippedEllbow = new Vector3(1, 0, 0);

        private static readonly Vector3 rightUnequippedPosition = new Vector3(0.027f, 0.05f, -0.18f);
        private static readonly Quaternion rightUnequippedRotation = new Quaternion(35, -13f, -35f, -0.001f);
        private static readonly Vector3 rightUnequippedEllbow = new Vector3(-1, 0, 0);

        private static readonly Vector3 leftEquippedPosition = new Vector3(-0.02f, 0.09f, -0.1f);
        private static readonly Quaternion leftEquippedRotation = Quaternion.Euler(0, 90, -170);
        private static readonly Vector3 leftEquippedEllbow = new Vector3(1, -3f, 0);
        private static readonly Vector3 rightEquippedPosition = new Vector3(0.02f, 0.09f, -0.1f);
        private static readonly Quaternion rightEquippedRotation = Quaternion.Euler(0, -90, -170);
        private static readonly Vector3 rightEquippedEllbow = new Vector3(-1, -3f, 0);

        private static readonly Vector3 leftspearPosition = new Vector3(-0.02f, 0.06f, -0.15f);
        private static readonly Quaternion leftSpearRotation = Quaternion.Euler(0, 90, 140);
        private static readonly Vector3 leftSpearEllbow = new Vector3(1, -3f, 0);
        private static readonly Vector3 rightspearPosition = new Vector3(0.02f, 0.06f, -0.15f);
        private static readonly Quaternion rightSpearRotation = Quaternion.Euler(0, -90, -140);
        private static readonly Vector3 rightSpearEllbow = new Vector3(-1, -3f, 0);

        public static Transform localPlayerRightHandConnector = null;
        public static Transform localPlayerLeftHandConnector = null;
        public static Transform camera;
        private static Transform CameraRig { get { return camera.parent; } }


        private static VRIK CreateTargets(GameObject playerObject)
        {
            VRIK vrik = playerObject.GetComponent<VRIK>() ?? playerObject.AddComponent<VRIK>();
            vrik.solver.leftArm.target = new GameObject().transform;
            vrik.solver.rightArm.target = new GameObject().transform;
            vrik.solver.spine.headTarget = new GameObject().transform;
            localPlayerLeftHandConnector = new GameObject().transform;
            localPlayerRightHandConnector = new GameObject().transform;
            return vrik;
        }

        private static void InitializeTargts(VRIK vrik, Transform leftController, Transform rightController, Transform camera, bool isLocalPlayer)
        {
            vrik.AutoDetectReferences();

            Transform leftHandConnector = isLocalPlayer ? VrikCreator.localPlayerLeftHandConnector : new GameObject().transform;
            leftHandConnector.SetParent(leftController, false);
            vrik.solver.leftArm.target.SetParent(leftHandConnector, false);

            Transform rightHandConnector = isLocalPlayer ? VrikCreator.localPlayerRightHandConnector : new GameObject().transform;
            rightHandConnector.SetParent(rightController, false);
            vrik.solver.rightArm.target.SetParent(rightHandConnector, false);

            Transform head = vrik.solver.spine.headTarget;
            head.SetParent(camera);
            if (isLocalPlayer)
            {
                VrikCreator.camera = camera;
            }
            head.localPosition = new Vector3(0, -0.165f, -0.00f);
            head.localRotation = Quaternion.Euler(180, 80, 90);
            vrik.solver.spine.maxRootAngle = 180;
            //float sizeF = (vrik.solver.spine.headTarget.position.y - vrik.references.root.position.y) / (vrik.references.head.position.y - vrik.references.root.position.y);
            //vrik.references.root.localScale *= sizeF;
            vrik.solver.locomotion.weight = 0f;
        }
        public static Transform sourceTransform;
        public static Hand _sourceHand;
        public static bool isRightHand;
        public static Hand sourceHand
        {
            get
            {
                return _sourceHand;
            }
            set
            {
                _sourceHand = value;
                isRightHand = (_sourceHand == VRPlayer.rightHand);
                ensureSourceTransform();
            }
        }
        public static bool ensureSourceTransform()
        {
            if (sourceTransform == null)
            {
                foreach (var t in sourceHand.GetComponentsInChildren<Transform>())
                {
                    if (t.name == "wrist_r")
                    {
                        sourceTransform = t;
                    }
                }
            }
            return sourceTransform != null;
        }

        public static void updateFingerPart(Transform source, Transform target)
        {
            target.rotation = Quaternion.LookRotation(-source.up, isRightHand ? source.right : -source.right);

            if (source.childCount > 0 && target.childCount > 0)
            {
                updateFingerPart(source.GetChild(0), target.GetChild(0));
            }
        }

        public static void updateFingerRotations()
        {
            if (!ensureSourceTransform())
            {
                return;
            }


            for (int i = 0; i < sourceTransform.childCount; i++)
            {

                var child = sourceTransform.GetChild(i);
                switch (child.name)
                {

                    case ("LeftHandThumb1"):
                    case ("RightHandThumb1"):
                        updateFingerPart(sourceTransform.GetChild(0).GetChild(0), child);
                        break;

                    case ("LeftHandIndex1"):
                    case ("RightHandIndex1"):
                        updateFingerPart(sourceTransform.GetChild(1).GetChild(0), child);
                        break;

                    case ("LeftHandMiddle1"):
                    case ("RightHandMiddle1"):
                        updateFingerPart(sourceTransform.GetChild(2).GetChild(0), child);
                        break;

                    case ("LeftHandRing1"):
                    case ("RightHandRing1"):
                        updateFingerPart(sourceTransform.GetChild(3).GetChild(0), child);
                        break;

                    case ("LeftHandPinky1"):
                    case ("RightHandPinky1"):
                        updateFingerPart(sourceTransform.GetChild(4).GetChild(0), child);
                        break;
                }
            }
        }

        private static bool IsPaused(VRIK vrik)
        {
            return
                vrik.solver.leftArm.target.parent == CameraRig &&
                vrik.solver.rightArm.target.parent == CameraRig &&
                vrik.solver.spine.headTarget.parent == CameraRig;
        }

        public static VRIK initialize(GameObject playerGameObject, Transform leftController, Transform rightController, Transform camera)
        {
            VRIK vrik = CreateTargets(playerGameObject);
            InitializeTargts(vrik, leftController, rightController, camera, Human.LocalHuman != null && playerGameObject == Human.LocalHuman.gameObject);
            return vrik;
        }
        public static void resetVrikHandTransform(Human player)
        {

            VRIK vrik = Human.LocalHuman.gameObject.GetComponent<VRIK>();

            if (vrik == null)
            {
                return;
            }

            /*if (player.GetComponent<VRPlayerSync>()?.currentLeftWeapon != null)
            {
                if (VHVRConfig.LeftHanded() && player.GetComponent<VRPlayerSync>().currentLeftWeapon.name.StartsWith("Spear") && !VHVRConfig.SpearInverseWield())
                {
                    vrik.solver.leftArm.target.localPosition = leftspearPosition;
                    vrik.solver.leftArm.target.localRotation = leftSpearRotation;
                    vrik.solver.leftArm.palmToThumbAxis = leftSpearEllbow;
                    return;
                }
                vrik.solver.leftArm.target.localPosition = leftEquippedPosition;
                vrik.solver.leftArm.target.localRotation = leftEquippedRotation;
                vrik.solver.leftArm.palmToThumbAxis = leftEquippedEllbow;
            }
            else*/

            vrik.solver.leftArm.target.localPosition = leftUnequippedPosition;
            vrik.solver.leftArm.target.localRotation = leftUnequippedRotation;
            vrik.solver.leftArm.palmToThumbAxis = leftUnequippedEllbow;


            /*if (player.GetComponent<VRPlayerSync>()?.currentRightWeapon != null)
            {
                if (!VHVRConfig.LeftHanded() && player.GetComponent<VRPlayerSync>().currentRightWeapon.name.StartsWith("Spear") && !VHVRConfig.SpearInverseWield())
                {
                    vrik.solver.rightArm.target.localPosition = rightspearPosition;
                    vrik.solver.rightArm.target.localRotation = rightSpearRotation;
                    vrik.solver.rightArm.palmToThumbAxis = rightSpearEllbow;
                    return;
                }
                vrik.solver.rightArm.target.localPosition = rightEquippedPosition;
                vrik.solver.rightArm.target.localRotation = rightEquippedRotation;
                vrik.solver.rightArm.palmToThumbAxis = rightEquippedEllbow;
                return;
            }*/

            vrik.solver.rightArm.target.localPosition = rightUnequippedPosition;
            vrik.solver.rightArm.target.localRotation = rightUnequippedRotation;
            vrik.solver.rightArm.palmToThumbAxis = rightUnequippedEllbow;
        }

        public static Transform GetLocalPlayerDominantHandConnector()
        {
            return ConfigFile.LeftHanded ? VrikCreator.localPlayerLeftHandConnector : VrikCreator.localPlayerRightHandConnector;
        }

        public static bool LeftHanded()
        {
            return ConfigFile.GetDominantHand() == VRPlayer.LEFT_HAND;
        }

        public static void ResetHandConnectors()
        {
            localPlayerLeftHandConnector.localPosition = Vector3.zero;
            localPlayerLeftHandConnector.localRotation = Quaternion.identity;
            localPlayerRightHandConnector.localPosition = Vector3.zero;
            localPlayerRightHandConnector.localRotation = Quaternion.identity;
        }


        public static void PauseLocalPlayerVrik()
        {
            VRIK vrik = Human.LocalHuman.gameObject?.GetComponent<VRIK>();

            if (vrik == null)
            {
                return;
            }

            if (IsPaused(vrik))
            {
                ModLog.Error("Trying to pause VRIK while it is already paused.");
                return;
            }

            vrik.solver.leftArm.target.SetParent(camera.parent, true);
            vrik.solver.rightArm.target.SetParent(camera.parent, true);
            vrik.solver.spine.headTarget.SetParent(camera.parent, true);
        }

        public static void UnpauseLocalPlayerVrik()
        {
            VRIK vrik = Human.LocalHuman?.GetComponent<VRIK>();

            if (vrik == null)
            {
                return;
            }

            if (!IsPaused(vrik))
            {
                ModLog.Error("Trying to unpause VRIK while it is not yet paused.");
                return;
            }

            InitializeTargts(vrik, localPlayerLeftHandConnector.parent, localPlayerRightHandConnector.parent, camera, isLocalPlayer: true);
            resetVrikHandTransform(Human.LocalHuman);
        }
    }
}