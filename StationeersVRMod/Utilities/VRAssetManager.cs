using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;

/**
 * Manages assets required for mod
 */
namespace StationeersVR.Utilities
{
    static class VRAssetManager
    {
        private static readonly string STEAMVR_PREFAB_ASSETBUNDLE_NAME = "steamvr_player_prefabs";
        //private static readonly string CUSTOM_RESOURCES_ASSETBUNDLE_NAME = "stationeersvr_custom";
        private static readonly string STEAM_VR_SHADERS = "steamvr_shaders";
        private static Dictionary<string, Object> _assets;
        private static bool initialized = false;
        /**
         * Loads all the prefabs from disk and saves references
         * to them in local memory for quick access.
         */

        public static bool Initialize()
        {
 
            ModLog.Debug("Initializing VRAssetManager");
            if (initialized)
            {
                ModLog.Debug("VR assets already loaded.");
                return true;
            }
            _assets = new Dictionary<string, Object>();
            bool loadResult = true;
            foreach (var assetBundleName in new string[]
                { STEAMVR_PREFAB_ASSETBUNDLE_NAME, STEAM_VR_SHADERS }) //CUSTOM_RESOURCES_ASSETBUNDLE_NAME })
            {
                loadResult &= LoadAssets(assetBundleName);
            }
            initialized = loadResult;
            return loadResult;
        }
 
        private static bool LoadAssets(string assetBundleName)
        {
            string assetBundlePath = Path.Combine(Application.streamingAssetsPath,
                assetBundleName);
            AssetBundle prefabAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (prefabAssetBundle == null)
            {
                ModLog.Error("Problem loading AssetBundle from file: " + assetBundlePath);
                return false;
            }
            foreach (var asset in prefabAssetBundle.LoadAllAssets())
            {
                if (!_assets.ContainsKey(asset.name))
                {
                    _assets.Add(asset.name, asset);
                }
                else
                {
                    ModLog.Warning("Asset with duplicate name loaded: " + asset.name);
                }
            }
            return true;
        }

        /**
         * Return an asset of type T from the loaded
         * asset bundles.
         */
        public static T GetAsset<T>(string name) where T :Object
        {
            ModLog.Debug("Getting asset: " + name);
            if (!initialized)
            {
                ModLog.Error("GetAsset called before Initialize()");
                return default;
            }
            if (!_assets.ContainsKey(name))
            {
                ModLog.Error("No asset with name found: " + name);
            }
            var loadedAsset = _assets[name];
            if (loadedAsset == null)
            {
                ModLog.Error("Loaded asset is null!");
                return default;
            }
            if (!loadedAsset.GetType().IsAssignableFrom(typeof(T))) {
                ModLog.Error("Asset " + name + " is not assignable to type " + typeof(T));
                return default;
            }
            ModLog.Debug("Asset " + name + " successfully retrieved.");
            return loadedAsset as T;
        }
    }
}
