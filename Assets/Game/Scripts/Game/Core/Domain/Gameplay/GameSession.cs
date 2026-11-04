using System;
using System.Collections.Generic;
using System.Linq;

namespace RosiesCornerShop.Core
{
    public sealed class GameSession
    {
        GameState state, undo;
        readonly GameProgress progress;
        int undoLeft;
        bool won, lost;
        public float Clock { get; private set; }
        public GameState State => state;
        public bool Won => won;
        public bool Lost => lost;
        public bool CanUndo => undo != null && undoLeft > 0 && !won;
        public int UndoLeft => undoLeft;
        public event Action Changed;
        public event Action ProgressChanged;
        public GameFailure LastFailure { get; private set; }

        public GameSession( GameProgress progress )
        {
            this.progress = progress ?? throw new ArgumentNullException( nameof(progress) );
        }

        void ProgressUpdated()
        {
            ProgressChanged?.Invoke();
        }

        bool Fail( GameFailure reason )
        {
            LastFailure = reason;
            return false;
        }

        void Publish()
        {
            LastFailure = GameFailure.None;
            Changed?.Invoke();
        }

        public bool UnlockBasket()
        {
            if ( state == null || won || state.unlocked == 3 || state.level < 2 ) return Fail( GameFailure.Unavailable );
            if ( progress.coins < GameRules.BasketCost ) return Fail( GameFailure.NoCoins );
            Snapshot();
            progress.coins -= GameRules.BasketCost;
            state.unlocked = 3;
            Refill();
            lost = false;
            ProgressUpdated();
            Publish();
            return true;
        }

        void GrantGifts( int level )
        {
            int[] unlocks = { 6, 4, 1 };
            for ( int i = 0; i < 3; i++ )
                if ( level + 1 >= unlocks[ i ] && !progress.gifts[ i ] )
                {
                    progress.gifts[ i ] = true;
                    progress.boosters[ i ] += 2;
                }

            ProgressUpdated();
        }

        float Rand()
        {
            uint x = state.random;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state.random = x;
            return ( x & 0xffffff ) / 16777216f;
        }

        void Shuffle< T >( List< T > list )
        {
            for ( int i = list.Count - 1; i > 0; i-- )
            {
                int j = (int)( Rand() * ( i + 1 ) );
                T v = list[ i ];
                list[ i ] = list[ j ];
                list[ j ] = v;
            }
        }

        public void StartLevel( int index )
        {
            if ( index < 0 || index >= LevelCatalog.All.Length ) throw new ArgumentOutOfRangeException( nameof(index) );
            LevelDefinition l = LevelCatalog.All[ index ];
            state = new GameState
            {
                level = index,
                jps = l.jps,
                visible = l.visible,
                total = l.types * l.per,
                random = (uint)l.seed,
                freezes = l.freeze
            };
            for ( int c = 0; c < 3; c++ ) state.columns.Add( new ColumnState() );
            var types = ProductSelector.Pick( l, progress );
            var stock = new List< ItemState >();
            foreach ( int t in types )
            {
                for ( int k = 0; k < l.per; k++ ) stock.Add( new ItemState { id = state.nextId++, type = t } );
                for ( int k = 0; k < l.per / GameRules.PackSize; k++ ) state.orders.Add( t );
            }

            Shuffle( stock );
            Shuffle( state.orders );
            for ( int i = 0; i < stock.Count; i++ ) state.columns[ i % 3 ].queue.Add( stock[ i ] );
            Fill();
            if ( l.locks > 0 )
            {
                ShelfState target = state.columns[ 0 ].shelves.Last();
                target.locked = true;
                target.frozen = 0;
                state.columns[ 1 ].shelves[ 0 ].key = target.id;
            }

            if ( l.board )
            {
                ShelfState target = state.columns[ 2 ].shelves.Last();
                target.boarded = true;
                target.frozen = 0;
                ShelfState shelf = state.columns[ 1 ].shelves[ 0 ];
                state.columns[ 1 ].queue.Insert( 0, shelf.slots[ 0 ] );
                shelf.slots[ 0 ] = new ItemState { id = state.nextId++, hammer = true, target = target.id };
            }

            Refill();
            won = lost = false;
            undo = null;
            undoLeft = index >= 1 ? 2 : 0;
            GrantGifts( index );
            Publish();
        }

