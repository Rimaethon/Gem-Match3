using System.Collections.Generic;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Create ItemDatabase")]
public class ItemDatabaseSO : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "Item Label", ValueLabel = "Item SO")]
    public Dictionary<int, AbstractItemDataSO> BoardElements = new Dictionary<int, AbstractItemDataSO>();
    public GameObject GetBoardElement(int id)
    {
         BoardElements[id].ItemPrefab.GetComponent<IItem>().ItemType = id;
       
        return BoardElements[id].ItemPrefab;

    }
    public GameObject GetParticleEffect(int id)
    {
        ParticleEffects[id].ItemPrefab.GetComponent<ParticleEffectHandler>().particleEffectID = id;
        return ParticleEffects[id].ItemPrefab;
    }
    public Dictionary<int, AbstractItemDataSO> UIElements = new Dictionary<int, AbstractItemDataSO>();
    public Dictionary<int, AbstractItemDataSO> ParticleEffects = new Dictionary<int, AbstractItemDataSO>();
}