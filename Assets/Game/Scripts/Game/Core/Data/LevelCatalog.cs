using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosiesCornerShop.Core
{
public static class LevelCatalog
{
    public static readonly LevelDefinition[] All = {
        new LevelDefinition(51013,2,3,3,6,new[]{1}), new LevelDefinition(253292,2,2,4,6,new[]{1}),
        new LevelDefinition(463312,3,2,5,6,new[]{1}), new LevelDefinition(97275,3,5,8,9,new[]{1,1,1,2,2},true),
        new LevelDefinition(140216,3,5,6,9,new[]{1},locks:1), new LevelDefinition(548279,3,5,7,9,new[]{1,2}),
        new LevelDefinition(250616,3,5,6,9,new[]{1,2}), new LevelDefinition(112291,3,5,7,9,new[]{1,2},freeze:2),
        new LevelDefinition(221678,3,5,7,9,new[]{1,2}), new LevelDefinition(183986,3,5,8,9,new[]{1,1,2,2,3},true),
        new LevelDefinition(572049,3,5,6,9,new[]{1,2},board:true), new LevelDefinition(69897,3,5,7,9,new[]{1},locks:1,board:true),
        new LevelDefinition(496665,3,5,7,9,new[]{1},wrap:true), new LevelDefinition(575088,3,5,7,9,new[]{1,2},freeze:1,wrap:true),
        new LevelDefinition(196792,3,5,8,9,new[]{1,2},true,board:true), new LevelDefinition(298438,3,5,6,9,new[]{1,2}),
        new LevelDefinition(283969,3,5,7,9,new[]{1,2},deep:true), new LevelDefinition(168867,3,5,7,9,new[]{1,2,3},freeze:1,deep:true),
        new LevelDefinition(580153,3,5,8,9,new[]{1,1,2},wrap:true), new LevelDefinition(294749,3,5,8,9,new[]{1,1,2,2,3},true,deep:true)
    };
    static LevelCatalog() { All[15].family = "bakery"; All[18].family = "sweet"; }
}
}
