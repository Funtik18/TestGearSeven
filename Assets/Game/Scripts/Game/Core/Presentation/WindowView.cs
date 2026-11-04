using TMPro;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class WindowView : MonoBehaviour
    {
        [SerializeField] RectTransform panel;
        [SerializeField] CanvasGroup group;
        [SerializeField] TextMeshProUGUI title, description;
        [SerializeField] ShopButton[] actions;
        [SerializeField] RectTransform content;
        public RectTransform Content => content;
        public void Show(string heading, string body, string[] labels, Action[] callbacks)
        {
            title.text = heading; description.text = body;
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].gameObject.SetActive(i < labels.Length);
                if (i < labels.Length) actions[i].Bind(labels[i],callbacks[i]);
            }
            group.alpha = 0; group.DOFade(1,.18f).SetUpdate(true).SetLink(gameObject);
            panel.localScale = Vector3.one * .7f;
            panel.DOScale(1,.4f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
        }
        public void Close()
        {
            group.interactable = false;
            group.DOFade(0,.16f).SetUpdate(true).SetLink(gameObject);
            panel.DOScale(.85f,.16f).SetEase(Ease.InBack).SetUpdate(true).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }
    }
}
