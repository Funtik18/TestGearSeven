using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    public sealed class GamePresentation
    {
        public string Level { get; }
        public string Progress { get; }
        public string Coins { get; }
        public string Instruction { get; }
        public float Completion { get; }
        public int CounterCapacity { get; }
        public bool CanUndo { get; }
        public string UndoLabel { get; }
        public bool ExtraBasketVisible { get; }
        public IReadOnlyList< string > StockLabels { get; }
        public IReadOnlyList< string > BoosterLabels { get; }
        public IReadOnlyList< bool > BoosterAvailable { get; }
        public IReadOnlyList< ShelfPresentation > Shelves { get; }
        public IReadOnlyList< BasketPresentation > Baskets { get; }
        public IReadOnlyList< ItemPresentation > Counter { get; }

        public GamePresentation( string level, string progress, string coins, string instruction, float completion, int counterCapacity, bool canUndo, string undoLabel, bool extraBasketVisible, IReadOnlyList< string > stockLabels, IReadOnlyList< string > boosterLabels, IReadOnlyList< bool > boosterAvailable, IReadOnlyList< ShelfPresentation > shelves, IReadOnlyList< BasketPresentation > baskets, IReadOnlyList< ItemPresentation > counter )
        {
            Level = level;
            Progress = progress;
            Coins = coins;
            Instruction = instruction;
            Completion = completion;
            CounterCapacity = counterCapacity;
            CanUndo = canUndo;
            UndoLabel = undoLabel;
            ExtraBasketVisible = extraBasketVisible;
            StockLabels = stockLabels;
            BoosterLabels = boosterLabels;
            BoosterAvailable = boosterAvailable;
            Shelves = shelves;
            Baskets = baskets;
            Counter = counter;
        }
    }
}
