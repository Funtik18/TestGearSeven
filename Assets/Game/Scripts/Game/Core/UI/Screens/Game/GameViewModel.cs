using System;
using System.Collections.Generic;
using System.Linq;
using SoosvetGames.VVM;

namespace RosiesCornerShop.Core
{
    public sealed class GameViewModel : ViewModel< GameView >
    {
        private readonly GameSession _session;
        private readonly GameProgress _progress;
        private readonly ShopUIManager _ui;
        private readonly ShopAssets _assets;
        private readonly ShopAudio _audio;
        private GameBoardViewModel _boardViewModel;
        private bool _resultShown;
        private float _clock;

        public GameViewModel( GameSession session, GameProgress progress, ShopUIManager ui, ShopAssets assets, ShopAudio audio )
        {
            _session = session ?? throw new ArgumentNullException( nameof(session) );
            _progress = progress ?? throw new ArgumentNullException( nameof(progress) );
            _ui = ui ?? throw new ArgumentNullException( nameof(ui) );
            _assets = assets ?? throw new ArgumentNullException( nameof(assets) );
            _audio = audio ?? throw new ArgumentNullException( nameof(audio) );
            _clock = _session.Clock;
        }

        protected override void SubscribeView()
        {
            base.SubscribeView();
            _session.Changed += Render;
            ModelView.OnSettingsClicked += SettingsClickedHandler;
            ModelView.OnUndoClicked += UndoClickedHandler;
            ModelView.OnExtraBasketClicked += ExtraBasketClickedHandler;
            ModelView.OnBoosterClicked += BoosterClickedHandler;
            ModelView.OnItemClicked += ItemClickedHandler;
        }

        protected override void UnSubscribeView()
        {
            base.UnSubscribeView();
            ModelView.OnSettingsClicked -= SettingsClickedHandler;
            ModelView.OnUndoClicked -= UndoClickedHandler;
            ModelView.OnExtraBasketClicked -= ExtraBasketClickedHandler;
            ModelView.OnBoosterClicked -= BoosterClickedHandler;
            ModelView.OnItemClicked -= ItemClickedHandler;
            ModelView.SettingsButton.Unbind();
            ModelView.UndoButton.Unbind();
            ModelView.ExtraBasketButton.Unbind();
            foreach ( var button in ModelView.Boosters ) button.Unbind();
        }

        protected override void OnViewDispose()
        {
            _session.Changed -= Render;
            _boardViewModel?.Dispose();
            _boardViewModel = null;
        }

        protected override void OnViewCreated()
        {
            ModelView.SettingsButton.Bind( "II", ModelView.OnSettingsClick );
            ModelView.UndoButton.Bind( "", ModelView.OnUndoClick );
            ModelView.ExtraBasketButton.Bind( "+\n50 монет", ModelView.OnExtraBasketClick );
            for ( int i = 0; i < ModelView.Boosters.Length; i++ )
            {
                int index = i;
                ModelView.Boosters[ i ].Bind( "", () => ModelView.OnBoosterClick( index ) );
            }

            _boardViewModel = new GameBoardViewModel( ModelView, _assets, ModelView.OnItemClick );
            RenderPresentation( BuildPresentation(), true );
        }

        private void Render()
        {
            if ( ModelView != null ) RenderPresentation( BuildPresentation(), false );
        }

        private void RenderPresentation( GamePresentation presentation, bool instant )
        {
            ModelView.Level.text = presentation.Level;
            ModelView.Progress.text = presentation.Progress;
            ModelView.Coins.text = presentation.Coins;
            ModelView.Instruction.text = presentation.Instruction;
            ModelView.AnimateProgress( presentation.Completion, instant );
            ModelView.Counter.Caption.text = "ПРИЛАВОК · " + presentation.Counter.Count + "/" + presentation.CounterCapacity;
            ModelView.Counter.SetWarning( presentation.Counter.Count >= presentation.CounterCapacity - 1 );
            for ( int i = 0; i < ModelView.Boosters.Length; i++ )
            {
                ModelView.Boosters[ i ].SetLabel( presentation.BoosterLabels[ i ] );
                ModelView.Boosters[ i ].SetAvailable( presentation.BoosterAvailable[ i ] );
            }

            ModelView.UndoButton.SetLabel( presentation.UndoLabel );
            ModelView.UndoButton.SetAvailable( presentation.CanUndo );
            ModelView.ExtraBasketButton.gameObject.SetActive( presentation.ExtraBasketVisible );
            for ( int i = 0; i < ModelView.Stock.Length; i++ ) ModelView.Stock[ i ].text = presentation.StockLabels[ i ];
            _boardViewModel.Render( presentation, instant );
        }

