using System;
using System.Collections.Generic;
using System.Linq;
using SoosvetGames.VVM;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class WindowViewModel : ViewModel< WindowView >
    {
        private readonly ViewCreator _creator;
        private readonly WindowView _prefab;
        private readonly WindowPresentation _presentation;
        private readonly Action[] _commands;
        private readonly Action< int > _levelSelected;
        private readonly ShopAssets _assets;
        private readonly List< ShopButton > _levelButtons = new();

        public WindowViewModel( ViewCreator creator, WindowView prefab, WindowPresentation presentation, Action[] commands, Action< int > levelSelected, ShopAssets assets )
        {
            _creator = creator ?? throw new ArgumentNullException( nameof(creator) );
            _prefab = prefab ?? throw new ArgumentNullException( nameof(prefab) );
            _presentation = presentation ?? throw new ArgumentNullException( nameof(presentation) );
            _commands = commands ?? throw new ArgumentNullException( nameof(commands) );
            _levelSelected = levelSelected;
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
        }

        protected override WindowView GetView() => _creator.InstantiatePrefabForComponent< WindowView >( _prefab );

        protected override void SubscribeView()
        {
            base.SubscribeView();
            ModelView.OnActionClicked += ActionClickedHandler;
            ModelView.OnLevelSelected += LevelSelectedHandler;
        }

        protected override void UnSubscribeView()
        {
            base.UnSubscribeView();
            ModelView.OnActionClicked -= ActionClickedHandler;
            ModelView.OnLevelSelected -= LevelSelectedHandler;
            foreach ( var button in ModelView.Actions ) button.Unbind();
        }

        protected override void OnViewCreated()
        {
            ModelView.Title.text = _presentation.Title;
            ModelView.Description.text = _presentation.Description;

            for ( int i = 0; i < ModelView.Actions.Length; i++ )
            {
                int index = i;
                var button = ModelView.Actions[ i ];
                button.gameObject.SetActive( i < _presentation.Labels.Count );
                if ( i < _presentation.Labels.Count ) button.Bind( _presentation.Labels[ i ], () => ModelView.OnActionClick( index ) );
            }

            if ( _presentation.Levels == null ) return;
            for ( int i = 0; i < _presentation.Levels.Count; i++ ) CreateLevelButton( _presentation.Levels[ i ], i );
        }

        protected override void OnViewDispose()
        {
            foreach ( var button in _levelButtons )
                if ( button != null )
                    button.Unbind();
            _levelButtons.Clear();
        }

        private void CreateLevelButton( LevelOptionPresentation entry, int position )
        {
            var button = UnityEngine.Object.Instantiate( _assets.button, ModelView.Content );
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2( 0, 1 );
            rect.pivot = new Vector2( 0, 1 );
            rect.sizeDelta = new Vector2( 74, 52 );
            rect.anchoredPosition = new Vector2( position % 4 * 80, -position / 4 * 58 );
            button.Bind( entry.Label, () => ModelView.OnLevelSelect( entry.Index ), entry.Available );
            _levelButtons.Add( button );
        }

        private void ActionClickedHandler( int index )
        {
            if ( index >= 0 && index < _commands.Length && index < _presentation.Labels.Count ) _commands[ index ]?.Invoke();
        }

        private void LevelSelectedHandler( int index )
        {
            var entry = _presentation.Levels?.FirstOrDefault( option => option.Index == index );
            if ( entry != null && entry.Available ) _levelSelected?.Invoke( index );
        }
    }
}
