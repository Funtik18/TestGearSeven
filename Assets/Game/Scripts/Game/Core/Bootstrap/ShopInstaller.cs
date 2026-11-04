using UnityEngine;
using Zenject;

namespace RosiesCornerShop.Core
{
    public sealed class ShopInstaller : MonoInstaller
    {
        [ SerializeField ] private ShopApplication _application;

        public override void InstallBindings()
        {
            Container.BindInstance( _application.Assets );
            Container.BindInstance( _application.Audio );
            Container.BindInstance( _application.Toast );
            Container.Bind< Transform >().WithId( ShopUILayer.Screens ).FromInstance( _application.ScreenRoot );
            Container.Bind< Transform >().WithId( ShopUILayer.Dialogs ).FromInstance( _application.DialogRoot );

            Container.Bind< ProgressStore >().AsSingle();
            Container.Bind< GameProgress >()
                .FromMethod( context => context.Container.Resolve< ProgressStore >().Load() )
                .AsSingle();
            Container.Bind< GameSession >().AsSingle();
            Container.BindInterfacesAndSelfTo< ShopUIManager >().AsSingle();
        }
    }
}
