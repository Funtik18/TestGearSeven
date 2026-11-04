using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    public sealed class WindowPresentation
    {
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList< string > Labels { get; }
        public IReadOnlyList< LevelOptionPresentation > Levels { get; }

        public WindowPresentation( string title, string description, IReadOnlyList< string > labels, IReadOnlyList< LevelOptionPresentation > levels = null )
        {
            Title = title;
            Description = description;
            Labels = labels;
            Levels = levels;
        }
    }
}
