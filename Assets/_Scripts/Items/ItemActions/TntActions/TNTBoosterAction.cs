using _Scripts.Items.ItemActions;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class TNTBoosterAction : ExplodeAction,IItemAction
    {
        public bool IsFinished => _isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        
        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            _pos = pos;
            var explosionPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            ObjectPool.Instance.GetBoosterParticleEffect(value1, explosionPos, Quaternion.identity);
            
            InitializeExplode(Board,2, _pos);
            AudioManager.Instance.PlaySFX(SFXClips.TNTSound);
            _isFinished = false;
        }

        public void Execute()
        {
            HandleExplosion();
        }

    }
}