using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace DefaultNamespace
{
    public class RemoteContentManager:MonoBehaviour
    {
        public IAmazonS3 S3Client;
        
        void Start()
        {
            AmazonS3Config s3Config = new AmazonS3Config();
            s3Config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
            
           
        }
        
        public async UniTask GetObjects()
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = "match3contentbucket",
                Key = "Level1.json"
            };

            S3Client.GetObjectAsync(request, (response) =>
            {
                using (Stream responseStream = response.Response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseBody = reader.ReadToEnd(); // Now you have the object data
                    Debug.Log(responseBody);
                }
            });
            await UniTask.Yield();
          
        }
        
    }
}