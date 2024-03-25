using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using Rimaethon.Scripts.Managers;


public class UniversalCounter : MonoBehaviour
{
    private const string API_URL = "http://worldtimeapi.org/api/ip";
    private (int[], int[]) currentDateTime;
    private string path = "Assets/Data/User Data/";
    private string levelName = "TimeDependentData";

  
    private void OnDisable()
    {
        if(EventManager.Instance!=null)return;
    }

    private void Start()
    {
        //StartCoroutine(GetRealDateTimeFromAPI());
    }

    IEnumerator GetRealDateTimeFromAPI()
    {
       UnityWebRequest request = UnityWebRequest.Get(API_URL);
       yield return request.SendWebRequest();
       if(request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
       {
           Debug.LogError(request.error);
       }
       else
       {
           string response = request.downloadHandler.text;
           currentDateTime = ParseDataTime(response);
           Debug.Log("Current Date: " + currentDateTime.Item1[0] + "-" + currentDateTime.Item1[1] + "-" + currentDateTime.Item1[2]);
       }
    }

    private (int[], int[]) ParseDataTime(string dateTime)
    {
        string date = Regex.Match(dateTime, @"\d{4}-\d{2}-\d{2}").Value;
        string time = Regex.Match(dateTime, @"\d{2}:\d{2}:\d{2}").Value;

        string[] dateParts = date.Split('-');
        string[] timeParts = time.Split(':');

        int[] dateArray = Array.ConvertAll(dateParts, int.Parse);
        int[] timeArray = Array.ConvertAll(timeParts, int.Parse);

        Array.Reverse(dateArray);
        return (dateArray, timeArray);
    }
    
}
