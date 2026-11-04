using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class BasketView : MonoBehaviour
    {
        [ field: SerializeField ]
        public RectTransform[] Slots { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Order { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Count { get; private set; }
        [ field: SerializeField ]
        public Image Napkin { get; private set; }
        [ field: SerializeField ]
        public Image OrderIcon { get; private set; }

        private Sequence _packingSequence;

        public void PlayPacked( ShopAssets assets, RectTransform effects )
        {
            StopPacking();
            Napkin.transform.localScale = Vector3.zero;
            _packingSequence = DOTween.Sequence().SetUpdate( true ).SetLink( gameObject );
            _packingSequence.AppendInterval( .45f );
            _packingSequence.Append( Napkin.transform.DOScale( 1, .3f ).SetEase( Ease.OutBack ) );
            _packingSequence.InsertCallback( .55f, () => ShopMotion.Confetti( assets, effects, transform.position ) );
            ShopMotion.Punch( transform, .13f );
        }

        public void StopPacking() => _packingSequence?.Kill();

        public void Ship()
        {
            StopPacking();
            transform.DOLocalMoveY( transform.localPosition.y + 115, .35f ).SetEase( Ease.InBack ).SetUpdate( true ).SetLink( gameObject );
            transform.DOScale( .3f, .35f ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () => Destroy( gameObject ) );
        }

        private void OnDestroy() => StopPacking();
    }
}
