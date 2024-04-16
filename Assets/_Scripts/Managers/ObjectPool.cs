using System.Collections.Generic;
using Rimaethon.Scripts.Utility;
using Scripts;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private ItemDatabaseSO itemDatabase;
    private readonly Dictionary<int, Stack<GameObject>> _normalItems= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _boosters= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _normalItemParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _boosterParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Stack<GameObject> _boosterCreationEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> _matchEffects = new Stack<GameObject>();
    protected override void Awake()
    {
        base.Awake();
        foreach (int key in itemDatabase.NormalItems.Keys)
        {
            _normalItems.Add(key,new Stack<GameObject>());
            _normalItemParticleEffects.Add(key,new Stack<GameObject>());
        }
        foreach (int key in itemDatabase.Boosters.Keys)
        {
            _boosters.Add(key,new Stack<GameObject>());
            _boosterParticleEffects.Add(key,new Stack<GameObject>());
        }
       
    }
    
    private IItem GetItemFromPool(Dictionary<int, Stack<GameObject>> pool, int itemID, Vector3 position,GameObject prefab,Board board)
    {
        if (pool[itemID].Count > 0)
        {
            IItem item = pool[itemID].Pop().GetComponent<IItem>();
            
            item.Transform.position = position;
            item.Transform.gameObject.SetActive(true);
            item.Board = board;
            return item;
        }
        IItem newItem = Instantiate(prefab, position, prefab.transform.rotation).GetComponent<IItem>();
        newItem.Board = board;
        return newItem;
    }
    private GameObject GetParticleEffectFromPool(Dictionary<int, Stack<GameObject>> pool, int itemID, Vector3 position,GameObject prefab,Quaternion rotation=default)
    {
        if (pool[itemID].Count > 0)
        {
            GameObject item = pool[itemID].Pop();
            item.transform.position = position;
            item.transform.rotation = rotation;
            item.SetActive(true);
            return item;
        }
        return Instantiate(prefab, position,rotation);
    }

    private void ReturnItemToPool(Dictionary<int, Stack<GameObject>> pool, IItem item, int itemID)
    {
        item.Transform.parent=gameObject.transform;
        item.Transform.gameObject.SetActive(false);
        pool[itemID].Push(item.Transform.gameObject);
    }
    private void ReturnParticleEffectToPool(Dictionary<int, Stack<GameObject>> pool, GameObject item, int itemID)
    {
        item.SetActive(false);
        pool[itemID].Push(item);
    }
    public GameObject GetBoosterCreationEffect(Vector3 position)
    {
        if (_boosterCreationEffects.Count > 0)
        {
            GameObject item = _boosterCreationEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        return Instantiate(itemDatabase.boosterCreationEffect, position, Quaternion.identity);
    }
    public GameObject GetMatchEffect(Vector3 position)
    {
        if (_matchEffects.Count > 0)
        {
            GameObject item = _matchEffects.Pop();
            item.SetActive(true);
            return item;
        }
        return Instantiate(itemDatabase.matchEffect, position, Quaternion.identity);
    }
    public IItem GetItemGameObject(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(_normalItems, itemID, position, itemDatabase.GetNormalItem(itemID),board);
    }
    public IItem GetBoosterGameObject(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(_boosters, itemID, position, itemDatabase.GetBooster(itemID),board);
    }
    public GameObject GetItemParticleEffect(int itemID, Vector3 position)
    {
        return GetParticleEffectFromPool(_normalItemParticleEffects, itemID, position, itemDatabase.GetNormalItemParticleEffect(itemID));
    }
   
    public GameObject GetBoosterParticleEffect(int itemID, Vector3 position,Quaternion rotation= default)
    {
        GameObject item = GetParticleEffectFromPool(_boosterParticleEffects, itemID, position, itemDatabase.GetBoosterParticleEffect(itemID),rotation);
        return item;
    }

    public Sprite GetItemSprite(int itemID)
    {
        return itemDatabase.NormalItems[itemID].ItemSprite;
    }
    public void ReturnParticleEffect(GameObject item, int itemID)
    {
        ReturnParticleEffectToPool(_normalItemParticleEffects, item, itemID);
    }
    public void ReturnBoosterParticleEffect(GameObject item, int itemID)
    {
        ReturnParticleEffectToPool(_boosterParticleEffects, item, itemID);
    }

    public void ReturnItem(IItem item, int itemID)
    {
        if(item.IsBooster)
            ReturnItemToPool(_boosters, item, itemID);
        else
            ReturnItemToPool(_normalItems, item, itemID);
    }

    
}