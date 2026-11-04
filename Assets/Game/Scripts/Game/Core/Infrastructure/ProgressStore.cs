using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ProgressStore
    {
        const string Key = "RosiesCornerShop.Core.v2";

        public GameProgress Load()
        {
            GameProgress value = null;
            try
            {
                value = JsonUtility.FromJson< GameProgress >( PlayerPrefs.GetString( Key, PlayerPrefs.GetString( "RosiesCornerShop.Unity.v1", "" ) ) );
            }
            catch ( ArgumentException )
            {
            }

            if ( value == null ) value = new GameProgress();
            if ( value.stars == null || value.stars.Length != 20 ) value.stars = new int[20];
            if ( value.found == null || value.found.Length != ProductCatalog.All.Length ) value.found = new bool[ProductCatalog.All.Length];
            if ( value.boosters == null || value.boosters.Length != 3 ) value.boosters = new int[3];
            if ( value.gifts == null || value.gifts.Length != 4 ) value.gifts = new bool[4];
            value.coins = Mathf.Max( 0, value.coins );
            return value;
        }

        public void Save( GameProgress value )
        {
            PlayerPrefs.SetString( Key, JsonUtility.ToJson( value ) );
            PlayerPrefs.Save();
        }
    }
}