        public void Tick( float deltaTime )
        {
            if ( !IsShowing || _ui.IsDialogOpen ) return;
            int before = _session.State.collected;
            _clock += deltaTime;
            _session.Tick( _clock );
            if ( before != _session.State.collected ) _audio.Play( true, _progress.sound );
            if ( _resultShown ) return;
            if ( _session.Won )
            {
                _resultShown = true;
                ModelView.Celebrate( _assets );
                int level = _session.State.level;
                _ui.OpenDialog( DialogKind.Win, "Лавка сияет!", new string( '★', _session.Stars() ) + "\n\nУпаковано: " + _session.State.total + "\nХодов: " + _session.State.moves + "\nНа прилавке: " + _session.State.peak + "/5\nНаграда: " + _session.Reward() + " монет", new[] { level < LevelCatalog.All.Length - 1 ? "Следующий уровень" : "В лавку", "Переиграть", "В лавку" }, new Action[]
                {
                    () =>
                    {
                        if ( level < LevelCatalog.All.Length - 1 ) _ui.StartLevel( level + 1 );
                        else _ui.ShowHome();
                    },
                    () => _ui.StartLevel( level ), _ui.ShowHome
                } );
            }
            else if ( _session.Lost )
            {
                _resultShown = true;
                _ui.OpenDialog( DialogKind.Lose, "Прилавок переполнен", "Доступных ходов нет. Отмените ход или используйте усилитель.", new[] { "Отмена хода", "Убрать 3 товара", "Собрать корзину", "Заново", "В лавку" }, new Action[] { UndoClickedHandler, () => BoosterClickedHandler( 1 ), () => BoosterClickedHandler( 0 ), () => _ui.StartLevel( _session.State.level ), _ui.ShowHome } );
            }
        }

        public void DialogClosed()
        {
            if ( _session.Lost ) _resultShown = false;
        }

        private void SettingsClickedHandler() => _ui.ShowSettings();

        private void ItemClickedHandler( int shelfId, int slot )
        {
            if ( _ui.IsDialogOpen ) return;
            if ( _session.Take( shelfId, slot ) ) _audio.Play( false, _progress.sound );
            else
            {
                ModelView.Reject();
                ShowFailure();
            }
        }

        private void UndoClickedHandler()
        {
            if ( !_session.Undo() )
            {
                ShowFailure();
                return;
            }

            _resultShown = false;
            _ui.CloseDialog( true );
        }

        private void BoosterClickedHandler( int index )
        {
            int required = GameRules.BoosterUnlockLevel( index );
            if ( _session.State.level + 1 < required )
            {
                _ui.Notify( "Усилитель откроется на уровне " + required );
                return;
            }

            if ( _progress.boosters[ index ] > 0 )
            {
                UseBoost( index );
                return;
            }

            string[] names = { "Собрать корзину", "Убрать три товара", "Перемешать товары" };
            _ui.OpenDialog( DialogKind.Purchase, names[ index ], "Стоимость: " + GameRules.BoosterCost + " монет\nВаш баланс: " + _progress.coins, new[] { "Купить и применить", "Назад" }, new Action[] { () => UseBoost( index ), () => _ui.CloseDialog() } );
        }

        private void UseBoost( int index )
        {
            if ( !_session.Boost( index ) )
            {
                ShowFailure();
                return;
            }

            _resultShown = false;
            _ui.CloseDialog( true );
            _audio.Play( false, _progress.sound );
        }

