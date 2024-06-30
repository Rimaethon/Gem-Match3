using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Utility;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    public ItemDatabaseSO itemDatabase;
    private readonly Dictionary<int, Stack<GameObject>> normalItems= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> boosters= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> normalItemParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> boosterParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<IItemAction>> itemActions= new Dictionary<int, Stack<IItemAction>>();
    private readonly List<Tuple<int, int, Stack<IItemMergeAction>>> boosterMergeAction= new List<Tuple<int, int, Stack<IItemMergeAction>>>();
    private readonly Stack<GameObject> boosterCreationEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> missileHitEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> missileExplosionEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> tntRocketMergeParticleEffects = new Stack<GameObject>();
    private readonly Stack<MainEventUIEffect> mainEventUIEffects = new Stack<MainEventUIEffect>();
    //No more than one will be on board at a time
    public GameObject tntTntEffect;
    public GameObject lightBallLightBallEffect;
    private bool _isInitialized;

    public async UniTask InitializeStacks(HashSet<int> spawnAbleItems,int itemAmount,int boosterAmount)
    {
        if (_isInitialized)
            return;
        _isInitialized = true;
        foreach (var key in itemDatabase.NormalItems.Keys)
        {
            normalItems.Add(key, new Stack<GameObject>());
            normalItemParticleEffects.Add(key, new Stack<GameObject>());
            itemActions.Add(key, new Stack<IItemAction>());

        }
        foreach (int key in spawnAbleItems)
        {
            await AddItemsToPool(normalItems, itemDatabase.GetNormalItem, key, itemAmount );
            await AddItemsToPool(normalItemParticleEffects, itemDatabase.GetNormalItemParticleEffect, key, itemAmount );
            await AddItemsToPool(itemActions, itemDatabase.GetNormalItemAction, key, itemAmount);
        }
        await AddItemsToPool(boosterCreationEffects, _ => itemDatabase.boosterCreationEffect, 0, boosterAmount);
        await AddItemsToPool(missileHitEffects, _ => itemDatabase.missileHitEffect, 0, boosterAmount);
        await AddItemsToPool(missileExplosionEffects, _ => itemDatabase.missileExplosionEffect, 0, boosterAmount);
        if(SaveManager.Instance.HasMainEvent())
            await AddMainEventUIEffect(itemAmount,SaveManager.Instance.GetMainEventData().eventGoalID);
        foreach (int key in itemDatabase.Boosters.Keys)
        {
            if(key>104)
                boosterAmount = 1;
            boosters.Add(key, new Stack<GameObject>());
            boosterParticleEffects.Add(key, new Stack<GameObject>());
            itemActions.Add(key, new Stack<IItemAction>());
            await AddItemsToPool(boosters, itemDatabase.GetBooster, key, boosterAmount);
            await AddItemsToPool(itemActions, itemDatabase.GetBoosterItemAction, key, boosterAmount);
            await AddItemsToPool(boosterParticleEffects, itemDatabase.GetBoosterParticleEffect, key, boosterAmount);
        }

        foreach (var action in itemDatabase.BoosterMergeAction)
        {
            boosterMergeAction.Add(new Tuple<int, int, Stack<IItemMergeAction>>(action.Item1, action.Item2, new Stack<IItemMergeAction>()));
            for (int i = 0; i < 3; i++)
            {
                IItemMergeAction item = Activator.CreateInstance(action.Item3.GetType()) as IItemMergeAction;
                boosterMergeAction[^1].Item3.Push(item);

            }
        }
        tntTntEffect= Instantiate(itemDatabase.TntTntExplosionEffect);
        lightBallLightBallEffect= Instantiate(itemDatabase.LightBallLightBallExplosionEffect);
        tntTntEffect.SetActive(false);
        lightBallLightBallEffect.SetActive(false);
        tntTntEffect.transform.parent= gameObject.transform;
        lightBallLightBallEffect.transform.parent= gameObject.transform;
    }

    private async UniTask AddMainEventUIEffect(int itemAmount, int mainEventID)
    {
        for (int i = 0; i < itemAmount; i++)
        {
            MainEventUIEffect item =Instantiate(itemDatabase.mainEventUIEffect,transform).GetComponent<MainEventUIEffect>();
            item.gameObject.SetActive(false);
            item.image.sprite = itemDatabase.NormalItems[mainEventID].ItemSprite;
            mainEventUIEffects.Push(item);
        }
        await UniTask.Yield();

    }

    private async UniTask AddItemsToPool(Dictionary<int, Stack<GameObject>> pool, Func<int, GameObject> getItem, int key, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            getItem(key).SetActive(false);
            GameObject item =Instantiate(getItem(key));
            item.transform.parent= gameObject.transform;
            pool[key].Push(item);
        }
        await UniTask.Yield();
    }
    private async UniTask AddItemsToPool(Dictionary<int, Stack<IItemAction>> pool, Func<int, IItemAction> getItem, int key, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            IItemAction item = Activator.CreateInstance(getItem(key).GetType()) as IItemAction;;
            item.ItemID = key;
            pool[key].Push(item);
        }
        await UniTask.Yield();
    }
    private async UniTask AddItemsToPool(Stack<GameObject> pool, Func<int, GameObject> getItem, int key, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject item =Instantiate(getItem(key));
            item.transform.parent= gameObject.transform;
            item.SetActive(false);
            pool.Push(item);
        }
        await UniTask.Yield();
    }
    public IItemMergeAction GetItemMergeAction(int item1ID, int item2ID)
    {
        foreach (var merge in boosterMergeAction)
        {
            if (merge.Item1 == item1ID && merge.Item2 == item2ID)
            {
                if (merge.Item3.Count > 0)
                {
                    return merge.Item3.Pop();
                }
            }
        }
        return null;
    }
    public void ReturnItemMergeAction(IItemMergeAction item, int item1ID, int item2ID)
    {
        foreach (var merge in boosterMergeAction)
        {
            if (merge.Item1 == item1ID && merge.Item2 == item2ID||merge.Item1 == item2ID && merge.Item2 == item1ID)
            {
                merge.Item3.Push(item);
            }
        }
    }
    public MainEventUIEffect GetMainEventUIEffect(int itemID)
    {
        if (mainEventUIEffects.Count > 0)
        {
            MainEventUIEffect item = mainEventUIEffects.Pop();
            item.gameObject.SetActive(true);
            return item;
        }
        AddMainEventUIEffect(1,itemID).Forget();
        return GetMainEventUIEffect(itemID);
    }
    public void ReturnMainEventUIEffect(MainEventUIEffect item)
    {
        item.gameObject.SetActive(false);
        mainEventUIEffects.Push(item);
    }
    public IItemAction GetItemActionFromPool( int itemID)
    {
        if (itemActions[itemID].Count > 0)
        {
            return itemActions[itemID].Pop();;
        }
        if(itemDatabase.NormalItems.ContainsKey(itemID))
            return Activator.CreateInstance(itemDatabase.GetNormalItemAction(itemID).GetType()) as IItemAction;
        if(itemDatabase.Boosters.ContainsKey(itemID))
            return Activator.CreateInstance(itemDatabase.GetBoosterItemAction(itemID).GetType()) as IItemAction;
        return null;
    }

    public void ReturnItemActionToPool(IItemAction item)
    {
        itemActions[item.ItemID].Push(item);
    }

    private IBoardItem GetItemFromPool(Dictionary<int, Stack<GameObject>> pool, int itemID, Vector3 position,GameObject prefab,Board board)
    {
        if (pool[itemID].Count > 0)
        {
            IBoardItem boardItem = pool[itemID].Pop().GetComponent<IBoardItem>();

            boardItem.Transform.position = position;
            boardItem.Transform.gameObject.SetActive(true);
            boardItem.Board = board;
            return boardItem;
        }
        GameObject Item = Instantiate(prefab, position, prefab.transform.rotation);
        Item.SetActive(true);
        IBoardItem newBoardItem = Item.GetComponent<IBoardItem>();
        newBoardItem.Board = board;
        return newBoardItem;
    }
    private GameObject GetGameObjectFromPool(Dictionary<int, Stack<GameObject>> pool, int itemID, Vector3 position,GameObject prefab)
    {
        if (pool[itemID].Count > 0)
        {
            GameObject item = pool[itemID].Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        return Instantiate(prefab, position, Quaternion.identity);
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
        GameObject particleEffect = Instantiate(prefab, position,rotation);
        particleEffect.SetActive(true);
        return particleEffect;
    }
    private void ReturnItemToPool(Dictionary<int, Stack<GameObject>> pool, IBoardItem boardItem, int itemID)
    {
        boardItem.Transform.parent=gameObject.transform;
        boardItem.Transform.gameObject.SetActive(false);
        pool[itemID].Push(boardItem.Transform.gameObject);
    }
    private void ReturnParticleEffectToPool(Dictionary<int, Stack<GameObject>> pool, GameObject item, int itemID)
    {
        item.transform.parent=gameObject.transform;
        item.SetActive(false);
        pool[itemID].Push(item);
    }
    public GameObject GetBoosterCreationEffect(Vector3 position)
    {
        if (boosterCreationEffects.Count > 0)
        {
            GameObject item = boosterCreationEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        return Instantiate(itemDatabase.boosterCreationEffect, position, Quaternion.identity);
    }
    public void GetMissileHitEffect(Vector3 position)
    {
        if (missileHitEffects.Count > 0)
        {
            GameObject item = missileHitEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return;
        }
        Instantiate(itemDatabase.missileHitEffect, position, Quaternion.identity);
    }
    public void GetMissileExplosionEffect(Vector3 position)
    {
        if (missileExplosionEffects.Count > 0)
        {
            GameObject item = missileExplosionEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return;
        }
        Instantiate(itemDatabase.missileExplosionEffect, position, Quaternion.identity);
    }
    public GameObject GetTntRocketMergeParticleEffect(Vector3 position)
    {
        return Instantiate(itemDatabase.TntRocketMergeParticleEffect, position, Quaternion.identity);
    }
    public void ReturnMissileHitEffect(GameObject item)
    {
        item.SetActive(false);
        missileHitEffects.Push(item);
    }
    public void ReturnMissileExplosionEffect(GameObject item)
    {
        item.SetActive(false);
        missileExplosionEffects.Push(item);
    }

    public void ReturnBoosterCreationEffect(GameObject item)
    {
        item.SetActive(false);
        boosterCreationEffects.Push(item);
    }
    public IBoardItem GetItem(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(normalItems, itemID, position, itemDatabase.GetNormalItem(itemID),board);
    }

    public IBoardItem GetBoosterItem(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(boosters, itemID, position, itemDatabase.GetBooster(itemID),board);
    }
    public GameObject GetNormalItemGameObject(int itemID, Vector3 position)
    {
        return GetGameObjectFromPool(normalItems, itemID, position, itemDatabase.GetNormalItem(itemID));
    }
    public GameObject GetBoosterGameObject(int itemID, Vector3 position)
    {
        return GetGameObjectFromPool(boosters, itemID, position, itemDatabase.GetBooster(itemID));
    }
    public GameObject GetItemParticleEffect(int itemID, Vector3 position)
    {
        return GetParticleEffectFromPool(normalItemParticleEffects, itemID, position, itemDatabase.GetNormalItemParticleEffect(itemID));
    }

    public GameObject GetBoosterParticleEffect(int itemID, Vector3 position,Quaternion rotation= default)
    {
        GameObject item = GetParticleEffectFromPool(boosterParticleEffects, itemID, position, itemDatabase.GetBoosterParticleEffect(itemID),rotation);
        item.SetActive(true);
        return item;
    }

    public Sprite GetItemSprite(int itemID)
    {
        return itemDatabase.NormalItems[itemID].ItemSprite;
    }
    public Sprite GetBoosterSprite(int itemID)
    {
        return itemDatabase.Boosters[itemID].ItemSprite;
    }
    public void ReturnParticleEffect(GameObject item, int itemID)
    {
        ReturnParticleEffectToPool(normalItemParticleEffects, item, itemID);
    }
    public void ReturnBoosterParticleEffect(GameObject item, int itemID)
    {
        ReturnParticleEffectToPool(boosterParticleEffects, item, itemID);
    }

    public void ReturnItem(IBoardItem boardItem, int itemID)
    {
        if(boardItem.IsBooster)
            ReturnItemToPool(boosters, boardItem, itemID);
        else
            ReturnItemToPool(normalItems, boardItem, itemID);
    }

}
