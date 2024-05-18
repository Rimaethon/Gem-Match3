using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Scripts;
using Scripts.BoosterActions;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Create ItemDatabase")]
public class ItemDatabaseSO : SerializedScriptableObject
{
    //Some Effects are defined explicitly because It seemed unnecessary to change whole ItemData architecture for these since every item has one particle effect most of the time. 
    [DictionaryDrawerSettings(KeyLabel = "Item ID", ValueLabel = "Item Data")]
    public readonly GameObject boosterCreationEffect;
    public readonly GameObject missileHitEffect;
    public readonly GameObject missileExplosionEffect;
    public readonly GameObject LightBallLightBallExplosionEffect;
    public readonly GameObject TntRocketMergeParticleEffect;
    public readonly GameObject TntTntExplosionEffect;
    public readonly GameObject starParticleEffect;
    public readonly GameObject mainEventUIEffect;
    public readonly int coinID;
    public readonly  Dictionary<int, ItemData> NormalItems = new Dictionary<int, ItemData>();
    public readonly Dictionary<int, ItemData> Boosters = new Dictionary<int, ItemData>();
    public readonly List<Tuple<int,int,IItemMergeAction>> BoosterMergeAction = new List<Tuple<int, int,IItemMergeAction>>();
    public readonly Dictionary<int, BoardSpriteSaveData> Boards = new Dictionary<int, BoardSpriteSaveData>();
    public readonly Dictionary<int,Sprite> Backgrounds = new Dictionary<int, Sprite>();
    
    
    [Button("Set Item ID's to Prefabs")]
    public void Initialize()
    {
        foreach (KeyValuePair<int,ItemData> item in NormalItems)
        {
            if(item.Value.ItemPrefab!=null)
                item.Value.ItemPrefab.GetComponent<IItem>().ItemID = item.Key;
            if (item.Value.ItemParticleEffect != null)
                item.Value.ItemParticleEffect.GetComponent<ItemParticleEffect>().ItemID = item.Key;
     
        }
        foreach (KeyValuePair<int,ItemData> item in Boosters)
        {
            if(item.Value.ItemPrefab!=null&&item.Value.ItemPrefab.GetComponent<IItem>()!=null)
                item.Value.ItemPrefab.GetComponent<IItem>().ItemID = item.Key;
            if (item.Value.ItemParticleEffect != null)
                item.Value.ItemParticleEffect.GetComponent<BoosterParticleEffect>().ItemID = item.Key;
        }
        
        Debug.Log("Item ID's are set.");
    }
    public GameObject GetNormalItem(int id)
    {
        return NormalItems[id].ItemPrefab;
    }
    public GameObject GetNormalItemParticleEffect(int id)
    {
        return NormalItems[id].ItemParticleEffect;
    }
    public IItemAction GetNormalItemAction(int id)
    {
        if (NormalItems.ContainsKey(id))
        {
            return NormalItems[id].ItemAction;
            
        }
        if(Boosters.ContainsKey(id))
        {
            return Boosters[id].ItemAction;
        }
        return null;
     
    }
    public IItemAction GetBoosterItemAction(int id)
    {
        return Boosters[id].ItemAction;
    }
    public GameObject GetBooster(int id)
    {
        return Boosters[id].ItemPrefab;
    }
    public GameObject GetBoosterParticleEffect(int id)
    {
        return Boosters[id].ItemParticleEffect; 
    }
    public Sprite GetItemSprite(int id)
    {
        return NormalItems[id].ItemSprite;
    }
    public Sprite GetBoosterSprite(int id)
    {
        return Boosters[id].ItemSprite;
    }
    public GameObject GetBoosterCreationEffect()
    {
        return boosterCreationEffect;
    }

    public BoardSpriteSaveData GetBoardSpriteData(int id)
    {
        return Boards[id];
    }
    
}