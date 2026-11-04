using System;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ItemViewModel : IDisposable
    {
        private readonly ItemView _view;
        private readonly ShopAssets _assets;
        private readonly Action< int, int > _onClicked;
        private ItemPresentation _presentation;
        private bool _wasWrapped;

        public ItemView View => _view;

        public ItemViewModel( ItemView view, ShopAssets assets, Action< int, int > onClicked = null )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
            _onClicked = onClicked;
            _view.Button.onClick.AddListener( ItemClickedHandler );
        }

        public void Render( ItemPresentation presentation )
        {
            _presentation = presentation;
            _view.Id = presentation.Id;
            _view.Button.interactable = presentation.CanTake;
            var product = presentation.Hammer ? null : ProductCatalog.All[ presentation.ProductId ];
            Color color = presentation.Hammer ? ShopColors.Dark : ShopColors.Product( product.Id );
            string shape = product == null ? "hammer" : product.Shape;

            _view.Body.color = color;
            _view.Cap.color = Color.Lerp( color, ShopColors.Dark, .25f );
            _view.Body.sprite = shape.StartsWith( "fresh" ) || shape == "donut" || shape == "cookie" || shape == "orange" ? _assets.circle : shape == "cheese" || shape == "cone" ? _assets.triangle : _assets.pixel;
            _view.Cap.gameObject.SetActive( _view.Body.sprite == _assets.pixel || presentation.Hammer );
            _view.Rear.gameObject.SetActive( presentation.RearProductId >= 0 );
            if ( presentation.RearProductId >= 0 ) _view.Rear.color = Color.Lerp( ShopColors.Product( presentation.RearProductId ), ShopColors.Cream, .3f );
            _view.Bag.SetActive( presentation.Wrapped );
            if ( _wasWrapped && !presentation.Wrapped ) _view.PlayUnwrapped();
            _wasWrapped = presentation.Wrapped;
        }

        private void ItemClickedHandler() => _onClicked?.Invoke( _presentation.ShelfId, _presentation.Slot );

        public void Dispose()
        {
            if ( _view == null ) return;
            _view.Button.onClick.RemoveListener( ItemClickedHandler );
            _view.Button.interactable = false;
        }
    }
}
