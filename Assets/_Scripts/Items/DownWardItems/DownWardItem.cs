using Scripts;
using Scripts.DownWardItems;
using UnityEngine;

    public class DownWardItem :BoardItemBase
    {
          private DownWardItemAction _action;
          protected override void Awake()
          {
              base.Awake();
              isFallAble = true;
              _isMatchable = false;
              _isSwappable = true;
              _isExplodeAbleByNearMatches = false;
              _isGeneratorItem = false;
          }

		  public override void OnExplode()
          {

          }

		  public override void OnClick(Board board, Vector2Int pos)
          {


          }
    }
