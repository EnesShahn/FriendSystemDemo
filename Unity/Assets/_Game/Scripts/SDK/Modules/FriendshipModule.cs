using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SDK.Models;
using SDK.Models.Requests;
using SDK.Models.Responses;

using EnesShahn.Debugger;

using UnityEngine;
namespace SDK.Modules
{
    public class FriendshipModule : BaseModule
    {
        private const float MessageChangeCheckInterval = 2;
        private const float FriendsChangeCheckInterval = 5;

        private Dictionary<string, User> _friendsMap = new Dictionary<string, User>();
        private Dictionary<string, Dictionary<string, Message>> _friendshipMessages = new Dictionary<string, Dictionary<string, Message>>(); // Caching messages. 

        private Action<User> _onFriendAdded;
        private Action<User> _onFriendRemoved;
        private Action<User, DateTime> _onFriendLastOnlineTimeUpdated;
        private readonly Dictionary<string, Action<Message>> _onNewMessageAdded = new Dictionary<string, Action<Message>>();

        private List<string> _listenForFriendshipMessages = new List<string>();
        private Dictionary<string, bool> _isFetchingFriendshipMessages = new Dictionary<string, bool>();
        private Dictionary<string, float> _lastFriendshipMessagesFetchTime = new Dictionary<string, float>();

        private bool _listenForFriendsLastOnlineTimeChange;
        private bool _isFetchingFriendsLastOnlineTime;
        private float _lastFriendsLastOnlineTimeFetchTime;

        private bool _listenForFriendsChanges;
        private bool _isFetchingFriends;
        private float _lastFriendsFetchTime;

        #region Inherited Methods
        public override void OnUpdate()
        {
            CheckForChanges();
        }
        #endregion

        #region Stored Data Getters
        public List<User> GetStoredFriends()
        {
            return _friendsMap.Values.ToList();//TODO: Make GC free
        }
        public List<Message> GetStoredMessages(string friendId)
        {
            if (!_friendshipMessages.ContainsKey(friendId)) return new List<Message>();
            return _friendshipMessages[friendId].Values.ToList();
        }
        #endregion

        #region API Calls
        public async Task<Response<List<User>>> FetchFriends()
        {
            var response = await HTTPHelper.GetDataAsync<Response<List<User>>>($"/Friendship/GetFriends", Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchFriends Error: " + response.Message);
                return response;
            }

            UpdateFriendChange(response.Data);

            return response;
        }
        public async Task<Response<Dictionary<string, DateTime>>> FetchFriendsLastOnlineTime()
        {
            var response = await HTTPHelper.GetDataAsync<Response<Dictionary<string, DateTime>>>($"/Friendship/GetFriendsLastOnlineTime", Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchFriendsLastOnlineTime Error: " + response.Message);
                return response;
            }

            ApplyGroupMembersLastOnlineTimeChange(response.Data);

            return response;
        }

