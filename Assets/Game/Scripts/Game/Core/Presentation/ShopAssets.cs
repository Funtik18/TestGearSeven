using UnityEngine;

namespace RosiesCornerShop.Core
{
    [CreateAssetMenu(menuName = "Rosie/Shop assets")]
    public sealed class ShopAssets : ScriptableObject
    {
        public ShopApplication root;
        public HomeView home;
        public GameView game;
        public CollectionView collection;
        public ItemView item;
        public ShelfView shelf;
        public BasketView basket;
        public CounterView counter;
        public RectTransform cabinet;
        public ToastView toast;
        public ShopButton button;
        public WindowView settingsWindow, winWindow, loseWindow, levelsWindow, purchaseWindow, tutorialWindow;
        public RectTransform confetti;
        public Sprite pixel, circle, triangle;
    }
}
