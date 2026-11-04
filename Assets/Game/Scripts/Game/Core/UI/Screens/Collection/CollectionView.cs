using System;
using TMPro;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class CollectionView : ShopView
    {
        [ field: SerializeField ]
        public RectTransform Content { get; private set; }
        [ field: SerializeField ]
        public TextMeshProUGUI Summary { get; private set; }
        [ field: SerializeField ]
        public ShopButton BackButton { get; private set; }

        public event Action OnBackClicked;

        public void OnBackClick() => OnBackClicked?.Invoke();
    }
}