        public async Task<Response<List<Message>>> FetchMessages(string friendId)
        {
            var requestData = new FriendshipRequestParameters
            {
                FriendID = friendId,
            };
            var response = await HTTPHelper.PostDataAsync<Response<List<Message>>>($"/Friendship/GetFriendshipMessages", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchMessages Error: " + response.Message);
                return response;
            }

            UpdateMessageChanges(friendId, response.Data);

            return response;
        }
        public async Task<BaseResponse> SendFriendInvitation(string friendId)
        {
            var requestData = new FriendshipRequestParameters
            {
                FriendID = friendId
            };
            var response = await HTTPHelper.PostDataAsync<BaseResponse>("/Friendship/SendFriendInvitation", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("SendFriendInvitation Error: " + response.Message);
                return response;
            }
            return response;
        }
        public async Task<Response<User>> RemoveFriend(string friendId)
        {
            var requestData = new FriendshipRequestParameters
            {
                FriendID = friendId
            };
            var response = await HTTPHelper.PostDataAsync<Response<User>>("/Friendship/RemoveFriend", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("RemoveFriend Error: " + response.Message);
                return response;
            }

            var friend = response.Data;

            if (_friendsMap.Remove(friendId))
            {
                _friendshipMessages.Remove(friendId);
                _onFriendRemoved?.Invoke(friend);
            }

            return response;
        }
        public async Task<Response<Message>> SendMessage(string friendId, string message, bool silentSend)
        {
            var requestData = new FriendshipRequestParameters
            {
                FriendID = friendId,
                Message = message
            };
            var response = await HTTPHelper.PostDataAsync<Response<Message>>("/Friendship/SendMessage", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("SendMessage Error: " + response.Message);
                return response;
            }

            if (!_onNewMessageAdded.ContainsKey(friendId))
                _onNewMessageAdded.Add(friendId, null);
            if (!_friendshipMessages.ContainsKey(friendId))
                _friendshipMessages.Add(friendId, new Dictionary<string, Message>());

            _friendshipMessages[friendId].Add(response.Data.MessageID, response.Data);
            if (!silentSend)
                _onNewMessageAdded[friendId]?.Invoke(response.Data);

            return response;
        }
        #endregion

        #region Events
        public void AddOnFriendAddedListener(Action<User> onFriendAddedCallback)
        {
            _onFriendAdded += onFriendAddedCallback;
            CheckShouldListenForFriendChanges();
        }
        public void RemoveOnFriendAddedListener(Action<User> onFriendAddedCallback)
        {
            _onFriendAdded -= onFriendAddedCallback;
            CheckShouldListenForFriendChanges();
        }
        public void AddOnFriendRemovedListener(Action<User> onFriendRemovedCallback)
        {
            _onFriendRemoved += onFriendRemovedCallback;
            CheckShouldListenForFriendChanges();
        }
        public void RemoveOnFriendRemovedListener(Action<User> onFriendRemovedCallback)
        {
            _onFriendRemoved -= onFriendRemovedCallback;
            CheckShouldListenForFriendChanges();
        }
        private void CheckShouldListenForFriendChanges()
        {
            _listenForFriendsChanges = _onFriendAdded != null || _onFriendRemoved != null;
        }


        public void AddOnFriendLastOnlineTimeUpdated(Action<User, DateTime> onFriendLastOnlineTimeUpdatedCallback)
        {
            _onFriendLastOnlineTimeUpdated += onFriendLastOnlineTimeUpdatedCallback;
            CheckShouldListenToLastOnlineTimeChanges();
        }
        public void RemoveOnFriendLastOnlineTimeUpdated(Action<User, DateTime> onFriendLastOnlineTimeUpdatedCallback)
        {
            _onFriendLastOnlineTimeUpdated -= onFriendLastOnlineTimeUpdatedCallback;
            CheckShouldListenToLastOnlineTimeChanges();
        }
        private void CheckShouldListenToLastOnlineTimeChanges()
        {
            _listenForFriendsLastOnlineTimeChange = _onFriendLastOnlineTimeUpdated != null;
        }


        public void AddMessageChangeListener(string friendId, Action<Message> onNewMessageAddedCallback)
        {
            if (!_onNewMessageAdded.ContainsKey(friendId))
                _onNewMessageAdded.Add(friendId, null);
            if (!_listenForFriendshipMessages.Contains(friendId))
                _listenForFriendshipMessages.Add(friendId);
            _onNewMessageAdded[friendId] += onNewMessageAddedCallback;
        }
        public void RemoveMessageChangeListener(string friendId, Action<Message> onNewMessageAddedCallback)
        {
            if (!_onNewMessageAdded.ContainsKey(friendId))
                _onNewMessageAdded.Add(friendId, null);

            _onNewMessageAdded[friendId] -= onNewMessageAddedCallback;
            _listenForFriendshipMessages.Remove(friendId);
        }
        #endregion

