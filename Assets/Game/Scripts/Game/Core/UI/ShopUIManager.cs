using System;
using System.Linq;
using SoosvetGames.VVM;
using UnityEngine;
using Zenject;

namespace RosiesCornerShop.Core
{
    public sealed class ShopUIManager : IDisposable
    {
        private readonly ShopAssets _assets;
        private readonly GameSession _session;
        private readonly GameProgress _progress;
        private readonly ProgressStore _store;
        private readonly ToastView _toast;
        private readonly ViewModelAggregator _screens;
        private readonly ViewModelAggregator _dialogs;
        private readonly ViewCreator _dialogCreator;
        private IViewModel _screen;
        private WindowViewModel _dialog;
        private GameViewModel _game;
        public bool IsDialogOpen => _dialog != null;
        public int NextLevel
        {
            get
            {
                int i = Array.FindIndex( _progress.stars, s => s == 0 );
                return i < 0 ? _progress.stars.Length - 1 : i;
            }
        }

        public ShopUIManager(
            DiContainer container,
            ShopAssets assets,
            [ Inject( Id = ShopUILayer.Screens ) ] Transform screenRoot,
            [ Inject( Id = ShopUILayer.Dialogs ) ] Transform dialogRoot,
            GameSession session,
            GameProgress progress,
            ProgressStore store,
            ToastView toast )
        {
            _assets = assets;
            _session = session;
            _progress = progress;
            _store = store;
            _toast = toast;
            _screens = ViewModelAggregator.Create( container, screenRoot, new View[] { assets.home, assets.game, assets.collection } );
            _dialogs = new ViewModelAggregator( container );
            _dialogCreator = new ViewCreator( container, dialogRoot );
        }

        public void Tick( float deltaTime ) => _game?.Tick( deltaTime );
        public void Notify( string message ) => _toast.Show( message );

        public void ShowHome()
        {
            _store.Save( _progress );
            ShowScreen< HomeViewModel >();
        }

        public void ShowCollection() => ShowScreen< CollectionViewModel >();

        private void ShowScreen< T >() where T : class, IViewModel
        {
            CloseDialog( true );
            _screen?.Dispose();
            _game = null;
            _screen = _screens.GetOrCreateIfNotExist< T >();
            _screen.ShowView();
        }

        public void StartLevel( int index )
        {
            CloseDialog( true );
            _screen?.Dispose();
            _game = null;
            _session.StartLevel( index );
            _game = _screens.GetOrCreateIfNotExist< GameViewModel >();
            _screen = _game;
            _game.ShowView();
            ShowTutorial( index );
        }

        public void OpenDialog( DialogKind kind, string title, string body, string[] labels, Action[] commands )
        {
            OpenDialog( kind, new WindowPresentation( title, body, labels ), commands );
        }

        private void OpenDialog( DialogKind kind, WindowPresentation data, Action[] commands )
        {
            CloseDialog( true );
            WindowView prefab;
            switch ( kind )
            {
                case DialogKind.Win: prefab = _assets.winWindow; break;
                case DialogKind.Lose: prefab = _assets.loseWindow; break;
                case DialogKind.Levels: prefab = _assets.levelsWindow; break;
                case DialogKind.Purchase: prefab = _assets.purchaseWindow; break;
                case DialogKind.Tutorial: prefab = _assets.tutorialWindow; break;
                default: prefab = _assets.settingsWindow; break;
            }

            _dialog = (WindowViewModel)_dialogs.Create( typeof(WindowViewModel), new object[] { _dialogCreator, prefab, data, commands, (Action< int >)StartLevel } );
            _dialog.OnDisposed += DialogDisposed;
            _dialog.ShowView();
        }

        private void DialogDisposed( IViewModel model )
        {
            model.OnDisposed -= DialogDisposed;
            if ( _dialog != model ) return;
            _dialog = null;
            _game?.DialogClosed();
        }

        public void CloseDialog( bool immediate = false )
        {
            var current = _dialog;
            if ( current == null ) return;
            if ( immediate ) current.Dispose();
            else current.HideViewAndDispose();
        }

        public void ShowSettings()
        {
            string sound = _progress.sound ? "Звук: включён" : "Звук: выключен";
            bool playing = _game != null;
            OpenDialog( DialogKind.Settings, "Лавка Рози", "Собирайте заказы и держите прилавок свободным.", playing ? new[] { sound, "Продолжить", "Заново", "В лавку" } : new[] { sound, "Закрыть" }, playing ? new Action[] { ToggleSound, () => CloseDialog(), () => StartLevel( _session.State.level ), ShowHome } : new Action[] { ToggleSound, () => CloseDialog() } );
        }

        private void ToggleSound()
        {
            _progress.sound = !_progress.sound;
            _store.Save( _progress );
            ShowSettings();
        }

        public void ShowLevels()
        {
            var options = _progress.stars.Select( ( stars, i ) => new LevelOptionPresentation( i, ( i + 1 ) + " " + new string( '*', stars ), i <= NextLevel ) ).ToArray();
            OpenDialog( DialogKind.Levels, new WindowPresentation( "Выберите уровень", "Сложные уровни дают двойную награду.", new[] { "Закрыть" }, options ), new Action[] { () => CloseDialog() } );
        }

        private void ShowTutorial( int index )
        {
            string body;
            switch ( index )
            {
                case 0: body = "Нажмите товар на полке.\n\nВ каждой корзине — три одинаковых товара. Лишние ждут на прилавке из пяти мест.\n\nПустые полки складываются, открывая запас."; break;
                case 4: body = "Опустошите полку с ключом, чтобы развязать связанную полку."; break;
                case 7: body = "Лёд тает с каждой отправленной корзиной. Через три отправки полка откроется."; break;
                case 10: body = "Найдите молоток на другой полке и нажмите на него, чтобы убрать доски."; break;
                case 12: body = "Пакеты скрывают товары. Нажмите один, чтобы раскрыть соседние пакеты в ряду."; break;
                case 16: body = "За некоторыми товарами есть второй ряд. Заберите передний товар, чтобы открыть задний."; break;
                default: return;
            }

            OpenDialog( DialogKind.Tutorial, index == 0 ? "Добро пожаловать!" : "Новое в лавке", body, new[] { index == 0 ? "Открыть лавку" : "Понятно" }, new Action[] { () => CloseDialog() } );
        }

        public void Dispose()
        {
            _dialogs.Dispose();
            _screens.Dispose();
            _dialog = null;
            _screen = null;
            _game = null;
        }
    }
}
