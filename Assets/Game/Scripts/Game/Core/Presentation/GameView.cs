using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class GameView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI level, progress, coins, instruction;
        [SerializeField] Image progressFill;
        [SerializeField] RectTransform[] columns, baskets;
        [SerializeField] TextMeshProUGUI[] stock;
        [SerializeField] RectTransform itemLayer, effects;
        [SerializeField] ShopButton settings, undo, extraBasket;
        [SerializeField] ShopButton[] boosters;
        [SerializeField] CounterView counter;
        readonly Dictionary<int, ShelfView> shelfViews = new Dictionary<int, ShelfView>();
        readonly Dictionary<int, BasketView> basketViews = new Dictionary<int, BasketView>();
        readonly Dictionary<int, ItemView> itemViews = new Dictionary<int, ItemView>();
        ShopApplication app;
        ShopAssets assets;
        Tween progressTween;
        public void Initialize(ShopApplication application, ShopAssets library)
        {
            app = application; assets = library;
            settings.Bind("II",app.ShowSettings);
            undo.Bind("Отмена",app.Undo);
            extraBasket.Bind("+\n50 монет",app.OfferBasket);
            for (int i = 0; i < boosters.Length; i++) { int index = i; boosters[i].Bind("",() => app.OfferBoost(index)); }
        }
        public void Sync(bool instant = false)
        {
            GameState state = app.Session.State;
            counter.Bind(state.counter.Count,state.capacity);
            level.text = (LevelCatalog.All[state.level].hard ? "СЛОЖНЫЙ" : "УРОВЕНЬ") + "\n" + (state.level + 1);
            progress.text = state.collected + " / " + state.total; coins.text = app.Progress.coins + " м.";
            progressTween?.Kill(); float ratio = (float)state.collected / state.total;
            if (instant) progressFill.fillAmount = ratio;
            else progressTween = progressFill.DOFillAmount(ratio,.4f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
            string[] names = { "Собрать", "Убрать 3", "Смешать" }; int[] unlock = { 6,4,1 };
            for (int i = 0; i < boosters.Length; i++)
            {
                bool available = state.level + 1 >= unlock[i];
                boosters[i].SetLabel(available ? names[i] + "\n" + (app.Progress.boosters[i] > 0 ? app.Progress.boosters[i] + " шт." : "30 м.") : "С уровня " + unlock[i]);
                boosters[i].SetAvailable(available && !app.Session.Won);
            }
            undo.SetLabel("Отмена\n" + app.Session.UndoLeft); undo.SetAvailable(app.Session.CanUndo);
            extraBasket.gameObject.SetActive(state.unlocked < 3 && state.level >= 2);
            var liveShelves = new HashSet<int>();
            for (int c = 0; c < 3; c++)
            {
                stock[c].text = state.columns[c].queue.Count == 0 ? "Всё на полках" : "Запас: " + state.columns[c].queue.Count;
                float offset = 0;
                foreach (ShelfState shelf in state.columns[c].shelves)
                {
                    liveShelves.Add(shelf.id); bool fresh = !shelfViews.TryGetValue(shelf.id,out ShelfView view);
                    if (fresh)
                    {
                        view = Instantiate(assets.shelf,columns[c]); shelfViews.Add(shelf.id,view);
                        view.Rect.anchorMin = view.Rect.anchorMax = view.Rect.pivot = new Vector2(.5f,0);
                        view.Rect.anchoredPosition = new Vector2(0,instant ? offset : 340 + c * 25);
                    }
                    view.Bind(shelf,state.jps,56,!instant,c,offset); offset += shelf.height * 56;
                }
            }
            var liveBaskets = new HashSet<int>();
            for (int i = 0; i < 3; i++)
            {
                BasketState basket = state.baskets[i]; if (basket == null) continue;
                liveBaskets.Add(basket.id);
                if (!basketViews.TryGetValue(basket.id,out BasketView view))
                {
                    view = Instantiate(assets.basket,baskets[i]); basketViews.Add(basket.id,view);
                    ((RectTransform)view.transform).anchoredPosition = Vector2.zero;
                    if (!instant) ShopMotion.Enter(view.transform,i * .07f);
                }
                view.Bind(basket,assets,effects);
            }
            var wanted = new HashSet<int>();
            foreach (ColumnState col in state.columns) foreach (ShelfState shelf in col.shelves)
                for (int i = 0; i < shelf.slots.Count; i++)
                {
                    ItemState item = shelf.slots[i]; if (item == null) continue;
                    int index = i; ShelfState current = shelf;
                    Place(item,shelfViews[shelf.id].Slot(i),() => app.Take(current,index),shelf.Open,wanted,instant);
                }
            for (int i = 0; i < state.counter.Count; i++) Place(state.counter[i],counter.Slot(i),null,false,wanted,instant);
            foreach (BasketState basket in state.baskets) if (basket != null)
                for (int i = 0; i < basket.items.Count; i++) Place(basket.items[i],basketViews[basket.id].Slot(i),null,false,wanted,instant);
            foreach (int id in itemViews.Keys.Where(id => !wanted.Contains(id)).ToArray())
            {
                ItemView item = itemViews[id]; itemViews.Remove(id);
                // Packed goods leave with their basket; stockroom returns fly away separately.
                if (item.GetComponentInParent<BasketView>() == null) { item.Detach(itemLayer); item.Retire(); }
            }
            foreach (int id in shelfViews.Keys.Where(id => !liveShelves.Contains(id)).ToArray())
            { shelfViews[id].Fold(); shelfViews.Remove(id); }
            foreach (int id in basketViews.Keys.Where(id => !liveBaskets.Contains(id)).ToArray())
            { basketViews[id].Ship(); basketViews.Remove(id); }
            instruction.text = state.level == 0 ? "Товары идут в корзины по три. Остальные ждут на прилавке." : "Пустая полка складывается. Берегите место на прилавке.";
        }
        void Place(ItemState item, RectTransform slot, Action action, bool interactable, HashSet<int> wanted, bool instant)
        {
            wanted.Add(item.id); bool fresh = !itemViews.TryGetValue(item.id,out ItemView view);
            if (fresh) { view = Instantiate(assets.item,slot); itemViews.Add(item.id,view); }
            view.Bind(item,assets,action,interactable);
            view.Place(slot,itemLayer,!fresh && !instant);
            if (fresh && !instant) ShopMotion.Punch(view.transform,.08f);
        }
        public void Reject() => counter.Shake();
        void OnDestroy() { progressTween?.Kill(); }
    }
}
