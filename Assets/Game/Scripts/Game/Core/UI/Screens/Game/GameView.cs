using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class GameView : ShopView
    {
        [ field: SerializeField ]
        public TextMeshProUGUI Level { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Progress { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Coins { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Instruction { get; private set; }
        [ field: SerializeField ]
        public Image ProgressFill { get; private set; }
        [ field: SerializeField ]
        public RectTransform[] Columns { get; private set; }
        [ field: SerializeField ]
        public RectTransform[] Baskets { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI[] Stock { get; private set; }
        [ field: SerializeField ]
        public RectTransform ItemLayer { get; private set; }
        [ field: SerializeField ]
        public RectTransform Effects { get; private set; }
        [ field: SerializeField ]
        public ShopButton SettingsButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton UndoButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton ExtraBasketButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton[] Boosters { get; private set; }
        [ field: SerializeField ]
        public CounterView Counter { get; private set; }

        public event Action OnSettingsClicked;
        public event Action OnUndoClicked;
        public event Action OnExtraBasketClicked;
        public event Action< int > OnBoosterClicked;
        public event Action< int, int > OnItemClicked;

        private Tween _progressTween;

        public void OnSettingsClick() => OnSettingsClicked?.Invoke();
        public void OnUndoClick() => OnUndoClicked?.Invoke();
        public void OnExtraBasketClick() => OnExtraBasketClicked?.Invoke();
        public void OnBoosterClick( int index ) => OnBoosterClicked?.Invoke( index );
        public void OnItemClick( int shelfId, int slot ) => OnItemClicked?.Invoke( shelfId, slot );

        public void AnimateProgress( float value, bool instant )
        {
            _progressTween?.Kill();
            if ( instant ) ProgressFill.fillAmount = value;
            else _progressTween = ProgressFill.DOFillAmount( value, .4f ).SetEase( Ease.OutCubic ).SetUpdate( true ).SetLink( gameObject );
        }

        public void Reject() => Counter.Shake();
        public void Celebrate( ShopAssets assets ) => ShopMotion.Confetti( assets, Effects, ProgressFill.transform.position );

        private void OnDestroy() => _progressTween?.Kill();
    }
}
