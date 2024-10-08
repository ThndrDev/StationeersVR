using Assets.Scripts.UI;
using HarmonyLib;
using UnityEngine;

namespace StationeersVR.Patches
{
    internal class CreativePatches
    {
        [HarmonyPatch(typeof(DynamicInvPanel), nameof(DynamicInvPanel.AddDynamicItem))]
        public static class DynamicInvPanel_AddDynamicItem_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(string name, DynamicInvPanel __instance)
            {
                DynamicItem dynamicItem = Object.Instantiate(__instance.DynamicItemPrefab, __instance.Content, worldPositionStays: false);
                __instance.AllDynamicItems.Add(dynamicItem);
                dynamicItem.name = "~Spawn" + name;
                dynamicItem.Name.text = name;
                dynamicItem.Transform.localScale = new Vector3(1f, 1f, 1f);
                dynamicItem.Background.sprite = Resources.Load<Sprite>("UI/Thumbnails/" + name);
                dynamicItem.Background.color = __instance.color[(!dynamicItem.Background.sprite) ? 1 : 0];
                dynamicItem.Toggle.group = __instance._group;
                return false;
            }
        }
    }
}
