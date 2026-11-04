using TMPro;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ShopButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [ SerializeField ]
        private Button _button;
        [ SerializeField ]
        private TextMeshProUGUI _caption;
        private Tween _press;

        public void Bind( string label, Action action, bool available = true )
        {
            _caption.text = label;
            _button.interactable = available;
            _button.onClick.RemoveAllListeners();
            if ( action != null ) _button.onClick.AddListener( () => action() );
        }

        public void Unbind() => _button.onClick.RemoveAllListeners();

        public void SetLabel( string label ) => _caption.text = label;
        public void SetAvailable( bool value ) => _button.interactable = value;

        public void OnPointerDown( PointerEventData eventData )
        {
            if ( _button.interactable ) Scale( .94f, .08f );
        }

        public void OnPointerUp( PointerEventData eventData ) => Scale( 1, .24f );
        public void OnPointerExit( PointerEventData eventData ) => Scale( 1, .2f );

        private void Scale( float scale, float duration )
        {
            _press?.Kill();
            _press = transform.DOScale( scale, duration ).SetEase( Ease.OutBack ).SetUpdate( true ).SetLink( gameObject );
        }

        private void OnDisable()
        {
            _press?.Kill();
            transform.localScale = Vector3.one;
        }
    }
}
