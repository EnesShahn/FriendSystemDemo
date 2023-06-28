using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using SDK.Models;
using SDK.Models.Requests;
using SDK.Models.Responses;

using EnesShahn.Debugger;

using UnityEngine;

namespace SDK
{
    public class Client : MonoBehaviour
    {
        private static Dictionary<Type, BaseModule> s_modules = new Dictionary<Type, BaseModule>();
        private static bool s_initialized = false;
        private static User s_userData;
        private static Token s_token;
        private static GameObject s_updateManager;

        private static string _currentIdToken;
        private static string _currentRefreshToken;

        public static User UserData => s_userData;
        public static Token Token => s_token;
        public static string CurrentIdToken => _currentIdToken;
        public static string CurrentRefreshToken => _currentRefreshToken;


        public static event Action OnUserDataFetched;

        private Client() { }

        private void Update()
        {
            foreach (var module in s_modules)
            {
                module.Value.OnUpdate();
            }
        }

        public static void Init()
        {
            if (s_initialized) return;
            s_initialized = true;
            CreateModules();
            if (s_updateManager == null)
            {
                s_updateManager = new GameObject("Account Update Manager");
                s_updateManager.AddComponent<Client>();
                DontDestroyOnLoad(s_updateManager);
            }
        }
        public static T GetModule<T>() where T : BaseModule
        {
            if (!s_initialized) Init();
            return (T)s_modules.First(m => m.Key == typeof(T)).Value;
        }
        private static void CreateModules()
        {
            var baseType = typeof(BaseModule);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType))
                    {
                        s_modules.Add(type, (BaseModule)Activator.CreateInstance(type));
                    }
                }
            }
        }

        #region API Calls
        public static async Task<Response<Token>> Login(string email, string password)
        {
            var requestData = new LoginUserRequest
            {
                Email = email,
                Password = password
            };
            var response = await HTTPHelper.PostDataAsync<Response<Token>>("/User/Login", requestData);

            if (!response.Success)
            {
                GameDebugger.Log($"Login Error: {response.Message}");
                return response;
            }

            _currentIdToken = response.Data.IdToken;
            _currentRefreshToken = response.Data.RefreshToken;

            FetchUserData();
            return response;
        }
        public static async Task<Response<Token>> Register(string email, string password)
        {
            var requestData = new RegisterUserRequest
            {
                Email = email,
                Password = password
            };
            var response = await HTTPHelper.PostDataAsync<Response<Token>>($"/User/Register", requestData);

            if (!response.Success)
            {
                GameDebugger.Log($"Register Error: {response.Message}");
                return response;
            }

            _currentIdToken = response.Data.IdToken;
            _currentRefreshToken = response.Data.RefreshToken;

            FetchUserData();
            return response;
        }
        public static async Task<User> FetchUserData()
        {
            var response = await HTTPHelper.GetDataAsync<Response<User>>("/User/GetUser", CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log($"GetUser Error: {response.Message}");
                return null;
            }

            s_userData = response.Data;
            OnUserDataFetched?.Invoke();

            return response.Data;
        }
        public static async Task<DateTime> FetchUserLastOnlineTime()
        {
            var response = await HTTPHelper.GetDataAsync<Response<DateTime>>("/User/GetUserLastOnlineTime", CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log($"GetUserLastOnlineTime Error: {response.Message}");
                return DateTime.MinValue;
            }

            s_userData.LastOnlineTime = response.Data;

            return response.Data;
        }
        #endregion
    }
}