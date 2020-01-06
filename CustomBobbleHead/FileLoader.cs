using UnityEngine;
using System.IO;

namespace CustomBobbleHead
{
    static class FileLoader
    {


        //PUBLIC LOADING METHODS
        public static GameObject GetAssetBundleAsGameObject(string path, string name)
        {
            Debug.Log("AssetBundleLoader: Attempting to load AssetBundle...");
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if(bundle != null)
            {
                Debug.Log("AssetBundleLoader: Success.");
            }
            else
            {
                Debug.Log("AssetBundleLoader: Couldn't load AssetBundle from path: '" + path + "'");
            }

            Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'GameObject'.");
            var temp = bundle.LoadAsset(name, typeof(GameObject));

            if(temp != null)
            {
                Debug.Log("AssetBundleLoader: Success.");
                return (GameObject)temp;
            }
            else
            {
                Debug.Log("AssetBundleLoader: Couldn't retrieve GameObject from AssetBundle.");
                return null;
            }
        }
    }
}
