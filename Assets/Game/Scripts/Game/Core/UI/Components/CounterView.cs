using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class CounterView : MonoBehaviour
    {
        [ field: SerializeField ]
        public RectTransform[] Slots { get; private set; }
        [ field: SerializeField ]
        public Image Surface { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Caption { get; private set; }

        private Tween _warningTween;
        private Tween _shakeTween;
        private bool _isWarning;
        private Vector2 _restPosition;

        private void Awake() => _restPosition = ( (RectTransform)transform ).anchoredPosition;

        public void SetWarning( bool active )
        {
            if ( _isWarning == active ) return;
            _isWarning = active;
            _warningTween?.Kill();
            Surface.color = ShopColors.Wood;
            if ( active ) _warningTween = Surface.DOColor( ShopColors.Hex( "E79A6B" ), .45f ).SetLoops( -1, LoopType.Yoyo ).SetUpdate( true ).SetLink( gameObject );
        }

        public void Shake()
        {
            _shakeTween?.Kill();
            var rect = (RectTransform)transform;
            rect.anchoredPosition = _restPosition;
            _shakeTween = rect.DOShakeAnchorPos( .3f, new Vector2( 7, 0 ), 12 ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () => rect.anchoredPosition = _restPosition );
        }

        private void OnDestroy()
        {
            _warningTween?.Kill();
            _shakeTween?.Kill();
        }
    }
}
