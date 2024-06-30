using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Utility;
using Scripts;
using Scripts.BoosterActions;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPool : Singleton<ObjectPool>
{
    public ItemDatabaseSO itemDatabase;
    private readonly Dictionary<int, Stack<GameObject>> _normalItems= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _boosters= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _normalItemParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<GameObject>> _boosterParticleEffects= new Dictionary<int, Stack<GameObject>>();
    private readonly Dictionary<int, Stack<IItemAction>> _itemActions= new Dictionary<int, Stack<IItemAction>>();
    private List<Tuple<int, int, Stack<IItemMergeAction>>> _boosterMergeAction= new List<Tuple<int, int, Stack<IItemMergeAction>>>();
    private readonly Stack<GameObject> _boosterCreationEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> _missileHitEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> _missileExplosionEffects = new Stack<GameObject>();
    private readonly Stack<GameObject> _tntRocketMergeParticleEffects = new Stack<GameObject>();
    private readonly Stack<MainEventUIEffect> _mainEventUIEffects = new Stack<MainEventUIEffect>();
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
            _normalItems.Add(key, new Stack<GameObject>());
            _normalItemParticleEffects.Add(key, new Stack<GameObject>());
            _itemActions.Add(key, new Stack<IItemAction>());

        }
        foreach (int key in spawnAbleItems)
        {
            await AddItemsToPool(_normalItems, itemDatabase.GetNormalItem, key, itemAmount );
            await AddItemsToPool(_normalItemParticleEffects, itemDatabase.GetNormalItemParticleEffect, key, itemAmount );
            await AddItemsToPool(_itemActions, itemDatabase.GetNormalItemAction, key, itemAmount);
        }
        await AddItemsToPool(_boosterCreationEffects, _ => itemDatabase.boosterCreationEffect, 0, boosterAmount);
        await AddItemsToPool(_missileHitEffects, _ => itemDatabase.missileHitEffect, 0, boosterAmount);
        await AddItemsToPool(_missileExplosionEffects, _ => itemDatabase.missileExplosionEffect, 0, boosterAmount);
        if(SaveManager.Instance.HasMainEvent())
            await AddMainEventUIEffect(itemAmount,SaveManager.Instance.GetMainEventData().eventGoalID);
        foreach (int key in itemDatabase.Boosters.Keys)
        {
            if(key>104)
                boosterAmount = 1;
            _boosters.Add(key, new Stack<GameObject>());
            _boosterParticleEffects.Add(key, new Stack<GameObject>());
            _itemActions.Add(key, new Stack<IItemAction>());
            await AddItemsToPool(_boosters, itemDatabase.GetBooster, key, boosterAmount);
            await AddItemsToPool(_itemActions, itemDatabase.GetBoosterItemAction, key, boosterAmount);
            await AddItemsToPool(_boosterParticleEffects, itemDatabase.GetBoosterParticleEffect, key, boosterAmount);
        }

        foreach (var action in itemDatabase.BoosterMergeAction)
        {
            _boosterMergeAction.Add(new Tuple<int, int, Stack<IItemMergeAction>>(action.Item1, action.Item2, new Stack<IItemMergeAction>()));
            for (int i = 0; i < 3; i++)
            {
                IItemMergeAction item = Activator.CreateInstance(action.Item3.GetType()) as IItemMergeAction;
                _boosterMergeAction[^1].Item3.Push(item);

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
            _mainEventUIEffects.Push(item);
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
        foreach (var merge in _boosterMergeAction)
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
        foreach (var merge in _boosterMergeAction)
        {
            if (merge.Item1 == item1ID && merge.Item2 == item2ID||merge.Item1 == item2ID && merge.Item2 == item1ID)
            {
                merge.Item3.Push(item);
            }
        }
    }
    public MainEventUIEffect GetMainEventUIEffect(int itemID)
    {
        if (_mainEventUIEffects.Count > 0)
        {
            MainEventUIEffect item = _mainEventUIEffects.Pop();
            item.gameObject.SetActive(true);
            return item;
        }
        AddMainEventUIEffect(1,itemID).Forget();
        return GetMainEventUIEffect(itemID);
    }
    public void ReturnMainEventUIEffect(MainEventUIEffect item)
    {
        item.gameObject.SetActive(false);
        _mainEventUIEffects.Push(item);
    }
    public IItemAction GetItemActionFromPool( int itemID)
    {
        if (_itemActions[itemID].Count > 0)
        {
            return _itemActions[itemID].Pop();;
        }
        if(itemDatabase.NormalItems.ContainsKey(itemID))
            return Activator.CreateInstance(itemDatabase.GetNormalItemAction(itemID).GetType()) as IItemAction;
        if(itemDatabase.Boosters.ContainsKey(itemID))
            return Activator.CreateInstance(itemDatabase.GetBoosterItemAction(itemID).GetType()) as IItemAction;
        return null;
    }

    public void ReturnItemActionToPool(IItemAction item)
    {
        _itemActions[item.ItemID].Push(item);
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
        if (_boosterCreationEffects.Count > 0)
        {
            GameObject item = _boosterCreationEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }
        return Instantiate(itemDatabase.boosterCreationEffect, position, Quaternion.identity);
    }
    public void GetMissileHitEffect(Vector3 position)
    {
        if (_missileHitEffects.Count > 0)
        {
            GameObject item = _missileHitEffects.Pop();
            item.transform.position = position;
            item.SetActive(true);
            return;
        }
        Instantiate(itemDatabase.missileHitEffect, position, Quaternion.identity);
    }
    public void GetMissileExplosionEffect(Vector3 position)
    {
        if (_missileExplosionEffects.Count > 0)
        {
            GameObject item = _missileExplosionEffects.Pop();
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
        _missileHitEffects.Push(item);
    }
    public void ReturnMissileExplosionEffect(GameObject item)
    {
        item.SetActive(false);
        _missileExplosionEffects.Push(item);
    }

    public void ReturnBoosterCreationEffect(GameObject item)
    {
        item.SetActive(false);
        _boosterCreationEffects.Push(item);
    }
    public IBoardItem GetItem(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(_normalItems, itemID, position, itemDatabase.GetNormalItem(itemID),board);
    }

    public IBoardItem GetBoosterItem(int itemID, Vector3 position,Board board)
    {
        return GetItemFromPool(_boosters, itemID, position, itemDatabase.GetBooster(itemID),board);
    }
    public GameObject GetNormalItemGameObject(int itemID, Vector3 position)
    {
        return GetGameObjectFromPool(_normalItems, itemID, position, itemDatabase.GetNormalItem(itemID));
    }
    public GameObject GetBoosterGameObject(int itemID, Vector3 position)
    {
        return GetGameObjectFromPool(_boosters, itemID, position, itemDatabase.GetBooster(itemID));
    }
    public GameObject GetItemParticleEffect(int itemID, Vector3 position)
    {
        return GetParticleEffectFromPool(_normalItemParticleEffects, itemID, position, itemDatabase.GetNormalItemParticleEffect(itemID));
    }

    public GameObject GetBoosterParticleEffect(int itemID, Vector3 position,Quaternion rotation= default)
    {
        GameObject item = GetParticleEffectFromPool(_boosterParticleEffects, itemID, position, itemDatabase.GetBoosterParticleEffect(itemID),rotation);
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
        ReturnParticleEffectToPool(_normalItemParticleEffects, item, itemID);
    }
    public void ReturnBoosterParticleEffect(GameObject item, int itemID)
    {
        ReturnParticleEffectToPool(_boosterParticleEffects, item, itemID);
    }

    public void ReturnItem(IBoardItem boardItem, int itemID)
    {
        if(boardItem.IsBooster)
            ReturnItemToPool(_boosters, boardItem, itemID);
        else
            ReturnItemToPool(_normalItems, boardItem, itemID);
    }

}
