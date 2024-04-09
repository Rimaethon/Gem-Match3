using System.Collections.Generic;

namespace _Scripts.Data_Classes
{
    public class GoalSaveData
    {
        public int[] GoalIDs;
        public int[] GoalCounts;
        public GoalSaveData(int[] goalIDs, int[] goalCounts)
        {
            GoalIDs = goalIDs;
            GoalCounts = goalCounts;
        }
    }
}