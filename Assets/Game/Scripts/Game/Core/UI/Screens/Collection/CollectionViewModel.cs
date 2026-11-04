using System;
using System.Collections.Generic;
using System.Linq;
using SoosvetGames.VVM;
using TMPro;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class CollectionViewModel : ViewModel< CollectionView >
    {
        private readonly GameProgress _progress;
        private readonly ShopUIManager _ui;
        private readonly ShopAssets _assets;
        private readonly List< ShopButton > _cards = new();
        private readonly List< ItemViewModel > _items = new();

        public CollectionViewModel( GameProgress progress, ShopUIManager ui, ShopAssets assets )
        {
            _progress = progress ?? throw new ArgumentNullException( nameof(progress) );
            _ui = ui ?? throw new ArgumentNullException( nameof(ui) );
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
        }

        protected override void SubscribeView()
        {
            base.SubscribeView();
            ModelView.OnBackClicked += BackClickedHandler;
            ModelView.BackButton.Bind( "В лавку", ModelView.OnBackClick );
        }

        protected override void UnSubscribeView()
        {
            base.UnSubscribeView();
            ModelView.OnBackClicked -= BackClickedHandler;
            ModelView.BackButton.Unbind();
        }

        protected override void OnViewCreated()
        {
            ModelView.Summary.text = "Найдено " + _progress.found.Count( found => found ) + " из " + ProductCatalog.All.Length;
            foreach ( var product in ProductCatalog.All ) CreateCard( product.Id );
            ModelView.Content.sizeDelta = new Vector2( 369, Mathf.CeilToInt( ProductCatalog.All.Length / 3f ) * 118 );
        }

        protected override void OnViewDispose()
        {
            foreach ( var item in _items ) item.Dispose();
            foreach ( var card in _cards )
                if ( card != null )
                    card.Unbind();
            _items.Clear();
            _cards.Clear();
        }

        private void CreateCard( int id )
        {
            var card = UnityEngine.Object.Instantiate( _assets.button, ModelView.Content );
            var rect = (RectTransform)card.transform;
            rect.anchorMin = rect.anchorMax = new Vector2( 0, 1 );
            rect.pivot = new Vector2( 0, 1 );
            rect.anchoredPosition = new Vector2( id % 3 * 123, -id / 3 * 118 );
            rect.sizeDelta = new Vector2( 113, 105 );
            card.Bind( _progress.found[ id ] ? ProductCatalog.All[ id ].Name : "?", () => ProductClickedHandler( id ), _progress.found[ id ] );
            _cards.Add( card );
            if ( !_progress.found[ id ] ) return;

            var holder = new GameObject( "Product", typeof(RectTransform) ).GetComponent< RectTransform >();
            holder.SetParent( card.transform, false );
            holder.anchoredPosition = new Vector2( 0, 16 );
            holder.sizeDelta = new Vector2( 35, 47 );
            var view = UnityEngine.Object.Instantiate( _assets.item, holder );
            var model = new ItemViewModel( view, _assets );
            model.Render( new ItemPresentation( id, id ) );
            view.Place( holder, holder, false );
            _items.Add( model );

            var caption = card.GetComponentInChildren< TextMeshProUGUI >();
            caption.alignment = TextAlignmentOptions.Bottom;
            caption.fontSize = 13;
        }

        private void BackClickedHandler() => _ui.ShowHome();
        private void ProductClickedHandler( int id ) => _ui.Notify( ProductCatalog.All[ id ].Name );
    }
}
