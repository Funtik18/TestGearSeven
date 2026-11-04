using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    public sealed class ShelfPresentation
    {
        public int Id { get; }
        public int Height { get; }
        public int JarsPerRow { get; }
        public int Column { get; }
        public float Offset { get; }
        public string Status { get; }
        public bool Open { get; }
        public bool Frozen { get; }
        public IReadOnlyList< ItemPresentation > Items { get; }

        public ShelfPresentation( int id, int height, int jarsPerRow, int column, float offset, string status, bool open, bool frozen, IReadOnlyList< ItemPresentation > items )
        {
            Id = id;
            Height = height;
            JarsPerRow = jarsPerRow;
            Column = column;
            Offset = offset;
            Status = status;
            Open = open;
            Frozen = frozen;
            Items = items;
        }
    }
}
