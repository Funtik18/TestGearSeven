using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class CollectionView : MonoBehaviour
    {
        [SerializeField] RectTransform content;
        [SerializeField] TextMeshProUGUI summary;
        [SerializeField] ShopButton back;
        public void Initialize(ShopApplication app, ShopAssets assets)
        {
            back.Bind("В лавку",app.ShowHome);
            summary.text = "Найдено " + app.Progress.found.Count(f => f) + " из " + ProductCatalog.All.Length;
            for (int i = 0; i < ProductCatalog.All.Length; i++)
            {
                int id = i;
                ShopButton card = Instantiate(assets.button,content);
                RectTransform rect = (RectTransform)card.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0,1); rect.pivot = new Vector2(0,1);
                rect.anchoredPosition = new Vector2(i % 3 * 123,-i / 3 * 118); rect.sizeDelta = new Vector2(113,105);
                bool found = app.Progress.found[i];
                card.Bind(found ? ProductCatalog.All[i].Name : "?",() => app.Notify(ProductCatalog.All[id].Name),found);
                if (found)
                {
                    var holder = new GameObject("Product",typeof(RectTransform)).GetComponent<RectTransform>(); holder.SetParent(card.transform,false);
                    holder.anchoredPosition = new Vector2(0,16); holder.sizeDelta = new Vector2(35,47);
                    var item = Instantiate(assets.item,holder); item.Bind(new ItemState { type = i },assets,null,false); item.Place(holder,holder,false);
                    var text = card.GetComponentInChildren<TextMeshProUGUI>(); text.alignment = TextAlignmentOptions.Bottom; text.fontSize = 13;
                }
            }
            content.sizeDelta = new Vector2(369,Mathf.CeilToInt(ProductCatalog.All.Length / 3f) * 118);
        }
    }
}
