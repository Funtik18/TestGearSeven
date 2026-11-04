using System;
using DG.Tweening;
using SoosvetGames.VVM;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    [ RequireComponent( typeof(CanvasGroup) ) ]
    public abstract class ShopView : UIView
    {
        private CanvasGroup _canvasGroup;
        private Tween _fade;
        private CanvasGroup Group
        {
            get
            {
                if ( _canvasGroup == null ) _canvasGroup = GetComponent< CanvasGroup >();
                if ( _canvasGroup == null ) _canvasGroup = gameObject.AddComponent< CanvasGroup >();
                return _canvasGroup;
            }
        }

        public override void Show( Action callback = null )
        {
            _fade?.Kill();
            gameObject.SetActive( true );
            Group.alpha = 0;
            Group.blocksRaycasts = Group.interactable = true;
            IsInProcess = true;
            IsShowing = true;
            _fade = Group.DOFade( 1, .2f ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () =>
            {
                IsInProcess = false;
                callback?.Invoke();
            } );
        }

        public override void Hide( Action callback = null )
        {
            _fade?.Kill();
            Group.blocksRaycasts = Group.interactable = false;
            IsInProcess = true;
            IsShowing = false;
            _fade = Group.DOFade( 0, .16f ).SetUpdate( true ).SetLink( gameObject ).OnComplete( () =>
            {
                IsInProcess = false;
                callback?.Invoke();
            } );
        }

        public override void Dispose()
        {
            _fade?.Kill();
            base.Dispose();
        }
    }
}
