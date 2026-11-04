namespace RosiesCornerShop.Core
{
    public sealed class LevelDefinition
    {
        public int seed, jps, visible, types, per, locks, freeze;
        public int[] sizes;
        public bool hard, board, wrap, deep;
        public string family;

        public LevelDefinition( int seed, int jps, int visible, int types, int per, int[] sizes, bool hard = false, int locks = 0, int freeze = 0, bool board = false, bool wrap = false, bool deep = false )
        {
            this.seed = seed;
            this.jps = jps;
            this.visible = visible;
            this.types = types;
            this.per = per;
            this.sizes = sizes;
            this.hard = hard;
            this.locks = locks;
            this.freeze = freeze;
            this.board = board;
            this.wrap = wrap;
            this.deep = deep;
        }
    }
}