        void Fill()
        {
            LevelDefinition l = LevelCatalog.All[ state.level ];
            foreach ( ColumnState col in state.columns )
            {
                int used = col.shelves.Sum( b => b.height );
                while ( col.queue.Count > 0 && used < state.visible )
                {
                    int[] sizes = l.sizes.Where( h => h <= state.visible - used ).ToArray();
                    int height = sizes[ (int)( Rand() * sizes.Length ) ];
                    ShelfState b = new ShelfState { id = state.nextId++, height = height };
                    for ( int i = 0; i < height * state.jps; i++ )
                    {
                        ItemState item = null;
                        if ( col.queue.Count > 0 )
                        {
                            item = col.queue[ 0 ];
                            col.queue.RemoveAt( 0 );
                            if ( !item.hammer )
                            {
                                item.wrapped = l.wrap && Rand() < .25f;
                                if ( l.deep && col.queue.Count > 0 && !col.queue[ 0 ].hammer && Rand() < .25f )
                                {
                                    item.back = col.queue[ 0 ];
                                    col.queue.RemoveAt( 0 );
                                }
                            }
                        }

                        b.slots.Add( item );
                    }

                    b.height = Math.Max( 1, b.slots.FindLastIndex( j => j != null ) / state.jps + 1 );
                    b.slots.RemoveRange( b.height * state.jps, b.slots.Count - b.height * state.jps );
                    if ( col.shelves.Count > 0 && state.freezes > 0 && Rand() < .34f )
                    {
                        b.frozen = 3;
                        state.freezes--;
                    }

                    col.shelves.Add( b );
                    used += b.height;
                }
            }
        }

        IEnumerable< ShelfState > Shelves() => state.columns.SelectMany( c => c.shelves );
        BasketState Match( int type ) => state.baskets.FirstOrDefault( b => b != null && b.count < GameRules.PackSize && b.type == type );

        IEnumerable< ItemState > Stock()
        {
            foreach ( ItemState j in state.counter ) yield return j;
            foreach ( ColumnState c in state.columns )
            {
                foreach ( ItemState j in c.queue )
                {
                    yield return j;
                    if ( j.back != null ) yield return j.back;
                }

                foreach ( ShelfState b in c.shelves )
                    foreach ( ItemState j in b.slots )
                        if ( j != null )
                        {
                            yield return j;
                            if ( j.back != null ) yield return j.back;
                        }
            }
        }

        void Refill()
        {
            for ( int i = 0; i < state.unlocked; i++ )
            {
                if ( state.baskets[ i ] != null ) continue;
                var active = state.baskets.Where( b => b != null ).Select( b => b.type ).ToList();
                var options = Stock().Where( j => !j.hammer ).GroupBy( j => j.type ).Where( g => g.Count() >= GameRules.PackSize && !active.Contains( g.Key ) ).Select( g => g.Key ).ToList();
                if ( options.Count == 0 ) continue;
                int type = state.counter.GroupBy( j => j.type ).Where( g => options.Contains( g.Key ) ).OrderByDescending( g => g.Count() ).Select( g => g.Key ).DefaultIfEmpty( -1 ).First();
                if ( type < 0 ) type = state.orders.Where( t => options.Contains( t ) ).DefaultIfEmpty( options[ 0 ] ).First();
                state.orders.Remove( type );
                BasketState basket = new BasketState { id = state.nextId++, type = type };
                state.baskets[ i ] = basket;
                for ( int k = 0; k < state.counter.Count && basket.count < GameRules.PackSize; )
                    if ( state.counter[ k ].type == type )
                    {
                        basket.Add( state.counter[ k ] );
                        state.counter.RemoveAt( k );
                    }
                    else k++;

                if ( basket.count == GameRules.PackSize ) basket.readyAt = Clock + 1.1f;
            }
        }

        static ItemState CopyItem( ItemState j ) =>
            j == null
                ? null
                : new ItemState
                {
                    id = j.id,
                    type = j.type,
                    target = j.target,
                    wrapped = j.wrapped,
                    hammer = j.hammer,
                    back = CopyItem( j.back )
                };

