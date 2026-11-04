using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class CounterView : MonoBehaviour
    {
        [SerializeField] RectTransform[] slots;
        [SerializeField] Image surface;
        [SerializeField] TextMeshProUGUI caption;
        Tween warning, shake;
        bool hot;
        Vector2 rest;
        public RectTransform Slot(int i) => slots[i];
        void Awake() { rest = ((RectTransform)transform).anchoredPosition; }
        public void Bind(int count, int capacity)
        {
            caption.text = "ПРИЛАВОК · " + count + "/" + capacity;
            bool nextHot = count >= capacity - 1;
            if (hot == nextHot) return;
            hot = nextHot; warning?.Kill();
            surface.color = ShopColors.Wood;
            if (hot) warning = surface.DOColor(ShopColors.Hex("E79A6B"),.45f).SetLoops(-1,LoopType.Yoyo).SetUpdate(true).SetLink(gameObject);
        }
        public void Shake()
        {
            shake?.Kill(); var rect = (RectTransform)transform; rect.anchoredPosition = rest;
            shake = rect.DOShakeAnchorPos(.3f,new Vector2(7,0),12).SetUpdate(true).SetLink(gameObject).OnComplete(() => rect.anchoredPosition = rest);
        }
        void OnDestroy() { warning?.Kill(); shake?.Kill(); }
    }
}
