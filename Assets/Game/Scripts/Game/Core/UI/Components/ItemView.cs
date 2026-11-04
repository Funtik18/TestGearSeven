using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ItemView : MonoBehaviour
    {
        [ field: SerializeField ]
        public Image Body { get; private set; }
        [ field: SerializeField ]
        public Image Cap { get; private set; }
        [ field: SerializeField ]
        public Image Rear { get; private set; }
        [ field: SerializeField ]
        public GameObject Bag { get; private set; }
        [ field: SerializeField ]
        public Button Button { get; private set; }

        public int Id { get; internal set; }
        public RectTransform Destination { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        private Sequence _flightSequence;

        public void PlayUnwrapped() => ShopMotion.Punch( transform, .16f );

        public void Place( RectTransform destination, RectTransform flightLayer, bool animate, float delay = 0 )
        {
            if ( Destination == destination && ( _flightSequence != null && _flightSequence.IsActive() || transform.parent == destination ) ) return;
            _flightSequence?.Kill();
            Destination = destination;
            if ( !animate )
            {
                Attach( destination );
                return;
            }

            Vector3 start = transform.position;
            transform.SetParent( flightLayer, true );
            Vector3 end = destination.TransformPoint( destination.rect.center );
            float canvasScale = GetComponentInParent< Canvas >().scaleFactor;
            Vector3 middle = Vector3.Lerp( start, end, .48f ) + Vector3.up * 65 * canvasScale;
            Vector2 targetSize = destination.rect.size;
            // Item prefabs use a common 48x64 design; scale to fit the destination slot.
            float targetScale = Mathf.Min( targetSize.x / 48f, targetSize.y / 64f );
            _flightSequence = DOTween.Sequence().SetUpdate( true ).SetLink( gameObject );
            _flightSequence.AppendInterval( delay );
            _flightSequence.Append( Rect.DOPath( new[] { start, middle, end }, .43f, PathType.CatmullRom ).SetEase( Ease.InOutSine ) );
            _flightSequence.Join( transform.DOScale( targetScale * 1.12f, .2f ).SetLoops( 2, LoopType.Yoyo ) );
            _flightSequence.Join( transform.DORotate( new Vector3( 0, 0, start.x > end.x ? -9 : 9 ), .2f ).SetLoops( 2, LoopType.Yoyo ) );
            _flightSequence.OnComplete( () =>
            {
                if ( Destination != null )
                {
                    Attach( Destination );
                    ShopMotion.Punch( Destination, .1f );
                }
            } );
        }

        private void Attach( RectTransform target )
        {
            transform.SetParent( target, false );
            Rect.anchorMin = Rect.anchorMax = Rect.pivot = new Vector2( .5f, .5f );
            Rect.anchoredPosition = Vector2.zero;
            Rect.sizeDelta = new Vector2( 48, 64 );
            float size = Mathf.Min( target.rect.width / 48f, target.rect.height / 64f );
            transform.localScale = Vector3.one * size;
            transform.localRotation = Quaternion.identity;
        }

        public void Retire()
        {
            _flightSequence?.Kill();
            Button.interactable = false;
            transform.DOScale( 0, .23f ).SetEase( Ease.InBack ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () => Destroy( gameObject ) );
        }

        public void Detach( RectTransform layer )
        {
            _flightSequence?.Kill();
            Destination = null;
            transform.SetParent( layer, true );
        }

        private void OnDestroy()
        {
            _flightSequence?.Kill();
        }
    }
}
