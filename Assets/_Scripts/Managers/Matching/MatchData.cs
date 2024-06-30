using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Core
{
    public class MatchData
    {
        public List<Vector2Int> Matches=new List<Vector2Int>();
        public MatchType MatchType=MatchType.None;
        public int matchID;
        public bool IsInitialized;
    }
}
