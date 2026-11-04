using System;
using TMPro;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class HomeView : ShopView
    {
        [ field: SerializeField ]
        public ShopButton PlayButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton LevelsButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton CollectionButton { get; private set; }
        [ field: SerializeField ]
        public ShopButton SettingsButton { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Statistics { get; private set; }

        public event Action OnPlayClicked;
        public event Action OnLevelsClicked;
        public event Action OnCollectionClicked;
        public event Action OnSettingsClicked;

        public void OnPlayClick() => OnPlayClicked?.Invoke();
        public void OnLevelsClick() => OnLevelsClicked?.Invoke();
        public void OnCollectionClick() => OnCollectionClicked?.Invoke();
        public void OnSettingsClick() => OnSettingsClicked?.Invoke();
    }
}
