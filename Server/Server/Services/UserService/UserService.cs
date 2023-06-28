using Google.Cloud.Firestore;

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

            Dictionary<string, object>? loginResponse = await httpResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (loginResponse == null)
            {
                return new Response<Token>()
                {
                    Success = false,
                    Message = "Error, probably registeration"
                };
            }

            await CreateUserDocument(loginResponse["localId"].ToString(), loginResponse["email"].ToString());

            var response = new Response<Token>();

            if (loginResponse == null)
            {
                response.Success = false;
                response.Message = "Failed";
            }
            else
            {
                response.Success = true;
                response.Data = new Token
                {
                    IdToken = loginResponse["idToken"].ToString(),
                    RefreshToken = loginResponse["refreshToken"].ToString(),
                    ExpiresIn = loginResponse["expiresIn"].ToString()
                };
            }

            return response;
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
            Dictionary<string, object>? loginResponse = await httpResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (!httpResponse.IsSuccessStatusCode)
            {
                return new Response<Token>
                {
                    Success = false,
                    Message = "Login error"
                };
            }

            if (loginResponse == null)
            {
                return new Response<Token>()
                {
                    Success = false,
                    Message = "Invalid login credentials"
                };
            }

            var response = new Response<Token>();

            if (loginResponse == null)
            {
                response.Success = false;
                response.Message = "Failed";
            }
            else
            {
                //Check if user data doesn't exists..
                var userDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(loginResponse["localId"].ToString()).GetSnapshotAsync();
                if (!userDocSnapshot.Exists)
                    await CreateUserDocument(loginResponse["localId"].ToString(), loginResponse["email"].ToString());

                response.Success = true;
                response.Data = new Token
                {
                    IdToken = loginResponse["idToken"].ToString(),
                    RefreshToken = loginResponse["refreshToken"].ToString(),
                    ExpiresIn = loginResponse["expiresIn"].ToString()
                };
            }


            return response;
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
            };
            await usersCollection.Document(uid).SetAsync(newUser);
        }
    }
}