        static GameState CopyState( GameState s )
        {
            var copy = new GameState
            {
                level = s.level,
                jps = s.jps,
                visible = s.visible,
                total = s.total,
                collected = s.collected,
                moves = s.moves,
                peak = s.peak,
                nextId = s.nextId,
                capacity = s.capacity,
                unlocked = s.unlocked,
                freezes = s.freezes,
                random = s.random,
                orders = new List< int >( s.orders ),
                counter = s.counter.Select( CopyItem ).ToList()
            };
            foreach ( ColumnState c in s.columns )
            {
                var col = new ColumnState { queue = c.queue.Select( CopyItem ).ToList() };
                foreach ( ShelfState b in c.shelves )
                    col.shelves.Add( new ShelfState
                    {
                        id = b.id,
                        height = b.height,
                        key = b.key,
                        frozen = b.frozen,
                        locked = b.locked,
                        boarded = b.boarded,
                        slots = b.slots.Select( CopyItem ).ToList()
                    } );
                copy.columns.Add( col );
            }

            for ( int i = 0; i < 3; i++ )
            {
                BasketState b = s.baskets[ i ];
                if ( b != null ) copy.baskets[ i ] = new BasketState { id = b.id, type = b.type, items = b.items.Select( CopyItem ).ToList(), readyAt = b.readyAt };
            }

            return copy;
        }

        void Snapshot()
        {
            undo = CopyState( state );
        }

        void Clean()
        {
            foreach ( ColumnState c in state.columns )
                for ( int i = c.shelves.Count - 1; i >= 0; i-- )
                {
                    ShelfState b = c.shelves[ i ];
                    if ( b.slots.Any( j => j != null ) ) continue;
                    if ( b.key >= 0 )
                    {
                        ShelfState target = Shelves().FirstOrDefault( s => s.id == b.key );
                        if ( target != null ) target.locked = false;
                    }

                    c.shelves.RemoveAt( i );
                }

            Fill();
        }

        public bool Take( int shelfId, int index )
        {
            if ( state == null || won || lost ) return Fail( GameFailure.Unavailable );
            ShelfState shelf = Shelves().FirstOrDefault( s => s.id == shelfId );
            if ( shelf == null || index < 0 || index >= shelf.slots.Count || !shelf.Open ) return Fail( GameFailure.Unavailable );
            ItemState item = shelf.slots[ index ];
            if ( item == null ) return Fail( GameFailure.Unavailable );
            BasketState basket = item.hammer ? null : Match( item.type );
            if ( !item.hammer && basket == null && state.counter.Count >= state.capacity ) return Fail( GameFailure.CounterFull );
            Snapshot();
            state.moves++;
            shelf.slots[ index ] = item.back;
            if ( item.hammer )
            {
                ShelfState target = Shelves().FirstOrDefault( b => b.id == item.target );
                if ( target != null ) target.boarded = false;
            }
            else
            {
                if ( basket != null )
                {
                    basket.Add( item );
                    if ( basket.count == GameRules.PackSize ) basket.readyAt = Clock + 1.1f;
                }
                else
                {
                    state.counter.Add( new ItemState { id = item.id, type = item.type } );
                    state.peak = Math.Max( state.peak, state.counter.Count );
                }
            }

            int row = index / state.jps;
            for ( int k = row * state.jps; k < ( row + 1 ) * state.jps; k++ )
                if ( shelf.slots[ k ] != null )
                    shelf.slots[ k ].wrapped = false;
            Clean();
            Publish();
            return true;
        }

        public void Tick( float now )
        {
            Clock = now;
            if ( state == null || won ) return;
            bool wasWon = won, wasLost = lost;
            bool shipped = false;
            for ( int i = 0; i < state.unlocked; i++ )
            {
                BasketState b = state.baskets[ i ];
                if ( b == null || b.count < GameRules.PackSize || Clock < b.readyAt ) continue;
                state.collected += GameRules.PackSize;
                progress.found[ b.type ] = true;
                state.baskets[ i ] = null;
                ProgressUpdated();
                shipped = true;
                foreach ( ShelfState shelf in Shelves() )
                    if ( shelf.frozen > 0 )
                        shelf.frozen--;
            }

            if ( shipped )
            {
                Refill();
            }

            if ( state.collected == state.total )
            {
                won = true;
                lost = false;
                int stars = Stars();
                progress.stars[ state.level ] = Math.Max( stars, progress.stars[ state.level ] );
                progress.coins += Reward();
                ProgressUpdated();
            }
            else
            {
                bool ready = state.baskets.Any( b => b != null && b.count == GameRules.PackSize );
                bool canTake = Shelves().Any( b => b.Open && b.slots.Any( j => j != null && ( j.hammer || state.counter.Count < state.capacity || Match( j.type ) != null ) ) );
                lost = !ready && !canTake;
            }

            if ( shipped || wasWon != won || wasLost != lost ) Publish();
        }

