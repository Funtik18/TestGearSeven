namespace RosiesCornerShop.Core
{
    public static class GameRules
    {
        public const int BoosterCost = 30;
        public const int BasketCost = 50;
        public const int PackSize = 3;
        public static int BoosterUnlockLevel( int index ) => index == 0 ? 6 : index == 1 ? 4 : 1;
    }
}
