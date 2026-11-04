using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ToastView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI caption;
        [SerializeField] CanvasGroup group;
        Sequence sequence;
        public void Show(string message)
        {
            sequence?.Kill(); caption.text = message; group.alpha = 0;
            sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            sequence.Append(group.DOFade(1,.18f)).AppendInterval(2.6f).Append(group.DOFade(0,.3f));
        }
        void OnDestroy() { sequence?.Kill(); }
    }
}
