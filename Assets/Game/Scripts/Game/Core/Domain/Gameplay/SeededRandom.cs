using System.Collections.Generic;

namespace RosiesCornerShop.Core
{
    /// <summary>Mulberry32, matching the independent random streams used by the HTML game.</summary>
    public sealed class SeededRandom
    {
        uint state;

        public SeededRandom( uint seed )
        {
            state = seed;
        }

        public float Next()
        {
            unchecked
            {
                uint t = state += 0x6D2B79F5;
                t = ( t ^ t >> 15 ) * ( t | 1 );
                t ^= t + ( t ^ t >> 7 ) * ( t | 61 );
                return ( ( t ^ t >> 14 ) >> 8 ) / 16777216f;
            }
        }

        public void Shuffle< T >( IList< T > list )
        {
            for ( int i = list.Count - 1; i > 0; i-- )
            {
                int j = (int)( Next() * ( i + 1 ) );
                T value = list[ i ];
                list[ i ] = list[ j ];
                list[ j ] = value;
            }
        }
    }
}
