using System;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class BasketViewModel : IDisposable
    {
        private readonly BasketView _view;
        private readonly ShopAssets _assets;
        private readonly RectTransform _effects;
        private bool _wasComplete;

        public BasketView View => _view;

        public BasketViewModel( BasketView view, ShopAssets assets, RectTransform effects )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
            _effects = effects;
        }

        public void Render( BasketPresentation presentation )
        {
            _view.Order.text = ProductCatalog.All[ presentation.ProductId ].Name;
            _view.OrderIcon.color = ShopColors.Product( presentation.ProductId );
            _view.Count.text = presentation.ProgressLabel;
            _view.Napkin.gameObject.SetActive( presentation.Complete );
            if ( presentation.Complete && !_wasComplete ) _view.PlayPacked( _assets, _effects );
            else if ( !presentation.Complete && _wasComplete ) _view.StopPacking();
            _wasComplete = presentation.Complete;
        }

        public void Dispose()
        {
            if ( _view != null ) _view.StopPacking();
        }
    }
}
