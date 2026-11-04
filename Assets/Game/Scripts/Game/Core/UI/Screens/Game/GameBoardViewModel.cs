using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class GameBoardViewModel : IDisposable
    {
        private const float RowHeight = 56;
        private readonly GameView _view;
        private readonly ShopAssets _assets;
        private readonly Action< int, int > _onItemClicked;
        private readonly Dictionary< int, ShelfViewModel > _shelves = new();
        private readonly Dictionary< int, BasketViewModel > _baskets = new();
        private readonly Dictionary< int, ItemViewModel > _items = new();

        public GameBoardViewModel( GameView view, ShopAssets assets, Action< int, int > onItemClicked )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
            _onItemClicked = onItemClicked;
        }

        public void Render( GamePresentation presentation, bool instant )
        {
            var shelfIds = new HashSet< int >();
            foreach ( var shelf in presentation.Shelves )
            {
                shelfIds.Add( shelf.Id );
                if ( !_shelves.TryGetValue( shelf.Id, out var model ) )
                {
                    var view = UnityEngine.Object.Instantiate( _assets.shelf, _view.Columns[ shelf.Column ] );
                    view.Rect.anchorMin = view.Rect.anchorMax = view.Rect.pivot = new Vector2( .5f, 0 );
                    view.Rect.anchoredPosition = new Vector2( 0, instant ? shelf.Offset * RowHeight : 340 + shelf.Column * 25 );
                    model = new ShelfViewModel( view, shelf.Items.Count, shelf.JarsPerRow, RowHeight );
                    _shelves.Add( shelf.Id, model );
                }

                model.Render( shelf, RowHeight, !instant );
            }

            var basketIds = new HashSet< int >();
            foreach ( var basket in presentation.Baskets )
            {
                basketIds.Add( basket.Id );
                if ( !_baskets.TryGetValue( basket.Id, out var model ) )
                {
                    var view = UnityEngine.Object.Instantiate( _assets.basket, _view.Baskets[ basket.Slot ] );
                    ( (RectTransform)view.transform ).anchoredPosition = Vector2.zero;
                    if ( !instant ) ShopMotion.Enter( view.transform, basket.Slot * .07f );
                    model = new BasketViewModel( view, _assets, _view.Effects );
                    _baskets.Add( basket.Id, model );
                }

                model.Render( basket );
            }

            var itemIds = new HashSet< int >();
            foreach ( var shelf in presentation.Shelves )
                for ( int i = 0; i < shelf.Items.Count; i++ )
                    if ( shelf.Items[ i ] != null )
                        PlaceItem( shelf.Items[ i ], _shelves[ shelf.Id ].GetSlot( i ), itemIds, instant );
            for ( int i = 0; i < presentation.Counter.Count; i++ ) PlaceItem( presentation.Counter[ i ], _view.Counter.Slots[ i ], itemIds, instant );
            foreach ( var basket in presentation.Baskets )
                for ( int i = 0; i < basket.Items.Count; i++ )
                    PlaceItem( basket.Items[ i ], _baskets[ basket.Id ].View.Slots[ i ], itemIds, instant );

            foreach ( int id in _items.Keys.Where( id => !itemIds.Contains( id ) ).ToArray() )
            {
                var model = _items[ id ];
                model.Dispose();
                if ( model.View.GetComponentInParent< BasketView >() == null )
                {
                    model.View.Detach( _view.ItemLayer );
                    model.View.Retire();
                }

                _items.Remove( id );
            }

            foreach ( int id in _shelves.Keys.Where( id => !shelfIds.Contains( id ) ).ToArray() )
            {
                _shelves[ id ].View.Fold();
                _shelves.Remove( id );
            }

            foreach ( int id in _baskets.Keys.Where( id => !basketIds.Contains( id ) ).ToArray() )
            {
                var model = _baskets[ id ];
                model.Dispose();
                model.View.Ship();
                _baskets.Remove( id );
            }
        }

        private void PlaceItem( ItemPresentation presentation, RectTransform slot, HashSet< int > wanted, bool instant )
        {
            wanted.Add( presentation.Id );
            bool fresh = !_items.TryGetValue( presentation.Id, out var model );
            if ( fresh )
            {
                var view = UnityEngine.Object.Instantiate( _assets.item, slot );
                model = new ItemViewModel( view, _assets, _onItemClicked );
                _items.Add( presentation.Id, model );
            }

            model.Render( presentation );
            model.View.Place( slot, _view.ItemLayer, !fresh && !instant );
            if ( fresh && !instant ) ShopMotion.Punch( model.View.transform, .08f );
        }

        public void Dispose()
        {
            foreach ( var item in _items.Values ) item.Dispose();
            foreach ( var basket in _baskets.Values ) basket.Dispose();
            _items.Clear();
            _baskets.Clear();
            _shelves.Clear();
        }
    }
}
