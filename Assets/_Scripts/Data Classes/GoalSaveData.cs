using System;
using System.Collections.Generic;

namespace _Scripts.Data_Classes
{
    public class GoalSaveData
    {
        public int[] GoalIDs;
        public int[] GoalAmounts;
        public GoalSaveData(int[] goalIDs, int[] goalAmounts)
        {
            GoalIDs = goalIDs;
            GoalAmounts = goalAmounts;
        }
    }
}