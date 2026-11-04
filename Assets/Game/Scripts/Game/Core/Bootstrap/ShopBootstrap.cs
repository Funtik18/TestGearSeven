using UnityEngine;
using Zenject;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RosiesCornerShop.Core
{
    public static class ShopBootstrap
    {
        [ RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad ) ]
        private static void Start()
        {
            if ( Object.FindAnyObjectByType< EventSystem >() == null )
                new GameObject( "Shop EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule) );
            if ( Object.FindAnyObjectByType< AudioListener >() == null )
                new GameObject( "Shop Audio Listener", typeof(AudioListener) );
            var existing = Object.FindAnyObjectByType< ShopApplication >();
            if ( existing != null )
            {
                var context = existing.GetComponent< GameObjectContext >();
                if ( context != null && !context.Initialized ) ProjectContext.Instance.Container.InjectGameObject( existing.gameObject );
                return;
            }
            ShopAssets assets = Resources.Load< ShopAssets >( "Rosie/ShopAssets" );
            if ( assets == null || assets.root == null )
            {
                Debug.LogError( "ShopAssets or ShopRoot is missing in Resources/Rosie." );
                return;
            }

            ProjectContext.Instance.Container.InstantiatePrefabForComponent< ShopApplication >( assets.root );
        }
    }
}
