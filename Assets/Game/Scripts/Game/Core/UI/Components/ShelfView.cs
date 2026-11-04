using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ShelfView : MonoBehaviour
    {
        [ field: SerializeField ]
        public RectTransform SlotsRoot { get; private set; }
        [ field: SerializeField ]
        public RectTransform SlotPrefab { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Status { get; private set; }
        [ field: SerializeField ]
        public Image Barrier { get; private set; }

        public RectTransform Rect => (RectTransform)transform;
        private Tween _movementTween;

        public void MoveTo( Vector2 position, bool animate, float delay )
        {
            _movementTween?.Kill();
            if ( !animate ) Rect.anchoredPosition = position;
            else _movementTween = Rect.DOAnchorPos( position, .46f ).SetDelay( delay ).SetEase( Ease.OutBack ).SetUpdate( true ).SetLink( gameObject );
        }

        public void PlayUnlocked() => ShopMotion.Punch( transform, .1f );

        public void Fold()
        {
            _movementTween?.Kill();
            transform.DOScaleY( 0, .2f ).SetEase( Ease.InBack ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () => Destroy( gameObject ) );
        }

        private void OnDestroy() => _movementTween?.Kill();
    }
}
