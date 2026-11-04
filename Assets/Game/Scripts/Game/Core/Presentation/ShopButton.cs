using TMPro;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ShopButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI caption;
        Tween press;
        public void Bind(string label, Action action, bool available = true)
        {
            caption.text = label; button.interactable = available;
            button.onClick.RemoveAllListeners();
            if (action != null) button.onClick.AddListener(() => action());
        }
        public void SetLabel(string label) => caption.text = label;
        public void SetAvailable(bool value) => button.interactable = value;
        public void OnPointerDown(PointerEventData eventData)
        { if (button.interactable) Scale(.94f, .08f); }
        public void OnPointerUp(PointerEventData eventData) => Scale(1, .24f);
        public void OnPointerExit(PointerEventData eventData) => Scale(1, .2f);
        void Scale(float scale, float duration)
        { press?.Kill(); press = transform.DOScale(scale, duration).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject); }
        void OnDisable() { press?.Kill(); transform.localScale = Vector3.one; }
    }
}
