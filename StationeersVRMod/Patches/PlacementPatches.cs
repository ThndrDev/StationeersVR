using JetBrains.Annotations;
using StationeersVR.Utilities;
using StationeersVR.VRCore.UI;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using Valve.VR;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Util;


// These Harmony patches are used to modify the placement control of the game when in VR mode
namespace StationeersVR
{
    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.PlacementMode))]
    [UsedImplicitly]
    class InventoryManager_PlacementMode_Patch
    {
        // Declare a Queue to store the previous 10 rotation values
        private static Queue<Vector2> handRotationHistory = new Queue<Vector2>();
        static int lastRotationFrame = 0;
        static int lastRotateBuildPieceFrame = 0;
        static float deadzone = 0.7f;
        static bool Prefix(InventoryManager __instance)
        {
            if (ConfigFile.UseVrControls)
            {
                switch (InventoryManager.ConstructionCursor.PlacementType)
                {
                    case PlacementSnap.Grid:
                        {
                            if (SteamVR_Actions.stationeers_Grab.GetState(SteamVR_Input_Sources.Any))
                            {
                                CheckHandsRotation();
                                rotateBuildPieces();
                            }
                        }
                        break;
                }
            }
            return true;
        }

        // rotation was detected, register the rotation frame and clear the rotation history
        private static void RotationDetected()
        {
            lastRotationFrame = Time.frameCount;
            handRotationHistory.Clear();
        }

        private static void CheckHandsRotation()
        {
            if (Time.frameCount - lastRotationFrame < 50)
            {
                return;
            }
            SteamVR_Action_Pose poseRValue = VRControls.instance.poseR;
            Quaternion lastRotation = poseRValue.GetLastLocalRotation(SteamVR_Input_Sources.RightHand);

            float rotationx = lastRotation.eulerAngles.x;
            float rotationy = lastRotation.eulerAngles.y;

            // Add the current rotation values to the rotation history
            handRotationHistory.Enqueue(new Vector2(rotationx, rotationy));

            // Remove the oldest rotation values if the history size exceeds 10
            if (handRotationHistory.Count > 10)
            {
                handRotationHistory.Dequeue();
            }

            Vector2 tenthLastRotation = handRotationHistory.Count >= 10 ? handRotationHistory.ElementAt(handRotationHistory.Count - 10) : Vector2.zero;

            // Compare the 10th last rotation values with the current rotation values
            if (tenthLastRotation != Vector2.zero)
            {
                if (Mathf.Abs(rotationx - tenthLastRotation.x) > 70)
                {
                    if (rotationx > tenthLastRotation.x)
                    {
                        ModLog.Debug("RightHand is rotating down");
                        InventoryManager.ConstructionCursor.ThingTransform.Rotate(Vector3.right, (InventoryManager.ConstructionCursor is IMounted) ? 180f : 90f, Space.World);
                        UIAudioManager.Play(InventoryManager.RotateBlueprintHash);
                        RotationDetected();
                        return;
                    }
                    else if (rotationx < tenthLastRotation.x)
                    {
                        ModLog.Debug("RightHand is rotating up");
                        InventoryManager.ConstructionCursor.ThingTransform.Rotate(Vector3.right, (InventoryManager.ConstructionCursor is IMounted) ? (-180f) : (-90f), Space.World);
                        UIAudioManager.Play(InventoryManager.RotateBlueprintHash);
                        RotationDetected();
                        return;
                    }
                }
                if (Mathf.Abs(rotationy - tenthLastRotation.y) > 70)
                {
                    if (rotationy > tenthLastRotation.y)
                    {
                        ModLog.Debug("RightHand is rotating left");
                        InventoryManager.ConstructionCursor.ThingTransform.Rotate(Vector3.up, 90f, Space.World);
                        UIAudioManager.Play(InventoryManager.RotateBlueprintHash);
                        RotationDetected();
                        return;
                    }
                    else if (rotationy < tenthLastRotation.y)
                    {
                        ModLog.Debug("RightHand is rotating right");
                        InventoryManager.ConstructionCursor.ThingTransform.Rotate(Vector3.up, -90f, Space.World);
                        UIAudioManager.Play(InventoryManager.RotateBlueprintHash);
                        RotationDetected();
                        return;
                    }
                    //TODO: add RotateRollLeft and right
                }
            }
            return;
        }

        private static void rotateBuildPieces()
        {
            if (Time.frameCount - lastRotateBuildPieceFrame < 50)
            {
                return;
            }
            float rightstickY = VRControls.instance.GetJoyRightStickY();
            if (rightstickY > 0f + deadzone)
            {
                // this is not working the same way mousewheel scrolling does, need to check why
                SmartRotate.GetNext(InventoryManager.ConstructionCursor as ISmartRotatable, Quaternion.identity);
            }
            else if (rightstickY < 0f - deadzone)
            {
                SmartRotate.GetPrevious(InventoryManager.ConstructionCursor as ISmartRotatable, Quaternion.identity);
            }
        }
    }
}
