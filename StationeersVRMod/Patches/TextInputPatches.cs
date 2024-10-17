using System.Text;
using System.Threading;
//using GUIFramework;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using StationeersVR.Patches;
using StationeersVR.Utilities;
using Valve.VR;
using TMPro;
using MoonSharp.Interpreter;
using static UnityEngine.UIElements.TextField;
using Valve.VR.InteractionSystem;
using BepInEx.Configuration;
using UnityEngine.EventSystems;
using static System.Net.Mime.MediaTypeNames;
using Assets.Scripts.UI;

namespace StationeersVR.Patches
{

    [HarmonyPatch(typeof(InputField), "OnPointerClick")]
    class PatchInputFieldClick
    {
        public static void Postfix(InputField __instance)
        {
            if (Utilities.ConfigFile.UseVrControls)
                InputFieldManager.start(__instance, null, false);
            else
                InputFieldManager.StartMKB(__instance, null);
        }
    }

    [HarmonyPatch(typeof(TMP_InputField), "OnPointerClick")]
    class PatchInputFieldTmpClick
    {
        public static void Postfix(TMP_InputField __instance)
        {
            if (Utilities.ConfigFile.UseVrControls)
                InputFieldManager.start(null, __instance, false);
            else
                InputFieldManager.StartMKB(null, __instance);
        }       
    }


    [HarmonyPatch(typeof(TMP_InputField), "OnFocus")]
    class PatchPasswordFieldFocus
    {
        static private TMP_InputField passwordInputField;
        public static void Postfix(TMP_InputField __instance)
        {
            if (Utilities.ConfigFile.UseVrControls && __instance.inputType == TMP_InputField.InputType.Password)
            {
                InputFieldManager.start(null, __instance, false, OnClose);
                passwordInputField = __instance;
            }
            if (!Utilities.ConfigFile.UseVrControls && __instance.inputType == TMP_InputField.InputType.Password)
            {
                InputFieldManager.StartMKB(null, __instance, OnClose);
                passwordInputField = __instance;
            }
        }

        private static void OnClose()
        {
            passwordInputField.OnSubmit(null);
        }

    }

   /* [HarmonyPatch(typeof(Input), "GetKeyDownInt")]
    class PatchInputGetKeyDownInt
    {
        public static bool Prefix(ref bool __result, KeyCode key)
        {
            //ModLog.Error("KeyDown: " + key);
            return !Utilities.ConfigFile.UseVrControls || InputFieldManager.handleReturnKeyInput(ref __result, key);
        }
    }

    [HarmonyPatch(typeof(Input), "GetKeyInt")]
    class PatchInputGetKeyInt
    {

        public static bool Prefix(ref bool __result, KeyCode key)
        {
            //ModLog.Error("Key: " + key);
            return !Utilities.ConfigFile.UseVrControls || InputFieldManager.handleReturnKeyInput(ref __result, key);
        }
    }*/
}
public class InputFieldManager : MonoBehaviour
{

    private static bool initialized;
    private static InputField _inputField;
    private static TMP_InputField _inputFieldTmp;
    //private static GuiInputField _inputFieldGui;
    private static UnityAction _closedAction;
    private static bool _returnOnClose;

    public static float closeTime;
    public static bool triggerReturn;
    //This Is for when motion controls is being used
    public static void start(InputField inputField, TMP_InputField inputFieldTmp, bool returnOnClose = false, UnityAction closedAction = null)
    {
        // TODO: consider enforcing the check that one and only one among inputField, inputFieldGui, and inputFieldTmp is non-null.
        _inputField = inputField;
        _inputFieldTmp = inputFieldTmp;
        _returnOnClose = returnOnClose;
        _closedAction = closedAction;
        if (_inputField != null && _inputField.text == "...")
        {
            _inputField.text = "";
        }

        if (_inputFieldTmp != null && _inputFieldTmp.text == "...")
        {
            _inputFieldTmp.text = "";
        }
        if (!initialized)
        {
            SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
            initialized = true;
        }
        if (StationeersVR.Utilities.ConfigFile.UseVrControls)
            SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, "TextInput", 256, _inputField != null ? _inputField.text : _inputFieldTmp != null ? _inputFieldTmp.text : _inputFieldTmp.text, 1);
    }
    //This is for when Mouse and keyboard is being used
    public static void StartMKB(InputField inputField, TMP_InputField inputFieldTmp, UnityAction closedAction = null)
    {
        _inputField = inputField;
        _inputFieldTmp = inputFieldTmp;
        if (!initialized)
        {
            if (_inputField != null)
            {
                initialized = true;
            }
            else if (_inputFieldTmp != null)
            {
                initialized = true;
            }
            else
            {
                initialized = false;
            }
        }

    }

    void Update()
    {
        if(Input.anyKeyDown)
        {
            if (initialized)
            {
                if (_inputField != null)
                {
                    _inputField.ProcessEvent(Event.current);
                    _inputField.ForceLabelUpdate();
                }
                else if (_inputFieldTmp != null)
                {
                    _inputFieldTmp.ProcessEvent(Event.current);
                    _inputFieldTmp.ForceLabelUpdate();
                }
                else
                    initialized = false;
                
            }
        }
    }

    private static void OnKeyboardClosed(VREvent_t args)
    {
        closeTime = Time.fixedTime;
        StringBuilder textBuilder = new StringBuilder(256);
        int caretPosition = (int)SteamVR.instance.overlay.GetKeyboardText(textBuilder, 256);
        string text = textBuilder.ToString();

        if (_inputField)
        {
            _inputField.caretPosition = caretPosition;
            _inputField.text = text;
        }

        if (_inputFieldTmp)
        {
            _inputFieldTmp.caretPosition = caretPosition;
            _inputFieldTmp.text = text;
        }

        // if (_inputFieldGui)
        // {
        //     _inputFieldGui.caretPosition = caretPosition;
        //      _inputFieldGui.text = text;
        // }

        /* if (Scripts.QuickAbstract.shouldStartChat)
         {
             if (text != "")
             {
                 if (text.StartsWith("/cmd")) //SEND CONSOLE INPUT
                 {
                     if (text.StartsWith("/cmd "))
                         text = text.Remove(0, 5);
                     else
                         text = text.Remove(0, 4);

                     Console.instance.TryRunCommand(text);
                 }
                 else //SEND CHAT INPUT
                 {
                     Chat.instance.m_input.text = text;
                     Chat.instance.InputText();
                     Chat.instance.m_input.text = "";
                 }
             }
         }
         Scripts.QuickAbstract.shouldStartChat = false;*/

        triggerReturn = _returnOnClose;

        // If return is to be triggered, we will wait until then to fire close action.
        if (!_returnOnClose)
        {
            _closedAction?.Invoke();
        }
    }

    /*public static bool handleReturnKeyInput(ref bool result, KeyCode key)
    {
        ModLog.Error("handleReturnKeyInput: " + key);
        if (triggerReturn && key == KeyCode.Return)
        {
            result = true;
            triggerReturn = false;
            new Thread(() => _closedAction?.Invoke()).Start();
            return false;
        }

        return true;
    }*/
}