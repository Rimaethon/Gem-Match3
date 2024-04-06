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
    [DictionaryDrawerSettings(KeyLabel = "Item ID", ValueLabel = "Item Data")]
    public GameObject boosterCreationEffect;
    public GameObject matchEffect;
    public Dictionary<int, ItemData> NormalItems = new Dictionary<int, ItemData>();
    public Dictionary<int, ItemData> Boosters = new Dictionary<int, ItemData>();
    public List<Tuple<int,int,IItemMerge>> BoosterMergeAction = new List<Tuple<int, int,IItemMerge>>();
    public Dictionary<int, BoardSpriteSaveData> Boards = new Dictionary<int, BoardSpriteSaveData>();
    
    
    [Button("Set Item ID's")]
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
            if(item.Value.ItemPrefab!=null)
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
        return NormalItems[id].ItemAction;
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
    public GameObject GetBoosterCreationEffect()
    {
        return boosterCreationEffect;
    }
    public GameObject GetMatchEffect()
    {
        return matchEffect;
    }
    public BoardSpriteSaveData GetBoardSpriteData(int id)
    {
        return Boards[id];
    }
    

}