using System.Text;

using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SDK.Models.Responses;
using EnesShahn.Debugger;
using EnesShahn.Configurations;

namespace SDK
{
    public class HTTPHelper : MonoBehaviour
    {
        public static async Task<T> GetDataAsync<T>(string endpoint, string idToken = null) where T : BaseResponse, new()
        {
            UnityWebRequest req = new UnityWebRequest(GameConfig.DefaultConfig.BaseUrl + endpoint, UnityWebRequest.kHttpVerbGET);

            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(idToken))
                req.SetRequestHeader("Authorization", $"Bearer {idToken}");
            req.downloadHandler = new DownloadHandlerBuffer();

            GameDebugger.Log($"Sending GET request: ({endpoint}) ({idToken})\n\n");

            req.SendWebRequest();

            while (!req.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                var response = new T
                {
                    Success = false,
                    Message = $"Error: ({endpoint})\n{req.error}\n\n"
                };
                return response;
            }

            var responseContent = req.downloadHandler.text;

            GameDebugger.Log($"Response: {responseContent}");

            return JsonConvert.DeserializeObject<T>(responseContent);
        }
        public static async Task<T> PostDataAsync<T>(string endpoint, object data, string idToken = null) where T : BaseResponse, new()
        {
            UnityWebRequest req = new UnityWebRequest(GameConfig.DefaultConfig.BaseUrl + endpoint, UnityWebRequest.kHttpVerbPOST);

            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(idToken))
                req.SetRequestHeader("Authorization", $"Bearer {idToken}");
            req.downloadHandler = new DownloadHandlerBuffer();

            var dataSerialized = JsonConvert.SerializeObject(data);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(dataSerialized));
            req.uploadHandler.contentType = "application/json";

            GameDebugger.Log($"Sending POST request: ({endpoint}) ({idToken}) ({dataSerialized})\n\n");

            req.SendWebRequest();

            while (!req.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                var response = new T
                {
                    Success = false,
                    Message = $"Error: ({endpoint})\n{req.error}\n\n"
                };
                return response;
            }

            var responseContent = req.downloadHandler.text;
            GameDebugger.Log($"Response: {responseContent}");

            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}