using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class ShelfView : MonoBehaviour
    {
        [SerializeField] RectTransform slotsRoot;
        [SerializeField] TextMeshProUGUI status;
        [SerializeField] Image barrier;
        [SerializeField] RectTransform slotPrefab;
        RectTransform[] slots;
        Tween movement;
        bool previouslyBlocked;
        public RectTransform Rect => (RectTransform)transform;
        public RectTransform Slot(int i) => slots[i];
        public void Bind(ShelfState state, int jps, float rowHeight, bool animate, int column, float offset)
        {
            if (slots == null)
            {
                slots = new RectTransform[state.slots.Count];
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = Instantiate(slotPrefab,slotsRoot); slots[i].gameObject.SetActive(true);
                    slots[i].anchorMin = slots[i].anchorMax = new Vector2(0,1); slots[i].pivot = new Vector2(.5f,.5f);
                    slots[i].sizeDelta = new Vector2(108f / jps - 3,rowHeight - 8);
                    slots[i].anchoredPosition = new Vector2((i % jps + .5f) * 108f / jps + 5,-(i / jps + .5f) * rowHeight);
                }
            }
            Rect.sizeDelta = new Vector2(118,state.height * rowHeight - 3);
            Vector2 destination = new Vector2(0,offset);
            if (!animate) Rect.anchoredPosition = destination;
            else if ((Rect.anchoredPosition - destination).sqrMagnitude > .1f)
            {
                movement?.Kill(); float delay = Rect.anchoredPosition.y > 300 ? .32f + column * .08f : .18f;
                movement = Rect.DOAnchorPos(destination,.46f).SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
            }
            status.text = state.locked ? "ЗАВЯЗАНО" : state.boarded ? "ДОСКИ" : state.frozen > 0 ? "ЛЁД · " + state.frozen : state.key >= 0 ? "КЛЮЧ" : "";
            barrier.gameObject.SetActive(!state.Open);
            barrier.color = state.frozen > 0 ? new Color(.55f,.84f,.96f,.75f) : new Color(.65f,.43f,.23f,.8f);
            if (previouslyBlocked && state.Open) ShopMotion.Punch(transform,.1f);
            previouslyBlocked = !state.Open;
            barrier.transform.SetAsLastSibling(); status.transform.SetAsLastSibling();
        }
        public void Fold()
        {
            movement?.Kill();
            transform.DOScaleY(0,.2f).SetEase(Ease.InBack).SetUpdate(true).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }
        void OnDestroy() { movement?.Kill(); }
    }
}
