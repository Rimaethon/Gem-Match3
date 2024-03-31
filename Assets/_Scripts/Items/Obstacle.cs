using System;
using UnityEngine;

namespace Scripts
{
    public class Obstacle:ItemBase
    {

        protected override void Awake()
        {
            base.Awake();
            _isMovable = false;
            
        }

        public override void OnMatch()
        {
            base.OnMatch();
        }
    }
}