        #region Periodic Checks
        private async void CheckForChanges()
        {
            if (_listenForFriendsChanges && Time.time - _lastFriendsFetchTime > FriendsChangeCheckInterval)
            {
                _lastFriendsFetchTime = Time.time;
                if (!_isFetchingFriends)
                {
                    DOFriendsFetch();
                }
            }

            if (_listenForFriendsLastOnlineTimeChange && Time.time - _lastFriendsLastOnlineTimeFetchTime > FriendsChangeCheckInterval)
            {
                _lastFriendsLastOnlineTimeFetchTime = Time.time;
                if (!_isFetchingFriendsLastOnlineTime)
                {
                    DOFriendsLastOnlineTimeFetch();
                }
            }

            foreach (var friendId in _listenForFriendshipMessages)
            {
                if (!_lastFriendshipMessagesFetchTime.ContainsKey(friendId))
                    _lastFriendshipMessagesFetchTime.Add(friendId, Time.time);

                if (Time.time - _lastFriendshipMessagesFetchTime[friendId] > MessageChangeCheckInterval)
                {
                    if (!_isFetchingFriendshipMessages.ContainsKey(friendId))
                        _isFetchingFriendshipMessages.Add(friendId, false);

                    _lastFriendshipMessagesFetchTime[friendId] = Time.time;
                    if (!_isFetchingFriendshipMessages[friendId])
                    {
                        DOFriendshipMessageFetch(friendId);
                    }
                }
            }
        }
        private async void DOFriendshipMessageFetch(string friendId)
        {
            _isFetchingFriendshipMessages[friendId] = true;
            await FetchMessages(friendId);
            _isFetchingFriendshipMessages[friendId] = false;
        }
        private async void DOFriendsFetch()
        {
            _isFetchingFriends = true;
            await FetchFriends();
            _isFetchingFriends = false;
        }
        private async void DOFriendsLastOnlineTimeFetch()
        {
            _isFetchingFriendsLastOnlineTime = true;
            await FetchFriendsLastOnlineTime();
            _isFetchingFriendsLastOnlineTime = false;
        }

        #endregion

        #region Apply Changes Section
        private void UpdateFriendChange(List<User> newFriends)
        {
            var storedFriends = _friendsMap.Values.ToList();
            var friendsToRemove = storedFriends.Except(newFriends);
            var friendsToAdd = newFriends.Except(storedFriends);

            foreach (var friendToRemove in friendsToRemove)
            {
                if (_friendsMap.Remove(friendToRemove.UserID))
                {
                    _onFriendRemoved?.Invoke(friendToRemove);
                }
            }
            foreach (var friendToAdd in friendsToAdd)
            {
                if (!_friendsMap.ContainsKey(friendToAdd.UserID))
                {
                    _friendsMap.Add(friendToAdd.UserID, friendToAdd);
                    _onFriendAdded?.Invoke(friendToAdd);
                }
            }
        }
        private void UpdateMessageChanges(string friendId, List<Message> newMessages)
        {
            if (!_friendshipMessages.ContainsKey(friendId))
                _friendshipMessages.Add(friendId, new Dictionary<string, Message>());

            var storedMessageList = _friendshipMessages[friendId].Values.ToList();
            var messagesToAdd = newMessages.Except(storedMessageList);

            foreach (var messageToAdd in messagesToAdd)
            {
                _friendshipMessages[friendId].Add(messageToAdd.MessageID, messageToAdd);
                _onNewMessageAdded[friendId]?.Invoke(messageToAdd);
            }
        }
        private void ApplyGroupMembersLastOnlineTimeChange(Dictionary<string, DateTime> newTimes)
        {
            foreach (var friendNewTimes in newTimes)
            {
                if (!_friendsMap.ContainsKey(friendNewTimes.Key)) return;
                var friend = _friendsMap[friendNewTimes.Key];
                friend.LastOnlineTime = friendNewTimes.Value;
                _onFriendLastOnlineTimeUpdated?.Invoke(friend, friendNewTimes.Value);
            }
        }
        #endregion

    }
}