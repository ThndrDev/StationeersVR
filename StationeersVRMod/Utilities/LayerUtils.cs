using UnityEngine;
using StationeersVR.Utilities;

/**
    Layer Index: 0, Layer Name: Default
    Layer Index: 1, Layer Name: TransparentFX
    Layer Index: 2, Layer Name: Ignore Raycast
    Layer Index: 4, Layer Name: Water
    Layer Index: 5, Layer Name: UI
    Layer Index: 8, Layer Name: PlayerInvisible
    Layer Index: 9, Layer Name: Player
    Layer Index: 10, Layer Name: PlayerImmune
    Layer Index: 11, Layer Name: PlayerRagdoll
    Layer Index: 12, Layer Name: WorldspaceUI
    Layer Index: 13, Layer Name: HUD
    Layer Index: 14, Layer Name: MiniMap
    Layer Index: 15, Layer Name: LabelText
    Layer Index: 16, Layer Name: Terrain
    Layer Index: 17, Layer Name: CursorVoxel
    Layer Index: 18, Layer Name: CharacterCreation
    Layer Index: 19, Layer Name: Planets
    Layer Index: 20, Layer Name: IgnoreWheelColliders
    Layer Index: 21, Layer Name: PostProcess
    Layer Index: 22, Layer Name: Stars
    Layer Index: 23, Layer Name: BlockSound
    Layer Index: 24, Layer Name: Plants
    Layer Index: 25, Layer Name: Cables
 */
namespace StationeersVR.Utilities
{
    static class LayerUtils
    {
        // I need a layer with non-visible objects since
        // layers are short supply. Must be
        // in sync with what is in the prefab in Unity Editor.
        //public static readonly int WATER = 4;
        public static readonly int PLAYER = 9;
        private static readonly int HANDS_LAYER = 27;
        public static readonly int HANDS_LAYER_MASK = (1 << HANDS_LAYER);
        public static readonly int UI_PANEL_LAYER = 28;
        public static readonly int UI_PANEL_LAYER_MASK = (5 << UI_PANEL_LAYER);
        private static readonly int WORLDSPACE_UI_LAYER = 12;
        public static readonly int WORLDSPACE_UI_LAYER_MASK = (1 << WORLDSPACE_UI_LAYER);

        public static int getHandsLayer()
        {
            checkLayer(HANDS_LAYER);
            return HANDS_LAYER;
        }

        public static int getUiPanelLayer()
        {
            checkLayer(UI_PANEL_LAYER);
            return UI_PANEL_LAYER;
        }

        public static int getWorldspaceUiLayer()
        {
            checkLayer(WORLDSPACE_UI_LAYER);
            return WORLDSPACE_UI_LAYER;
        }

        private static void checkLayer(int layer)
        {
            string layerString = LayerMask.LayerToName(layer);
            if (layerString != null && layerString.Length > 0)
            {
                ModLog.Warning("Layer " + layer + " is a named layer: " + layerString);
            }
        }

    }
}