        private void ExtraBasketClickedHandler()
        {
            _ui.OpenDialog( DialogKind.Purchase, "Ещё одна корзина", "Третья корзина до конца этого уровня.\nСтоимость: " + GameRules.BasketCost + " монет", new[] { "Открыть корзину", "Назад" }, new Action[]
            {
                () =>
                {
                    if ( _session.UnlockBasket() )
                    {
                        _resultShown = false;
                        _ui.CloseDialog( true );
                    }
                    else ShowFailure();
                },
                () => _ui.CloseDialog()
            } );
        }

        private void ShowFailure()
        {
            string message;
            switch ( _session.LastFailure )
            {
                case GameFailure.CounterFull: message = "Прилавок полон. Выберите товар для корзины!"; break;
                case GameFailure.EmptyCounter: message = "Прилавок уже пуст"; break;
                case GameFailure.NoCoins: message = "Не хватает монет"; break;
                case GameFailure.NoEffect: message = "Сейчас усилитель не нужен"; break;
                case GameFailure.UndoUnavailable: message = "Отмена недоступна. Со второго уровня — две отмены на уровень."; break;
                default: message = "Этот ход пока недоступен"; break;
            }

            _ui.Notify( message );
        }

        private GamePresentation BuildPresentation()
        {
            GameState state = _session.State;
            var shelves = new List< ShelfPresentation >();
            for ( int c = 0; c < state.columns.Count; c++ )
            {
                int offset = 0;
                foreach ( var shelf in state.columns[ c ].shelves )
                {
                    var items = shelf.slots.Select( ( item, i ) => Present( item, shelf.Open, shelf.id, i ) ).ToArray();
                    string status = shelf.locked ? "ЗАВЯЗАНО" : shelf.boarded ? "ДОСКИ" : shelf.frozen > 0 ? "ЛЁД · " + shelf.frozen : shelf.key >= 0 ? "КЛЮЧ" : "";
                    shelves.Add( new ShelfPresentation( shelf.id, shelf.height, state.jps, c, offset, status, shelf.Open, shelf.frozen > 0, items ) );
                    offset += shelf.height;
                }
            }

            var baskets = state.baskets.Select( ( b, i ) => b == null ? null : new BasketPresentation( b.id, b.type, i, b.items.Select( j => Present( j ) ).ToArray(), b.count == GameRules.PackSize, b.count + "/" + GameRules.PackSize ) ).Where( b => b != null ).ToArray();
            string[] names = { "Собрать", "Убрать 3", "Смешать" };
            var labels = new string[3];
            var available = new bool[3];
            for ( int i = 0; i < 3; i++ )
            {
                int required = GameRules.BoosterUnlockLevel( i );
                available[ i ] = state.level + 1 >= required && !_session.Won;
                labels[ i ] = available[ i ] ? names[ i ] + "\n" + ( _progress.boosters[ i ] > 0 ? _progress.boosters[ i ] + " шт." : GameRules.BoosterCost + " м." ) : "С уровня " + required;
            }

            return new GamePresentation( ( LevelCatalog.All[ state.level ].hard ? "СЛОЖНЫЙ" : "УРОВЕНЬ" ) + "\n" + ( state.level + 1 ), state.collected + " / " + state.total, _progress.coins + " м.", state.level == 0 ? "Товары идут в корзины по три. Остальные ждут на прилавке." : "Пустая полка складывается. Берегите место на прилавке.", (float)state.collected / state.total, state.capacity, _session.CanUndo, "Отмена\n" + _session.UndoLeft, state.unlocked < 3 && state.level >= 2, state.columns.Select( c => c.queue.Count == 0 ? "Всё на полках" : "Запас: " + c.queue.Count ).ToArray(), labels, available, shelves, baskets, state.counter.Select( j => Present( j ) ).ToArray() );
        }

        private static ItemPresentation Present( ItemState item, bool canTake = false, int shelfId = -1, int slot = -1 )
        {
            return item == null ? null : new ItemPresentation( item.id, item.type, item.back == null ? -1 : item.back.type, item.wrapped, item.hammer, canTake, shelfId, slot );
        }
    }
}