        public int Stars()
        {
            int t = state.level < 3 ? 2 : 3;
            return state.peak <= t ? 3 : state.peak == t + 1 ? 2 : 1;
        }

        public int Reward() => Stars() * 10 * ( LevelCatalog.All[ state.level ].hard ? 2 : 1 );

        public bool Undo()
        {
            if ( !CanUndo ) return Fail( GameFailure.UndoUnavailable );
            state = undo;
            undo = null;
            lost = false;
            undoLeft--;
            foreach ( BasketState b in state.baskets )
                if ( b != null && b.count == GameRules.PackSize )
                    b.readyAt = Clock + .55f;
            Publish();
            return true;
        }

        public bool Boost( int kind )
        {
            if ( kind < 0 || kind > 2 || state == null || state.level + 1 < GameRules.BoosterUnlockLevel( kind ) ) return Fail( GameFailure.Unavailable );
            if ( won ) return Fail( GameFailure.Unavailable );
            if ( kind == 1 && state.counter.Count == 0 ) return Fail( GameFailure.EmptyCounter );
            if ( progress.boosters[ kind ] <= 0 && progress.coins < GameRules.BoosterCost ) return Fail( GameFailure.NoCoins );
            GameState previousUndo = undo;
            Snapshot();
            bool changed = false;
            if ( kind == 0 )
            {
                BasketState basket = state.baskets.Where( b => b != null && b.count < GameRules.PackSize ).OrderByDescending( b => b.count ).FirstOrDefault();
                if ( basket != null )
                {
                    foreach ( ColumnState col in state.columns )
                    {
                        foreach ( ShelfState shelf in col.shelves )
                            for ( int i = 0; i < shelf.slots.Count && basket.count < GameRules.PackSize; i++ )
                            {
                                ItemState j = shelf.slots[ i ];
                                if ( j != null && !j.hammer && j.type == basket.type )
                                {
                                    shelf.slots[ i ] = j.back;
                                    basket.Add( j );
                                    changed = true;
                                    i--;
                                }
                                else if ( j != null && j.back != null && j.back.type == basket.type )
                                {
                                    basket.Add( j.back );
                                    j.back = null;
                                    changed = true;
                                }
                            }

                        for ( int i = 0; i < col.queue.Count && basket.count < GameRules.PackSize; )
                            if ( !col.queue[ i ].hammer && col.queue[ i ].type == basket.type )
                            {
                                basket.Add( col.queue[ i ] );
                                col.queue.RemoveAt( i );
                                changed = true;
                            }
                            else i++;
                    }

                    for ( int i = 0; i < state.counter.Count && basket.count < GameRules.PackSize; )
                        if ( state.counter[ i ].type == basket.type )
                        {
                            basket.Add( state.counter[ i ] );
                            state.counter.RemoveAt( i );
                            changed = true;
                        }
                        else i++;

                    if ( basket.count == GameRules.PackSize ) basket.readyAt = Clock + 1.1f;
                    if ( changed ) progress.found[ basket.type ] = true;
                }
            }
            else if ( kind == 1 )
            {
                ColumnState col = state.columns.OrderBy( c => c.queue.Count ).First();
                int n = Math.Min( 3, state.counter.Count );
                col.queue.InsertRange( 0, state.counter.Take( n ) );
                state.counter.RemoveRange( 0, n );
                changed = n > 0;
            }
            else
            {
                var spots = new List< (ShelfState shelf, int index) >();
                foreach ( ShelfState b in Shelves() )
                    if ( b.Open )
                        for ( int i = 0; i < b.slots.Count; i++ )
                            if ( b.slots[ i ] != null && !b.slots[ i ].hammer )
                                spots.Add( ( b, i ) );
                var items = spots.Select( p => p.shelf.slots[ p.index ] ).ToList();
                if ( items.Count > 1 )
                {
                    Shuffle( items );
                    for ( int i = 0; i < spots.Count; i++ ) spots[ i ].shelf.slots[ spots[ i ].index ] = items[ i ];
                    changed = true;
                }
            }

            if ( !changed )
            {
                undo = previousUndo;
                return Fail( GameFailure.NoEffect );
            }

            if ( progress.boosters[ kind ] > 0 ) progress.boosters[ kind ]--;
            else progress.coins -= GameRules.BoosterCost;
            state.moves++;
            Clean();
            lost = false;
            ProgressUpdated();
            Publish();
            return true;
        }
    }
}
