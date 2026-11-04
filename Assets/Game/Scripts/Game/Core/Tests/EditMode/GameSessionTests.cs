using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RosiesCornerShop.Core.Tests
{
    public sealed class GameSessionTests
    {
        GameProgress progress;
        GameSession session;

        void CreateSession( int level = 1 )
        {
            progress = new GameProgress { coins = 100, gifts = new[] { true, true, true, true } };
            session = new GameSession( progress );
            session.StartLevel( level );
            var state = session.State;
            state.columns.Clear();
            state.columns.Add( new ColumnState() );
            state.orders.Clear();
            state.counter.Clear();
            state.baskets = new BasketState[3];
            state.jps = 2;
            state.visible = 3;
            state.freezes = 0;
            state.total = 0;
            state.nextId = 100;
        }

        ShelfState AddShelf( int height, params ItemState[] items )
        {
            var shelf = new ShelfState { id = session.State.nextId++, height = height, slots = items.ToList() };
            session.State.columns[ 0 ].shelves.Add( shelf );
            session.State.total += items.Sum( Count );
            return shelf;
        }

        void AddBasket( int type, params ItemState[] items )
        {
            session.State.baskets[ 0 ] = new BasketState { id = session.State.nextId++, type = type, items = items.ToList() };
            session.State.total += items.Length;
        }

        static ItemState Item( int id, int type ) => new ItemState { id = id, type = type };
        static int Count( ItemState item ) => item == null ? 0 : ( item.hammer ? 0 : 1 ) + Count( item.back );

        [ Test ]
        public void Take_MatchingItemEntersBasket_AndRearItemMovesForward()
        {
            CreateSession();
            var rear = Item( 2, 1 );
            var front = Item( 1, 0 );
            front.back = rear;
            var shelf = AddShelf( 1, front, null );
            AddShelf( 1, Item( 3, 0 ), Item( 4, 0 ) );
            AddShelf( 1, Item( 5, 1 ), Item( 6, 1 ) );
            AddBasket( 0 );

            Assert.That( session.Take( shelf.id, 0 ), Is.True );

            Assert.That( session.State.baskets[ 0 ].items.Select( i => i.id ), Is.EqualTo( new[] { 1 } ) );
            Assert.That( session.State.baskets[ 0 ].items[ 0 ].back, Is.Null );
            Assert.That( shelf.slots[ 0 ], Is.SameAs( rear ) );
            Assert.That( session.State.counter, Is.Empty );
            Assert.That( session.State.moves, Is.EqualTo( 1 ) );
        }

        [ Test ]
        public void FullCounter_RejectsUnmatchedItem_StillAcceptsBasketItem()
        {
            CreateSession();
            var shelf = AddShelf( 1, Item( 1, 0 ), Item( 2, 2 ) );
            AddBasket( 0 );
            for ( int i = 0; i < 5; i++ ) session.State.counter.Add( Item( 10 + i, 1 ) );
            var counterBefore = session.State.counter.Select( i => i.id ).ToArray();

            Assert.That( session.Take( shelf.id, 1 ), Is.False );
            Assert.That( session.LastFailure, Is.EqualTo( GameFailure.CounterFull ) );
            Assert.That( shelf.slots[ 1 ].id, Is.EqualTo( 2 ) );
            Assert.That( session.State.counter.Select( i => i.id ), Is.EqualTo( counterBefore ) );
            Assert.That( session.State.moves, Is.Zero );

            Assert.That( session.Take( shelf.id, 0 ), Is.True );
            Assert.That( session.State.counter.Count, Is.EqualTo( 5 ) );
            Assert.That( session.State.baskets[ 0 ].count, Is.EqualTo( 1 ) );
        }

        [ Test ]
        public void Shipping_ThawsFrozenShelf_AndAwardsVictoryExactlyOnce()
        {
            CreateSession();
            var source = AddShelf( 1, Item( 1, 0 ), null );
            var frozen = AddShelf( 2, Item( 4, 1 ), Item( 5, 1 ), Item( 6, 1 ), null );
            frozen.frozen = 1;
            AddBasket( 0, Item( 2, 0 ), Item( 3, 0 ) );

            session.Take( source.id, 0 );
            session.Tick( 1.2f );
            Assert.That( frozen.frozen, Is.Zero );
            Assert.That( session.State.collected, Is.EqualTo( 3 ) );
            Assert.That( session.Won, Is.False );
            for ( int i = 0; i < 3; i++ ) Assert.That( session.Take( frozen.id, i ), Is.True );
            session.Tick( 2.4f );
            Assert.That( session.Won, Is.True );
            Assert.That( session.State.collected, Is.EqualTo( 6 ) );
            Assert.That( progress.stars[ 1 ], Is.EqualTo( 3 ) );
            Assert.That( progress.coins, Is.EqualTo( 100 + session.Reward() ) );

            int paidCoins = progress.coins;
            session.Tick( 100 );
            Assert.That( progress.coins, Is.EqualTo( paidCoins ) );
        }

        [ Test ]
        public void Undo_RestoresRemovedKeyShelf_LockAndCounter()
        {
            CreateSession();
            var locked = AddShelf( 1, Item( 1, 0 ), null );
            locked.locked = true;
            var key = AddShelf( 1, Item( 2, 1 ), null );
            key.key = locked.id;
            session.Take( key.id, 0 );
            Assert.That( locked.locked, Is.False );
            Assert.That( session.State.counter.Count, Is.EqualTo( 1 ) );

            Assert.That( session.Undo(), Is.True );

            var shelves = session.State.columns[ 0 ].shelves;
            Assert.That( shelves.Single( s => s.id == locked.id ).locked, Is.True );
            Assert.That( shelves.Single( s => s.id == key.id ).slots[ 0 ].id, Is.EqualTo( 2 ) );
            Assert.That( shelves.Single( s => s.id == key.id ).key, Is.EqualTo( locked.id ) );
            Assert.That( session.State.counter, Is.Empty );
            Assert.That( session.State.moves, Is.Zero );
            Assert.That( session.UndoLeft, Is.EqualTo( 1 ) );
            Assert.That( session.CanUndo, Is.False );
        }

        [ Test ]
        public void Tidy_ReturnsThreeItemsWithoutLoss_ChargesOnlyForSuccessfulUse()
        {
            CreateSession( 3 );
            session.State.counter.AddRange( new[] { Item( 1, 0 ), Item( 2, 1 ), Item( 3, 2 ) } );
            session.State.total = 3;

            Assert.That( session.Boost( 1 ), Is.True );

            var items = session.State.counter.Concat( session.State.columns.SelectMany( c => c.queue ) ).Concat( session.State.columns.SelectMany( c => c.shelves ).SelectMany( s => s.slots ) ).Where( i => i != null ).ToArray();
            Assert.That( session.State.counter, Is.Empty );
            Assert.That( items.Select( i => i.id ), Is.EquivalentTo( new[] { 1, 2, 3 } ) );
            Assert.That( items.Select( i => i.id ).Distinct().Count(), Is.EqualTo( 3 ) );
            Assert.That( progress.coins, Is.EqualTo( 100 - GameRules.BoosterCost ) );

            Assert.That( session.Boost( 1 ), Is.False );
            Assert.That( session.LastFailure, Is.EqualTo( GameFailure.EmptyCounter ) );
            Assert.That( progress.coins, Is.EqualTo( 100 - GameRules.BoosterCost ) );
            Assert.That( session.State.moves, Is.EqualTo( 1 ) );
        }
    }
}
