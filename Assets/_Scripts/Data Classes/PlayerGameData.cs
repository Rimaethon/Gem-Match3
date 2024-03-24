using System;
using Sirenix.Serialization;

namespace Data
{
    // To make it distinct from aws functions I used a different name
    [Serializable]
    public class PlayerGameData
    { 
        [OdinSerialize]
        public int Level { get; set; }
        [OdinSerialize]
        public int Coins { get; set; }
        [OdinSerialize]
        public int Stars { get; set; }
        [OdinSerialize]
        public int[] PowerUps { get; set; }
    }
}