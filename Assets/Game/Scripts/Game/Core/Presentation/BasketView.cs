using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class BasketView : MonoBehaviour
    {
        [SerializeField] RectTransform[] slots;
        [SerializeField] TextMeshProUGUI order, count;
        [SerializeField] Image napkin, orderIcon;
        bool complete;
        public RectTransform Slot(int i) => slots[i];
        public void Bind(BasketState basket, ShopAssets assets, RectTransform effects)
        {
            var product = ProductCatalog.All[basket.type];
            order.text = product.Name; orderIcon.color = product.Color;
            count.text = basket.count + "/3";
            if (basket.count == 3 && !complete)
            {
                complete = true; napkin.gameObject.SetActive(true);
                napkin.transform.localScale = Vector3.zero;
                napkin.transform.DOScale(1,.3f).SetDelay(.45f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
                ShopMotion.Punch(transform,.13f);
                DOTween.Sequence().SetUpdate(true).SetLink(gameObject).AppendInterval(.55f)
                    .AppendCallback(() => ShopMotion.Confetti(assets,effects,transform.position));
            }
            else if (basket.count < 3 && complete) { complete = false; napkin.gameObject.SetActive(false); }
        }
        public void Ship()
        {
            transform.DOLocalMoveY(transform.localPosition.y + 115,.35f).SetEase(Ease.InBack).SetUpdate(true).SetLink(gameObject);
            transform.DOScale(.3f,.35f).SetUpdate(true).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }
    }
}
