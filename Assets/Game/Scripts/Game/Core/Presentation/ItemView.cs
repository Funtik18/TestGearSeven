using TMPro;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ItemView : MonoBehaviour
    {
        [SerializeField] Image body, cap, emblem, rear;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] GameObject bag;
        [SerializeField] Button button;
        public int Id { get; private set; }
        public RectTransform Destination { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        Sequence flight;
        bool wasWrapped;
        public void Bind(ItemState item, ShopAssets assets, Action onClick, bool interactable)
        {
            Id = item.id;
            button.onClick.RemoveAllListeners(); button.interactable = interactable;
            if (onClick != null) button.onClick.AddListener(() => onClick());
            var product = item.hammer ? null : ProductCatalog.All[item.type];
            Color color = item.hammer ? ShopColors.Dark : product.Color;
            body.color = color; cap.color = Color.Lerp(color, ShopColors.Dark, .25f);
            string shape = product == null ? "hammer" : product.Shape;
            body.sprite = shape.StartsWith("fresh") || shape == "donut" || shape == "cookie" || shape == "orange" ? assets.circle
                : shape == "cheese" || shape == "cone" ? assets.triangle : assets.pixel;
            emblem.sprite = assets.circle;
            label.text = item.hammer ? "T" : (item.type + 1).ToString();
            label.color = color;
            cap.gameObject.SetActive(body.sprite == assets.pixel || item.hammer);
            rear.gameObject.SetActive(item.back != null);
            if (item.back != null) rear.color = Color.Lerp(ProductCatalog.All[item.back.type].Color, ShopColors.Cream, .3f);
            bag.SetActive(item.wrapped);
            if (wasWrapped && !item.wrapped) ShopMotion.Punch(transform, .16f);
            wasWrapped = item.wrapped;
        }
        public void Place(RectTransform destination, RectTransform flightLayer, bool animate, float delay = 0)
        {
            if (Destination == destination && (flight != null && flight.IsActive() || transform.parent == destination)) return;
            flight?.Kill(); Destination = destination;
            if (!animate) { Attach(destination); return; }
            Vector3 start = transform.position;
            transform.SetParent(flightLayer, true);
            Vector3 end = destination.TransformPoint(destination.rect.center);
            float canvasScale = GetComponentInParent<Canvas>().scaleFactor;
            Vector3 middle = Vector3.Lerp(start, end, .48f) + Vector3.up * 65 * canvasScale;
            Vector2 targetSize = destination.rect.size;
            // Item prefabs use a common 48x64 design; scale to fit the destination slot.
            float targetScale = Mathf.Min(targetSize.x / 48f, targetSize.y / 64f);
            flight = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            flight.AppendInterval(delay);
            flight.Append(Rect.DOPath(new[] { start, middle, end }, .43f, PathType.CatmullRom).SetEase(Ease.InOutSine));
            flight.Join(transform.DOScale(targetScale * 1.12f, .2f).SetLoops(2, LoopType.Yoyo));
            flight.Join(transform.DORotate(new Vector3(0,0,start.x > end.x ? -9 : 9), .2f).SetLoops(2, LoopType.Yoyo));
            flight.OnComplete(() => { if (Destination != null) { Attach(Destination); ShopMotion.Punch(Destination, .1f); } });
        }
        void Attach(RectTransform target)
        {
            transform.SetParent(target, false); Rect.anchorMin = Rect.anchorMax = Rect.pivot = new Vector2(.5f,.5f);
            Rect.anchoredPosition = Vector2.zero; Rect.sizeDelta = new Vector2(48,64);
            float size = Mathf.Min(target.rect.width / 48f, target.rect.height / 64f);
            transform.localScale = Vector3.one * size; transform.localRotation = Quaternion.identity;
        }
        public void Retire()
        {
            flight?.Kill(); button.interactable = false;
            transform.DOScale(0, .23f).SetEase(Ease.InBack).SetUpdate(true).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }
        public void Detach(RectTransform layer) { flight?.Kill(); Destination = null; transform.SetParent(layer, true); }
        void OnDestroy() { flight?.Kill(); }
    }
}
