namespace RosiesCornerShop.Core
{
    public sealed class LevelOptionPresentation
    {
        public int Index { get; }
        public string Label { get; }
        public bool Available { get; }

        public LevelOptionPresentation( int index, string label, bool available )
        {
            Index = index;
            Label = label;
            Available = available;
        }
    }
}
