using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace RosiesCornerShop.Core
{
    public sealed class WindowView : ShopView
    {
        [ field: SerializeField ]
        public RectTransform Panel { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Title { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Description { get; private set; }
        [ field: SerializeField ]
        public ShopButton[] Actions { get; private set; }
        [ field: SerializeField ]
        public RectTransform Content { get; private set; }

        public event Action< int > OnActionClicked;
        public event Action< int > OnLevelSelected;

        private Tween _popupTween;

        public void OnActionClick( int index ) => OnActionClicked?.Invoke( index );
        public void OnLevelSelect( int index ) => OnLevelSelected?.Invoke( index );

        public override void Show( Action callback = null )
        {
            base.Show( callback );
            _popupTween?.Kill();
            Panel.localScale = Vector3.one * .7f;
            _popupTween = Panel.DOScale( 1, .38f ).SetEase( Ease.OutBack ).SetUpdate( true ).SetLink( gameObject );
        }

        public override void Hide( Action callback = null )
        {
            base.Hide( callback );
            _popupTween?.Kill();
            _popupTween = Panel.DOScale( .85f, .16f ).SetEase( Ease.InBack ).SetUpdate( true ).SetLink( gameObject );
        }

        public override void Dispose()
        {
            _popupTween?.Kill();
            base.Dispose();
        }
    }
}
