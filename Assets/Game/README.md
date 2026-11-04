# Rosie's Corner Shop — Unity / C#

Откройте SampleScene и нажмите Play. ShopBootstrap загружает готовый ShopRoot.prefab
из Resources/Rosie через ProjectContext контейнера Zenject.
В ShopRoot установлен GameObjectContext с ShopInstaller из текущего проекта;
Zenject используется из com.soosvet_games.ioc_zenject. ProgressStore, GameProgress,
GameSession и ShopUIManager регистрируются как AsSingle внутри контекста игры.
Зависимости передаются через конструкторы и [Inject]; UI не создаёт DiContainer.
Освобождением ShopUIManager управляет Zenject через IDisposable.
Для портретного интерфейса удобно выбрать 430 × 900 в Game View.
Все надписи используют TextMeshProUGUI; шрифты с кириллицей находятся в Assets/Fonts.

## Структура Core

- Domain — отдельная сборка RosiesCornerShop.Domain с noEngineReferences: true.
  Состояние уровня, генерация, корзины, прилавок, препятствия, усилители и отмена.
  Здесь нет UnityEngine, UI, DOTween, сохранения в PlayerPrefs и текстов интерфейса.
- UI/Screens — пары HomeView/HomeViewModel, GameView/GameViewModel,
  CollectionView/CollectionViewModel.
- UI/Dialogs — WindowView/WindowViewModel для окон настроек, результатов,
  покупки, выбора уровня и обучения.
- UI/Components — пассивные элементы поля и их анимации: товары, полки,
  корзины, прилавок, кнопки и уведомление.
- UI/Presentation — неизменяемые данные отображения. View не получает GameSession,
  GameProgress или изменяемые объекты состояния поля.
- UI/ShopUIManager — создание и переключение экранов и диалогов через
  ViewModelAggregator, ViewCreator и Zenject из установленного com.soosvet_games.vvm.
- Infrastructure — PlayerPrefs/JSON и процедурный звук.
- Bootstrap — сборка зависимостей, Unity lifecycle и safe area.
- Tests/EditMode — пять NUnit-тестов, ссылающихся только на Domain.

Паттерн UI следует примеру
[RPG-fernfall](https://github.com/Funtik18/RPG-fernfall/tree/main/Assets/Game/Scripts/Game/UI):
View сообщает о нажатиях событиями, ViewModel подписывается в SubscribeView,
обращается к модели и обновляет отображение. Подписки снимаются при Dispose.
ViewModel наследуется от ViewModel<T>, экран и окно — от UIView через ShopView.

View содержит сериализованные свойства `[field: SerializeField]`, события ввода
и визуальные эффекты. В View нет Render, создания элементов, обходов данных или
обращений к каталогу. Настройку кнопок и построение списков выполняют ViewModel.
GameBoardViewModel управляет составом поля; ItemViewModel, ShelfViewModel и
BasketViewModel применяют данные к отдельным элементам. Их представления оставляют
у себя только ссылки на компоненты и анимации. Частные поля UI именуются с `_`,
доступ объявляется явно, обработчики нажатий имеют суффикс Handler.

GameSession принимает команды и время через Tick; Unity Time и DOTween ему не нужны.
Изменения поля публикуются событием Changed, изменения прогресса — ProgressChanged.
Сохранение подключает composition root; локализация ошибок находится в UI.
DOTween работает только в UI и анимирует полёты, складывание и падение полок,
упаковку и отправку корзин, окна, кнопки, прогресс, предупреждения и конфетти.

## Префабы

В Assets/Prefabs находятся готовые редактируемые префабы товаров, полок, корзин,
прилавка, шкафа, экранов, окон и эффектов. Ссылки собраны в ShopAssets.asset.
Сохранены GUID скриптов и ссылки существующих префабов. Для экранов и окон
обязателен CanvasGroup. Арт заменяется непосредственно в префабах и Images.

Editor-генераторы, миграции и проверочные меню удалены из игрового проекта.
Прогресс продолжает сохраняться по ключу RosiesCornerShop.Core.v2.

## Тесты

Window → General → Test Runner → EditMode → RosiesCornerShop.Domain.Tests → Run All.

Пять тестов в GameSessionTests.cs проверяют:

1. Товар для заказа попадает в корзину, задний ряд выходит вперёд.
2. Полный прилавок отвергает лишний товар, но разрешает товар для корзины.
3. Отправка корзины размораживает полку; награда за победу начисляется один раз.
4. Отмена восстанавливает удалённую полку с ключом, замок и прилавок.
5. Уборка возвращает три товара без потерь и списывает монеты только при успехе.

Тесты не создают GameObject, не используют кадры/анимации и не меняют сохранения.
