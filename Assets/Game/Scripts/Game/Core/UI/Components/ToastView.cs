using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ToastView : MonoBehaviour
    {
        [ field: SerializeField ]
        public TextMeshProUGUI Caption { get; private set; }
        [ field: SerializeField ]
        public CanvasGroup Group { get; private set; }

        private Sequence _sequence;

        public void Show( string message )
        {
            _sequence?.Kill();
            Caption.text = message;
            Group.alpha = 0;
            _sequence = DOTween.Sequence().SetUpdate( true ).SetLink( gameObject );
            _sequence.Append( Group.DOFade( 1, .18f ) ).AppendInterval( 2.6f ).Append( Group.DOFade( 0, .3f ) );
        }

        private void OnDestroy() => _sequence?.Kill();
    }
}
