using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public static class ShopMotion
    {
        public static void Punch( Transform target, float strength = .12f )
        {
            if ( target != null ) target.DOPunchScale( Vector3.one * strength, .28f, 5, .55f ).SetUpdate( true ).SetLink( target.gameObject );
        }

        public static void Enter( Transform target, float delay = 0 )
        {
            target.localScale = Vector3.one * .82f;
            target.DOScale( 1, .38f ).SetDelay( delay ).SetEase( Ease.OutBack ).SetUpdate( true ).SetLink( target.gameObject );
        }

        public static void Confetti( ShopAssets assets, RectTransform parent, Vector3 position )
        {
            for ( int i = 0; i < 22; i++ )
            {
                var particle = Object.Instantiate( assets.confetti, parent );
                particle.position = position;
                particle.GetComponent< Image >().color = i % 3 == 0 ? ShopColors.Gold : i % 3 == 1 ? ShopColors.Berry : ShopColors.Green;
                var end = particle.anchoredPosition + new Vector2( Random.Range( -120, 120 ), Random.Range( 30, 135 ) );
                var sequence = DOTween.Sequence().SetUpdate( true ).SetLink( particle.gameObject );
                sequence.Append( particle.DOAnchorPos( end, .45f ).SetEase( Ease.OutQuad ) );
                sequence.Join( particle.DORotate( new Vector3( 0, 0, Random.Range( -260, 260 ) ), .7f ) );
                sequence.Append( particle.DOAnchorPosY( end.y - Random.Range( 80, 180 ), .5f ).SetEase( Ease.InQuad ) );
                sequence.Join( particle.GetComponent< Image >().DOFade( 0, .5f ) );
                sequence.OnComplete( () =>
                {
                    if ( particle != null ) Object.Destroy( particle.gameObject );
                } );
            }
        }
    }
}
