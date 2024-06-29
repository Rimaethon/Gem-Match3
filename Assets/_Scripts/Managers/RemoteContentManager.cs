using System;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class RemoteContentManager:MonoBehaviour
    {
        public IAmazonS3 S3Client;
        public string pathToSave="Assets/Data/Levels/";
        public string S3BucketName = "match3contentbucket";
        public List<String> LevelNames=new List<string>();

        void Start()
        {

            UnityInitializer.AttachToGameObject(gameObject);
            AmazonS3Config s3Config = new AmazonS3Config();
            s3Config.RegionEndpoint = Amazon.RegionEndpoint.EUCentral1;
            S3Client = new AmazonS3Client("", "", s3Config);
        }


        [Button]
        public void  ListAllLevelsInServer()
        {
            ListObjectsV2Request listObjectsRequest = new ListObjectsV2Request()
            {
                BucketName = S3BucketName
            };
            S3Client.ListObjectsV2Async(listObjectsRequest, (responseObject) =>
                                                            {
                                                                if(responseObject.Exception==null)
                                                                    Debug.Log("Successfully retrieved object with key " + S3BucketName);
                                                                else
                                                                    Debug.Log("Exception while retrieving object with key "+S3BucketName +" " + responseObject.Exception);
                                                                if (responseObject.Exception == null)
                                                                {
                                                                    responseObject.Response.S3Objects.ForEach((o) =>
                                                                                                              {
                                                                                                                  if (LevelNames.Contains(o.Key) == false)
                                                                                                                  {
                                                                                                                      LevelNames.Add(o.Key);
                                                                                                                  }
                                                                                                              });
                                                                }
                                                            });
        }

        [Button]
        public void GetAllLevelsFromServer()
        {
             ListAllLevelsInServer();
            foreach (var levelName in LevelNames)
            {
                if (File.Exists(pathToSave + levelName))
                {
                    Debug.Log("Level with name " + levelName + " already exists");
                    continue;
                }
                 S3Client.GetObjectAsync(S3BucketName, levelName, (responseObj) =>
                                                                   {
                                                                       if(responseObj.Exception==null)
                                                                           Debug.Log("Successfully retrieved object with key " + levelName);
                                                                       else
                                                                           Debug.Log("Exception while retrieving object with key " + levelName + ": " + responseObj.Exception);
                                                                       byte[] data = null;
                                                                       var response = responseObj.Response;

                                                                           using (StreamReader reader = new StreamReader(response.ResponseStream))
                                                                           {
                                                                               using (MemoryStream memory = new MemoryStream())
                                                                               {
                                                                                   var buffer = new byte[512];
                                                                                   var bytesRead = default(int);


                                                                                   while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                                                                   {
                                                                                       memory.Write(buffer, 0, bytesRead);
                                                                                   }

                                                                                   data = memory.ToArray();

                                                                               }
                                                                           }

                                                                           using (MemoryStream memory= new MemoryStream(data))
                                                                           {
                                                                               Debug.Log("Level "+levelName+" saved to "+pathToSave+" folder");
                                                                               File.WriteAllBytes(pathToSave+levelName, memory.ToArray());
                                                                               EventManager.Instance.Broadcast(GameEvents.OnLevelDownloaded);
                                                                           }
                                                                   });
            }

        }
        public async UniTask ListBuckets()
        {
            ListBucketsRequest listBucketsV2Request = new ListBucketsRequest();
            S3Client.ListBucketsAsync(listBucketsV2Request,(responseObject) =>
                                                           {
                                                               if (responseObject.Exception == null)
                                                               {
                                                                   responseObject.Response.Buckets.ForEach((b) =>
                                                                                                           {
                                                                                                               Debug.Log("Bucket with name " + b.BucketName);
                                                                                                           });
                                                               }
                                                               else
                                                               {
                                                                   Debug.Log("Exception while listing buckets " + responseObject.Exception);
                                                               }
                                                           });
            await UniTask.Yield();
        }

    }
}
