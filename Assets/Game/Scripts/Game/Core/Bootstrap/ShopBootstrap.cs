using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RosiesCornerShop.Core
{
    public static class ShopBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("Shop EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            if (Object.FindAnyObjectByType<AudioListener>() == null)
                new GameObject("Shop Audio Listener",typeof(AudioListener));
            if (Object.FindAnyObjectByType<ShopApplication>() != null) return;
            ShopAssets assets = Resources.Load<ShopAssets>("Rosie/ShopAssets");
            if (assets == null || assets.root == null)
            { Debug.LogError("Rosie assets are missing. Run Tools > Rosie's Corner Shop > Rebuild prefabs."); return; }
            Object.Instantiate(assets.root);
        }
    }
}
