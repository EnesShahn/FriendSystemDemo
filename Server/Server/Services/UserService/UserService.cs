using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;

namespace Server.Services.UserService
{
    public class UserService : IUserService
    {
        private const string SignInEndpoint = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
        private const string SignUpEndpoint = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
        private const string RefreshTokenEndpoint = "https://securetoken.googleapis.com/v1/token";

        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private FirestoreDb _firestoreDb;

        public UserService(IConfiguration configuration, FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            _configuration = configuration;
            _apiKey = _configuration["FirebaseAPIKey"];
        }

        public async Task<Response<Token>> Register(RegisterUserRequest request)
        {
            HttpClient client = new HttpClient();
            var uri = SignUpEndpoint + $"?key={_apiKey}";

            HttpContent httpContent = JsonContent.Create(new
            {
                email = request.Email,
                password = request.Password,
            });

            var httpResponse = await client.PostAsync(uri, httpContent);
            client.Dispose();

            string responseBody = await httpResponse.Content.ReadAsStringAsync();
            var response = JObject.Parse(responseBody);

            if (!httpResponse.IsSuccessStatusCode || string.IsNullOrEmpty(responseBody) || response == null)
            {
                return new Response<Token>()
                {
                    Success = false,
                    Message = response?["error"]?["message"]?.ToString() ?? "Unexpected error occured."
                };
            }

            await CreateUserDocument(response["localId"].ToString(), response["email"].ToString());

            var tokenData = new Token
            {
                IdToken = response["idToken"].ToString(),
                RefreshToken = response["refreshToken"].ToString(),
                ExpiresIn = response["expiresIn"].ToString()
            };
            var tokenResponse = new Response<Token>
            {
                Success = true,
                Data = tokenData
            };

            return tokenResponse;
        }
        public async Task<Response<Token>> Login(LoginUserRequest request)
        {
            HttpClient client = new HttpClient();
            var uri = SignInEndpoint + $"?key={_apiKey}";

            HttpContent httpContent = JsonContent.Create(new
            {
                email = request.Email,
                password = request.Password,
                returnSecureToken = true,
            });

            var httpResponse = await client.PostAsync(uri, httpContent);
            client.Dispose();

            string responseBody = await httpResponse.Content.ReadAsStringAsync();
            var response = JObject.Parse(responseBody);

            if (!httpResponse.IsSuccessStatusCode || string.IsNullOrEmpty(responseBody) || response == null)
            {
                return new Response<Token>()
                {
                    Success = false,
                    Message = response?["error"]?["message"]?.ToString() ?? "Unexpected error occured."
                };
            }

            var userDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection)
                .Document(response["localId"].ToString()).GetSnapshotAsync();
            if (!userDocSnapshot.Exists)
                await CreateUserDocument(response["localId"].ToString(), response["email"].ToString());

            var tokenData = new Token
            {
                IdToken = response["idToken"].ToString(),
                RefreshToken = response["refreshToken"].ToString(),
                ExpiresIn = response["expiresIn"].ToString()
            };
            var tokenResponse = new Response<Token>
            {
                Success = true,
                Data = tokenData
            };

            return tokenResponse;
        }
        public async Task<Response<User>> GetUser(string userId)
        {
            var userDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(userId).GetSnapshotAsync();
            if (!userDocSnapshot.Exists)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "User doesn't exist",
                };
            }

            var user = userDocSnapshot.ConvertTo<User>();


            var response = new Response<User>
            {
                Success = true,
                Data = user
            };

            return response;
        }

        public async Task<Response<DateTime>> GetUserLastOnlineTime(string userId)
        {
            var userDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(userId).GetSnapshotAsync();
            if (!userDocSnapshot.Exists)
            {
                return new Response<DateTime>
                {
                    Success = false,
                    Message = "User doesn't exist",
                };
            }

            var lastOnlineTime = userDocSnapshot.GetValue<DateTime>(FieldConstants.LastOnlineTime);
            return new Response<DateTime>
            {
                Success = true,
                Data = lastOnlineTime
            };

        }
        private async Task CreateUserDocument(string uid, string email)
        {
            CollectionReference usersCollection = _firestoreDb.Collection("Users");
            User newUser = new User
            {
                Email = email,
                LastOnlineTime = DateTime.UtcNow
            };
            await usersCollection.Document(uid).SetAsync(newUser);
        }
    }
}
