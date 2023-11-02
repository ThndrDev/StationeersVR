using Assets.Scripts;
using HarmonyLib;
using StationeersVR.Utilities;
using UnityEngine;
using Valve.VR;
using Assets.Scripts.UI;
using JetBrains.Annotations;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace StationeersVR
{
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

            // Ensure GraphicRaycaster is present for the UI interactions
            if (menuCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                menuCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            // Add event trigger and collider to buttons for the laser pointer
            AddVRPointerEventAndCollider(__instance._customWorldButton);
            AddVRPointerEventAndCollider(__instance._newGameButton);
            AddVRPointerEventAndCollider(__instance._loadButton);
            AddVRPointerEventAndCollider(__instance._tutorialButton);
            AddVRPointerEventAndCollider(__instance._joinServerButton);
            AddVRPointerEventAndCollider(__instance._workshopButton);
            AddVRPointerEventAndCollider(__instance._scenarioButton);
            AddVRPointerEventAndCollider(__instance._settingsButton);
            AddVRPointerEventAndCollider(__instance._appearanceButton);
            AddVRPointerEventAndCollider(__instance._exitButton);
            AddVRPointerEventAndCollider(__instance._changeLogButton);
        }

        private static void AddVRPointerEventAndCollider(Button button)
        {
            // Add collider to the button
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            BoxCollider2D collider = button.gameObject.GetComponent<BoxCollider2D>() ?? button.gameObject.AddComponent<BoxCollider2D>();
            collider.size = rectTransform.sizeDelta;

            // Set the layer on the button:
            button.gameObject.layer = 12;

            // Add or get the EventTrigger component
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

            // Add pointer click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((eventData) => { button.onClick.Invoke(); });
            eventTrigger.triggers.Add(clickEntry);

            // You can add more events like PointerEnter, PointerExit if you want hover effects.
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
