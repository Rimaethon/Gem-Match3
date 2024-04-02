using System.Collections.Generic;
using System.IO;
using Data;
using Rimaethon.Scripts.Utility;
using Sirenix.Serialization;
using UnityEngine;

//Get and Set Methods in this class are just for testing purposes. In a real game, these methods should be server authoritative .
public class SaveManager : PersistentSingleton<SaveManager>
{
    [SerializeField] private string userDataName = "Assets/Data/General/UserData";
    [SerializeField] private string gameDataName = "Assets/Data/General/GameData";
    [SerializeField] private string levelDataFolder = "Assets/Data/Levels/Level";
    private const string Extension = ".json";
    [SerializeField] private ItemDatabaseSO itemDatabase;
    private UserData _userData=new UserData();
  
    protected override void Awake()
    {
        CheckAndCreateData();
        base.Awake();
    }

    
    private void CheckAndCreateData()
    {
        //This part would normally consist server side check for data
        if (!File.Exists(userDataName))
        {
            InitializeUserData(_userData);
            SaveToJson(userDataName, _userData);
        }
        else
        {
            LoadFromJson(userDataName);
        }
    }
    public bool DoesLevelExist(int levelIndex)
    {
        if(!File.Exists(levelDataFolder + levelIndex + Extension))
            //This part would normally consist server side check for data
            return false;
        return true ;
    }
    public int GetLevelIndex()
    {
        return _userData.currentLevel;
    }
    public void SetLevelIndex(int levelIndex)
    {
        _userData.currentLevel = levelIndex;
        SaveToJson(userDataName, _userData);
    }
    public int GetCoinCount()
    {
        return _userData.coinCount;
    }
    public void SetCoinCount(int coinCount)
    {
        _userData.coinCount = coinCount;
        SaveToJson(userDataName, _userData);
    }
    public int GetHeartCount()
    {
        return _userData.heartCount;
    }
    public void SetHeartCount(int heartCount)
    {
        _userData.heartCount = heartCount;
        SaveToJson(userDataName, _userData);
    }
    public int GetStarCount()
    {
        return _userData.starCount;
    }
    public void SetStarCount(int starCount)
    {
        _userData.starCount = starCount;
        SaveToJson(userDataName, _userData);
    }
    public int GetBoosterCount(int boosterId)
    {
        if (_userData.BoosterCounts.TryGetValue(boosterId, out int count))
        {
            return count;
        }
        return 0;
    }
    public void SetBoosterCount(int boosterId, int count)
    {
        if (_userData.BoosterCounts.ContainsKey(boosterId))
        {
            _userData.BoosterCounts[boosterId] = count;
        }
        else
        {
            _userData.BoosterCounts.Add(boosterId, count);
        }
        SaveToJson(userDataName, _userData);
    }
    private void LoadFromJson(string path)
    {
        var data = File.ReadAllBytes(path);
        _userData = SerializationUtility.DeserializeValue<UserData>(data, DataFormat.JSON);
    }
    private void InitializeUserData(UserData userData)
    {
        userData.coinCount = 3000;
        userData.currentLevel = 1;
        userData.heartCount = 5;
        userData.starCount = 0;
        userData.BoosterCounts = new Dictionary<int, int>();
        foreach (var item in itemDatabase.Boosters)
        {
            userData.BoosterCounts.Add(item.Key, 10);
        }
    }
    private void SaveToJson(string path, UserData userData)
    {
        var serializedData = SerializationUtility.SerializeValue(userData, DataFormat.JSON);
        File.WriteAllBytes(path+Extension, serializedData);
    }
}