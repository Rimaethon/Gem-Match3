using System;
using System.Collections.Generic;
using Rimaethon.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPool : Singleton<ObjectPool>
{
    
    private Dictionary<int, Stack<GameObject>> items;
    private Dictionary<int, Stack<GameObject>> particleEffects;
    
    [SerializeField] private ItemDatabaseSO itemDatabase;

  

    private void Start()
    {
        items = new Dictionary<int, Stack<GameObject>>();
        particleEffects= new Dictionary<int, Stack<GameObject>>();
        for (int i = 0; i < itemDatabase.BoardElements.Count; i++)
        {
            items[i] = new Stack<GameObject>();
        }
        for (int i = 0; i < itemDatabase.ParticleEffects.Count; i++)
        {
            particleEffects[i] = new Stack<GameObject>();
        }
    }

    public GameObject GetItemFromPool(int itemID, Vector3 position)
    {
      
        if (items[itemID].Count > 0)
        {
            GameObject item = items[itemID].Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        else
        {
            GameObject prefab = Instantiate(itemDatabase.GetBoardElement(itemID), position, Quaternion.identity);
            return prefab;
        }
    }
    
    public GameObject GetParticleEffectFromPool(int itemID, Vector3 position)
    {
        if (particleEffects[itemID].Count > 0)
        {
            GameObject particleEffect = particleEffects[itemID].Pop();
            particleEffect.transform.position = position;
            particleEffect.SetActive(true);
            return particleEffect;
        }
        else
        {
            GameObject prefab = itemDatabase.GetParticleEffect(itemID);
            GameObject effect=Instantiate(prefab, position, prefab.transform.rotation);
            return effect;
        }
    }
   
    public void ReturnParticleEffectToPool(GameObject item, int itemID)
    {
        item.SetActive(false);
        particleEffects[itemID].Push(item);
    }
    public GameObject GetRandomItemFromPool(Vector3 position)
    {
        int randomItemID = Random.Range(0, 4);
        if (items[randomItemID].Count > 0)
        {
            GameObject item = items[randomItemID].Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        else
        {
            GameObject prefab = Instantiate(itemDatabase.GetBoardElement(randomItemID), position, Quaternion.identity);
            return prefab;
        }
    }

    public void ReturnItemToPool(GameObject item, int itemID)
    {
        items[itemID].Push(item);
    }
}