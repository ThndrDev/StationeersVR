using Assets.Scripts;
using HarmonyLib;
using StationeersVR.Utilities;
using UnityEngine;
using Assets.Scripts.UI;
using JetBrains.Annotations;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace StationeersVR
{

    public class TransformLock : MonoBehaviour
    {
        Quaternion fixedRotation = Quaternion.identity;
        Vector3 fixedPosition = new Vector3();
        bool isLocked = true;

        void Update()
        {
            if (isLocked)
            {
                transform.rotation = fixedRotation;
                transform.position = fixedPosition;
            }
        }

        public void SetRotation(Quaternion quaternion)
        {
            fixedRotation = quaternion;
        }

        public void SetPosition(Vector3 position)
        {
            fixedPosition = position;
        }

        public void setLocked(bool locked)
        {
            isLocked = locked;
        }
    }

    [HarmonyPatch(typeof(MainMenu))]
    public class MainMenuPatches
    {
        //private static GameObject _mainMenuScene;
        //private static bool started1;
        [HarmonyPatch("Awake")]
        [UsedImplicitly]
        [HarmonyPostfix]
            private static void MainMenuPatch(MainMenu __instance)
        {
            ModLog.Debug("Patching Main Menu Canvas (awake)");
            //_mainMenuScene = GameObject.Find("MainMenuScene");
            Canvas menuCanvas = __instance.MainMenuCanvas;
            //Move the menu canvas to the WorldSpace           
            menuCanvas.renderMode = RenderMode.WorldSpace;
            //Reposition the menu in the world
            menuCanvas.transform.position = new Vector3(0.3f, 0.9f, -0.90f);
            menuCanvas.transform.rotation = new Quaternion(0f, 1f, 0f, 0f);
            menuCanvas.transform.localScale = new Vector3(0.001043074f, 0.001043074f, 0.001043074f);
            menuCanvas.transform.Rotate(Vector3.up, -30f);

            // Don't allow the game to move the camera while we're in the menu. Clicking "New Game" wants
            // to rotate and move the camera, for example. As the menus are in world space, this means we
            // end up no longer being able to see the menus easily.
            ModLog.Debug("Locking Main Camera xform");
            Camera mainCamera = CameraUtils.GetCamera(CameraUtils.MAIN_CAMERA);
            TransformLock txLockComponent = mainCamera.gameObject.AddComponent<TransformLock>();
            txLockComponent.SetRotation(mainCamera.transform.rotation);
            txLockComponent.SetPosition(mainCamera.transform.position);

            menuCanvas.gameObject.AddComponent<GraphicRaycaster>();
            menuCanvas.worldCamera = CameraUtils.GetCamera(CameraUtils.VR_CAMERA);
        }


        /* // This code is used to move the canvas at runtime using the keypad buttons, to help find the correct position.
           // Pressing * will print the current canvas coordinates to be used on the patch.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ManagerBase), nameof(ManagerBase.ManagerUpdate))]
        private static void HandleCameras()
        {
            if (started1)
            {

                HandleUIMenu();
            }
        }
        private static void HandleUIMenu()
        {
            Canvas menuCanvas = GameObject.Find("MainMenuCanvas").GetComponent<Canvas>();
            if (Input.GetKeyDown(KeyCode.Keypad4))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x + 0.01f, menuCanvas.transform.position.y, menuCanvas.transform.position.z);
            else if (Input.GetKeyDown(KeyCode.Keypad6))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x - 0.01f, menuCanvas.transform.position.y, menuCanvas.transform.position.z);
            else if (Input.GetKeyDown(KeyCode.Keypad8))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x, menuCanvas.transform.position.y + 0.01f, menuCanvas.transform.position.z);
            else if (Input.GetKeyDown(KeyCode.Keypad2))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x, menuCanvas.transform.position.y - 0.01f, menuCanvas.transform.position.z);
            else if (Input.GetKeyDown(KeyCode.Keypad9))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x, menuCanvas.transform.position.y, menuCanvas.transform.position.z + 0.01f);
            else if (Input.GetKeyDown(KeyCode.Keypad7))
                menuCanvas.transform.position = new Vector3(menuCanvas.transform.position.x, menuCanvas.transform.position.y, menuCanvas.transform.position.z - 0.01f);
            else if (Input.GetKeyDown(KeyCode.KeypadPlus))
                menuCanvas.transform.localScale = new Vector3(menuCanvas.transform.localScale.x + 0.00005f, menuCanvas.transform.localScale.y + 0.00005f, menuCanvas.transform.localScale.z + 0.00005f);
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                menuCanvas.transform.localScale = new Vector3(menuCanvas.transform.localScale.x - 0.00005f, menuCanvas.transform.localScale.y - 0.00005f, menuCanvas.transform.localScale.z - 0.00005f);
            else if (Input.GetKeyDown(KeyCode.Keypad3))
                menuCanvas.transform.rotation = Quaternion.Euler(0, 180, 0);
            else if (Input.GetKeyDown(KeyCode.Keypad1))
                menuCanvas.transform.rotation = Quaternion.Euler(0, -180, 0);
            else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
                ModLog.Debug("MainMenuCanvas POSITION: " + menuCanvas.transform.position + " Rotation: " + menuCanvas.transform.rotation + " Scale: " + menuCanvas.transform.localScale.x);
        }*/

    }
}
