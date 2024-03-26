using System;
using UnityEngine;

namespace Scripts
{
    public class Obstacle:ItemBase
    {

        private void Awake()
        {
            IsMovable = false;
            IsDraggable = false;
            
        }

        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Obstacle Matched");
        }
    }
}