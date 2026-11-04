using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ShopApplication : MonoBehaviour
    {
        [SerializeField] ShopAssets assets;
        [SerializeField] RectTransform safeArea, screenRoot, windowRoot, effects;
        [SerializeField] ToastView toast;
        [SerializeField] ShopAudio audio;
        readonly ProgressStore store = new ProgressStore();
        public GameProgress Progress { get; private set; }
        public GameSession Session { get; private set; }
        public int NextLevel { get { int i = Array.FindIndex(Progress.stars,s => s == 0); return i < 0 ? 19 : i; } }
        GameView game;
        GameObject currentScreen;
        WindowView window;
        string activeScreen;
        bool resultShown;
        Rect previousSafeArea;
        int previousWidth, previousHeight;
        void Awake()
        {
            DOTween.Init(false,true,LogBehaviour.ErrorsOnly);
            Progress = store.Load(); Session = new GameSession(Progress,Save); Session.Message += Notify;
            ApplySafeArea(); ShowHome();
        }
        void Start() { Canvas.ForceUpdateCanvases(); ApplySafeArea(); }
        void Update()
        {
            if (previousSafeArea != Screen.safeArea || previousWidth != Screen.width || previousHeight != Screen.height) ApplySafeArea();
            if (activeScreen != "game" || window != null || Session.State == null) return;
            int before = Session.State.collected; Session.Tick(Time.unscaledTime);
            if (before != Session.State.collected) { game.Sync(); audio.Play(true,Progress.sound); }
            if (Session.Won && !resultShown)
            {
                resultShown = true; int level = Session.State.level;
                ShopMotion.Confetti(assets,effects,safeArea.TransformPoint(new Vector3(0,150)));
                Open(assets.winWindow,"Лавка сияет!",new string('★',Session.Stars()) + "\n\nУпаковано: " + Session.State.total
                    + "\nХодов: " + Session.State.moves + "\nНа прилавке: " + Session.State.peak + "/5\nНаграда: " + Session.Reward() + " монет",
                    new[] { level < 19 ? "Следующий уровень" : "В лавку", "Переиграть", "В лавку" },
                    new Action[] { () => { if (level < 19) StartLevel(level + 1); else ShowHome(); }, () => StartLevel(level), ShowHome });
            }
            else if (Session.Lost && !resultShown)
            {
                resultShown = true;
                Open(assets.loseWindow,"Прилавок переполнен", "Доступных ходов нет. Отмените ход или используйте усилитель.",
                    new[] { "Отмена хода", "Убрать 3 товара", "Собрать корзину", "Заново", "В лавку" },
                    new Action[] { Undo, () => OfferBoost(1), () => OfferBoost(0), () => StartLevel(Session.State.level), ShowHome });
            }
        }
        void ApplySafeArea()
        {
            previousSafeArea = Screen.safeArea; previousWidth = Screen.width; previousHeight = Screen.height;
            if (Screen.width == 0 || Screen.height == 0) return;
            safeArea.anchorMin = safeArea.anchorMax = new Vector2(.5f,.5f);
            Canvas canvas = GetComponent<Canvas>(); float scale = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1;
            float fit = Mathf.Min(previousSafeArea.width / scale / 430f,previousSafeArea.height / scale / 900f);
            safeArea.localScale = Vector3.one * fit;
            safeArea.anchoredPosition = (previousSafeArea.center - new Vector2(Screen.width,Screen.height) * .5f) / scale;
        }
        void ClearScreen(string name)
        {
            CloseWindow(true); if (currentScreen != null) Destroy(currentScreen);
            currentScreen = null; game = null; activeScreen = name; resultShown = false;
            // Flight/confetti objects belong to the outgoing screen and are killed with their GameObjects.
            foreach (Transform child in effects) Destroy(child.gameObject);
        }
        public void ShowHome()
        { Save(); ClearScreen("home"); var view = Instantiate(assets.home,screenRoot); currentScreen = view.gameObject; view.Initialize(this); }
        public void ShowCollection()
        { ClearScreen("collection"); var view = Instantiate(assets.collection,screenRoot); currentScreen = view.gameObject; view.Initialize(this,assets); }
        public void StartLevel(int index)
        {
            ClearScreen("game"); Session.Tick(Time.unscaledTime); Session.StartLevel(Mathf.Clamp(index,0,19));
            game = Instantiate(assets.game,screenRoot); currentScreen = game.gameObject; game.Initialize(this,assets); Canvas.ForceUpdateCanvases(); game.Sync(true);
            if (index == 0) Open(assets.tutorialWindow,"Добро пожаловать!","Нажмите товар на полке.\n\nВ каждой корзине — три одинаковых товара. Лишние ждут на прилавке из пяти мест.\n\nПустые полки складываются, открывая запас.",new[] { "Открыть лавку" },new Action[] { () => CloseWindow() });
            else if (index == 4 || index == 7 || index == 10 || index == 12 || index == 16)
            {
                string body = index == 4 ? "Опустошите полку с ключом, чтобы развязать связанную полку."
                    : index == 7 ? "Лёд тает с каждой отправленной корзиной. Через три отправки полка откроется."
                    : index == 10 ? "Найдите молоток на другой полке и нажмите на него, чтобы убрать доски."
                    : index == 12 ? "Пакеты скрывают товары. Нажмите один, чтобы раскрыть соседние пакеты в ряду."
                    : "За некоторыми товарами есть второй ряд. Заберите передний товар, чтобы открыть задний.";
                Open(assets.tutorialWindow,"Новое в лавке",body,new[] { "Понятно" },new Action[] { () => CloseWindow() });
            }
        }
        public void Take(ShelfState shelf, int slot)
        { if (window != null) return; if (Session.Take(shelf,slot)) { game.Sync(); audio.Play(false,Progress.sound); } else game.Reject(); }
        public void Undo()
        {
            if (!Session.Undo()) { Notify("Отмена недоступна. Со второго уровня — две отмены на уровень."); return; }
            CloseWindow(true); resultShown = false; game.Sync();
        }
        public void OfferBoost(int index)
        {
            int required = index == 0 ? 6 : index == 1 ? 4 : 1;
            if (Session.State.level + 1 < required) { Notify("Усилитель откроется на уровне " + required); return; }
            if (Progress.boosters[index] > 0) { UseBoost(index); return; }
            string[] names = { "Собрать корзину", "Убрать три товара", "Перемешать товары" };
            Open(assets.purchaseWindow,names[index],"Стоимость: 30 монет\nВаш баланс: " + Progress.coins,new[] { "Купить и применить", "Назад" },
                new Action[] { () => UseBoost(index), () => { CloseWindow(); resultShown = false; } });
        }
        void UseBoost(int index)
        { if (!Session.Boost(index)) return; CloseWindow(true); resultShown = false; game.Sync(); audio.Play(false,Progress.sound); }
        public void OfferBasket()
        {
            Open(assets.purchaseWindow,"Ещё одна корзина","Третья корзина до конца этого уровня.\nСтоимость: 50 монет",new[] { "Открыть корзину", "Назад" },
                new Action[] { () => { if (Session.UnlockBasket()) { CloseWindow(true); resultShown = false; game.Sync(); } else Notify("Не хватает монет"); }, () => CloseWindow() });
        }
        public void ShowSettings()
        {
            Open(assets.settingsWindow,"Лавка Рози","Собирайте заказы и держите прилавок свободным.",
                activeScreen == "game" ? new[] { Progress.sound ? "Звук: включён" : "Звук: выключен", "Продолжить", "Заново", "В лавку" }
                    : new[] { Progress.sound ? "Звук: включён" : "Звук: выключен", "Закрыть" },
                activeScreen == "game" ? new Action[] { ToggleSound, () => CloseWindow(), () => StartLevel(Session.State.level), ShowHome }
                    : new Action[] { ToggleSound, () => CloseWindow() });
        }
        void ToggleSound() { Progress.sound = !Progress.sound; Save(); ShowSettings(); }
        public void ShowLevels()
        {
            Open(assets.levelsWindow,"Выберите уровень","Сложные уровни дают двойную награду.",new[] { "Закрыть" },new Action[] { () => CloseWindow() });
            for (int i = 0; i < 20; i++)
            {
                int index = i; ShopButton button = Instantiate(assets.button,window.Content); RectTransform r = (RectTransform)button.transform;
                r.anchorMin = r.anchorMax = new Vector2(0,1); r.pivot = new Vector2(0,1); r.sizeDelta = new Vector2(74,52);
                r.anchoredPosition = new Vector2(i % 4 * 80,-i / 4 * 58);
                button.Bind((i + 1) + " " + new string('*',Progress.stars[i]),() => StartLevel(index),i <= NextLevel);
            }
        }
        void Open(WindowView prefab, string title, string body, string[] labels, Action[] actions)
        { CloseWindow(true); window = Instantiate(prefab,windowRoot); window.Show(title,body,labels,actions); }
        void CloseWindow(bool immediate = false)
        { if (window == null) return; if (immediate) Destroy(window.gameObject); else window.Close(); window = null; }
        public void Notify(string message) { if (toast != null) toast.Show(message); }
        void Save() { if (Progress != null) store.Save(Progress); }
        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() => Save();
        void OnDestroy() { if (Session != null) Session.Message -= Notify; }
    }
}
