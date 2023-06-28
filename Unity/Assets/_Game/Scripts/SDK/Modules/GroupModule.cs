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
    public class GroupModule : BaseModule
    {
        private const float GroupsChangeCheckInterval = 5;
        private const float GroupDataChangeCheckInterval = 3;
        private const float GroupMembersLastOnlineTimeChangeCheckInterval = 10;

        private Dictionary<string, Group> _groupsMap = new Dictionary<string, Group>();
        private Dictionary<string, Dictionary<string, Message>> _groupsMessagesMap = new Dictionary<string, Dictionary<string, Message>>(); // Caching messages. 
        private Dictionary<string, Dictionary<string, User>> _groupsMembersMap = new Dictionary<string, Dictionary<string, User>>(); // Caching messages. 

        private Action<Group> _onGroupAdded;
        private Action<Group> _onGroupRemoved;
        private readonly Dictionary<string, Action<Message>> _onMessageAdded = new Dictionary<string, Action<Message>>();
        private readonly Dictionary<string, Action<User>> _onMemberAdded = new Dictionary<string, Action<User>>();
        private readonly Dictionary<string, Action<User>> _onMemberRemoved = new Dictionary<string, Action<User>>();
        private readonly Dictionary<string, Action<User, DateTime>> _onMemberLastOnlineTimeUpdated = new Dictionary<string, Action<User, DateTime>>();

        private List<string> _listenForGroupDataChange = new List<string>();
        private Dictionary<string, bool> _isFetchingGroupDataMap = new Dictionary<string, bool>();
        private Dictionary<string, float> _lastGroupDataFetchTime = new Dictionary<string, float>();

        private List<string> _listenForGroupMembersLastOnlineTimeChange = new List<string>();
        private Dictionary<string, bool> _isFetchingGroupMembersLastOnlineTimeMap = new Dictionary<string, bool>();
        private Dictionary<string, float> _lastGroupMembersLastOnlineTimeetchTime = new Dictionary<string, float>();

        private bool _listenForGroupsChange;
        private bool _isFetchingGroups;
        private float _lastGroupsFetchTime;

        #region Inherited Methods
        public override void OnUpdate()
        {
            CheckForChanges();
        }
        #endregion

        #region Stored Data Getters
        public List<Group> GetStoredGroups()
        {
            return _groupsMap.Values.ToList();//TODO: Make GC free
        }
        public List<User> GetStoredMembers(string groupId)
        {
            if (!_groupsMembersMap.ContainsKey(groupId)) return new List<User>();
            return _groupsMembersMap[groupId].Values.ToList();
        }
        public List<Message> GetStoredMessages(string groupId)
        {
            if (!_groupsMessagesMap.ContainsKey(groupId)) return new List<Message>();
            return _groupsMessagesMap[groupId].Values.ToList();
        }
        #endregion

        #region API Calls
        public async Task<Response<List<Group>>> FetchGroups()
        {
            var response = await HTTPHelper.GetDataAsync<Response<List<Group>>>($"/Group/GetGroups", Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchGroups Error: " + response.Message);
                return response;
            }

            ApplyGroupChanges(response.Data);

            return response;
        }
        public async Task<Response<Dictionary<string, DateTime>>> FetchGroupMembersLastOnlineTime(string groupId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
            };
            var response = await HTTPHelper.PostDataAsync<Response<Dictionary<string, DateTime>>>($"/Group/GetGroupMembersLastOnlineTime", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchGroupMembersLastOnlineTime Error: " + response.Message);
                return response;
            }

            ApplyGroupMembersLastOnlineTimeChange(groupId, response.Data);

            return response;
        }
        public async Task<Response<List<User>>> FetchGroupMembers(string groupId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
            };
            var response = await HTTPHelper.PostDataAsync<Response<List<User>>>($"/Group/GetGroupMembers", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchGroupMembers Error: " + response.Message);
                return response;
            }

            ApplyGroupMembersChange(groupId, response.Data);

            return response;
        }
        public async Task<Response<List<Message>>> FetchGroupMessages(string groupId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
            };
            var response = await HTTPHelper.PostDataAsync<Response<List<Message>>>($"/Group/GetGroupMessages", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("FetchGroupMessages Error: " + response.Message);
                return response;
            }

            ApplyGroupMessagesChange(groupId, response.Data);

            return response;
        }
        public async Task<BaseResponse> SendGroupInvitation(string groupId, string userId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
                MemberID = userId
            };
            var response = await HTTPHelper.PostDataAsync<BaseResponse>("/Group/SendGroupInvitation", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("SendGroupInvitation Error: " + response.Message);
                return response;
            }
            return response;
        }
        public async Task<Response<Group>> LeaveGroup(string groupId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId
            };
            var response = await HTTPHelper.PostDataAsync<Response<Group>>("/Group/LeaveGroup", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("LeaveGroup Error: " + response.Message);
                return response;
            }

            var group = response.Data;

            if (_groupsMap.Remove(groupId))
            {
                _groupsMessagesMap.Remove(groupId);
                _onGroupRemoved?.Invoke(group);
            }

            return response;
        }
        public async Task<Response<Message>> SendMessage(string groupId, string message, bool silentSend)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
                Message = message
            };
            var response = await HTTPHelper.PostDataAsync<Response<Message>>("/Group/SendMessage", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("SendMessage (Group) Error: " + response.Message);
                return response;
            }

            if (!_onMessageAdded.ContainsKey(groupId))
                _onMessageAdded.Add(groupId, null);
            if (!_groupsMessagesMap.ContainsKey(groupId))
                _groupsMessagesMap.Add(groupId, new Dictionary<string, Message>());

            _groupsMessagesMap[groupId].Add(response.Data.MessageID, response.Data);
            if (!silentSend)
                _onMessageAdded[groupId]?.Invoke(response.Data);

            return response;
        }

        public async Task<Response<Group>> CreateGroup(string groupName)
        {
            var requestData = new GroupRequestParameters
            {
                GroupName = groupName,
            };
            var response = await HTTPHelper.PostDataAsync<Response<Group>>("/Group/CreateGroup", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("CreateGroup Error: " + response.Message);
                return response;
            }

            GameDebugger.Log(response.Data.GroupID);

            Group group = response.Data;
            _groupsMap.Add(group.GroupID, group);

            return response;
        }
        public async Task<Response<User>> KickMember(string groupId, string memberId)
        {
            var requestData = new GroupRequestParameters
            {
                GroupID = groupId,
                MemberID = memberId
            };
            var response = await HTTPHelper.PostDataAsync<Response<User>>("/Group/KickMember", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log("KickMember Error: " + response.Message);
                return response;
            }

            var member = response.Data;

            if (_groupsMembersMap[groupId].Remove(memberId))
            {
                _groupsMembersMap[groupId].Remove(memberId);
                _onMemberRemoved[groupId]?.Invoke(member);
            }

            return response;
        }
        #endregion

        #region Events
        public void AddOnGroupAddedListener(Action<Group> onGroupAddedCallback)
        {
            _onGroupAdded += onGroupAddedCallback;
            CheckShouldListenToGroupsChange();
        }
        public void RemoveOnGroupAddedListener(Action<Group> onGroupAddedCallback)
        {
            _onGroupAdded -= onGroupAddedCallback;
            CheckShouldListenToGroupsChange();
        }
        public void AddOnGroupRemovedListener(Action<Group> onGroupRemovedCallback)
        {
            _onGroupRemoved += onGroupRemovedCallback;
            CheckShouldListenToGroupsChange();
        }
        public void RemoveOnGroupRemovedListener(Action<Group> onGroupRemovedCallback)
        {
            _onGroupRemoved -= onGroupRemovedCallback;
            CheckShouldListenToGroupsChange();
        }
        private void CheckShouldListenToGroupsChange()
        {
            _listenForGroupsChange = _onGroupAdded != null || _onGroupRemoved != null;
        }

        public void AddOnMessageAddedListener(string groupId, Action<Message> onNewMessageAddedCallback)
        {
            InitGroupMessagesChangeEvents(groupId);
            _onMessageAdded[groupId] += onNewMessageAddedCallback;
            CheckShouldListenToMessagesChanges(groupId);
        }
        public void RemoveOnMessageAddedListener(string groupId, Action<Message> onNewMessageAddedCallback)
        {
            InitGroupMessagesChangeEvents(groupId);
            _onMessageAdded[groupId] -= onNewMessageAddedCallback;
            CheckShouldListenToMessagesChanges(groupId);
        }
        private void InitGroupMessagesChangeEvents(string groupId)
        {
            if (!_onMessageAdded.ContainsKey(groupId))
                _onMessageAdded.Add(groupId, null);
        }
        private void CheckShouldListenToMessagesChanges(string groupId)
        {
            if (_onMessageAdded[groupId] == null)
            {
                _listenForGroupDataChange.Remove(groupId);
            }
            else
            {
                if (!_listenForGroupDataChange.Contains(groupId))
                    _listenForGroupDataChange.Add(groupId);
            }
        }

        public void AddOnMemberLastOnlineTimeUpdated(string groupId, Action<User, DateTime> onMemberLastOnlineTimeUpdatedCallback)
        {
            InitMemberLastOnlineChangeEvents(groupId);
            _onMemberLastOnlineTimeUpdated[groupId] += onMemberLastOnlineTimeUpdatedCallback;
            CheckShouldListenToLastOnlineTimeChanges(groupId);
        }
        public void RemoveOnMemberLastOnlineTimeUpdated(string groupId, Action<User, DateTime> onMemberLastOnlineTimeUpdatedCallback)
        {
            InitMemberLastOnlineChangeEvents(groupId);
            _onMemberLastOnlineTimeUpdated[groupId] -= onMemberLastOnlineTimeUpdatedCallback;
            CheckShouldListenToLastOnlineTimeChanges(groupId);
        }
        private void InitMemberLastOnlineChangeEvents(string groupId)
        {
            if (!_onMemberLastOnlineTimeUpdated.ContainsKey(groupId))
                _onMemberLastOnlineTimeUpdated.Add(groupId, null);
        }
        private void CheckShouldListenToLastOnlineTimeChanges(string groupId)
        {
            if (_onMemberLastOnlineTimeUpdated[groupId] == null)
            {
                _listenForGroupMembersLastOnlineTimeChange.Remove(groupId);
            }
            else
            {
                if (!_listenForGroupMembersLastOnlineTimeChange.Contains(groupId))
                    _listenForGroupMembersLastOnlineTimeChange.Add(groupId);
            }
        }

        public void AddOnMemberAddedListener(string groupId, Action<User> onMemberAddedCallback)
        {
            InitGroupMemberChangeEvents(groupId);
            _onMemberAdded[groupId] += onMemberAddedCallback;
            CheckShouldListenToMemberChanges(groupId);
        }
        public void RemoveOnMemberAddedListener(string groupId, Action<User> onMemberAddedCallback)
        {
            InitGroupMemberChangeEvents(groupId);
            _onMemberAdded[groupId] -= onMemberAddedCallback;
            CheckShouldListenToMemberChanges(groupId);
        }
        public void AddOnMemberRemovedListener(string groupId, Action<User> onMemberRemovedCallback)
        {
            InitGroupMemberChangeEvents(groupId);
            _onMemberRemoved[groupId] += onMemberRemovedCallback;
            CheckShouldListenToMemberChanges(groupId);
        }
        public void RemoveOnMemberRemovedListener(string groupId, Action<User> onMemberRemovedCallback)
        {
            InitGroupMemberChangeEvents(groupId);
            _onMemberRemoved[groupId] -= onMemberRemovedCallback;
            CheckShouldListenToMemberChanges(groupId);
        }
        private void InitGroupMemberChangeEvents(string groupId)
        {
            if (!_onMemberAdded.ContainsKey(groupId))
                _onMemberAdded.Add(groupId, null);

            if (!_onMemberRemoved.ContainsKey(groupId))
                _onMemberRemoved.Add(groupId, null);
        }
        private void CheckShouldListenToMemberChanges(string groupId)
        {
            if (_onMemberAdded[groupId] == null && _onMemberRemoved[groupId] == null)
            {
                _listenForGroupDataChange.Remove(groupId);
            }
            else
            {
                if (!_listenForGroupDataChange.Contains(groupId))
                {
                    _listenForGroupDataChange.Add(groupId);
                }
            }
        }
        #endregion

        #region Periodic Checks
        private async void CheckForChanges()
        {
            if (_listenForGroupsChange && Time.time - _lastGroupsFetchTime > GroupsChangeCheckInterval)
            {
                _lastGroupsFetchTime = Time.time;
                if (!_isFetchingGroups)
                {
                    DOGroupsFetch();
                }
            }

            foreach (var groupId in _listenForGroupDataChange)
            {
                if (!_lastGroupDataFetchTime.ContainsKey(groupId))
                    _lastGroupDataFetchTime.Add(groupId, Time.time);

                if (Time.time - _lastGroupDataFetchTime[groupId] > GroupDataChangeCheckInterval)
                {
                    if (!_isFetchingGroupDataMap.ContainsKey(groupId))
                        _isFetchingGroupDataMap.Add(groupId, false);

                    _lastGroupDataFetchTime[groupId] = Time.time;
                    if (!_isFetchingGroupDataMap[groupId])
                    {
                        DOGroupDataFetch(groupId);
                    }
                }
            }

            foreach (var groupId in _listenForGroupMembersLastOnlineTimeChange)
            {
                if (!_lastGroupMembersLastOnlineTimeetchTime.ContainsKey(groupId))
                    _lastGroupMembersLastOnlineTimeetchTime.Add(groupId, Time.time);

                if (Time.time - _lastGroupMembersLastOnlineTimeetchTime[groupId] > GroupMembersLastOnlineTimeChangeCheckInterval)
                {
                    if (!_isFetchingGroupMembersLastOnlineTimeMap.ContainsKey(groupId))
                        _isFetchingGroupMembersLastOnlineTimeMap.Add(groupId, false);

                    _lastGroupMembersLastOnlineTimeetchTime[groupId] = Time.time;
                    if (!_isFetchingGroupMembersLastOnlineTimeMap[groupId])
                    {
                        DOMembersLastOnlineTimeFetch(groupId);
                    }
                }
            }
        }
        private async void DOGroupsFetch()
        {
            _isFetchingGroups = true;
            await FetchGroups();
            _isFetchingGroups = false;
        }
        private async void DOGroupDataFetch(string groupId)
        {
            _isFetchingGroupDataMap[groupId] = true;
            var fetchTask1 = FetchGroupMembers(groupId);
            var fetchTask2 = FetchGroupMessages(groupId);
            await fetchTask1; await fetchTask2;
            _isFetchingGroupDataMap[groupId] = false;
        }
        private async void DOMembersLastOnlineTimeFetch(string groupId)
        {
            _isFetchingGroupMembersLastOnlineTimeMap[groupId] = true;
            await FetchGroupMembersLastOnlineTime(groupId);
            _isFetchingGroupMembersLastOnlineTimeMap[groupId] = false;
        }
        #endregion

        #region Apply Changes Section
        private void ApplyGroupChanges(List<Group> newGroups)
        {
            var storedGroups = _groupsMap.Values.ToList();
            var groupsToRemove = storedGroups.Except(newGroups);
            var groupsToAdd = newGroups.Except(storedGroups);

            foreach (var groupToRemove in groupsToRemove)
            {
                if (_groupsMap.Remove(groupToRemove.GroupID))
                {
                    _onGroupRemoved?.Invoke(groupToRemove);
                }
            }
            foreach (var groupToAdd in groupsToAdd)
            {
                if (!_groupsMap.ContainsKey(groupToAdd.GroupID))
                {
                    _groupsMap.Add(groupToAdd.GroupID, groupToAdd);
                    _onGroupAdded?.Invoke(groupToAdd);
                }
            }
        }
        private void ApplyGroupMembersChange(string groupId, List<User> newMembers)
        {
            if (!_groupsMembersMap.ContainsKey(groupId))
                _groupsMembersMap.Add(groupId, new Dictionary<string, User>());

            var storedMembers = _groupsMembersMap[groupId].Values.ToList();
            var membersToRemove = storedMembers.Except(newMembers);
            var membersToAdd = newMembers.Except(storedMembers);

            foreach (var memberToRemove in membersToRemove)
            {
                if (_groupsMembersMap[groupId].Remove(memberToRemove.UserID))
                {
                    _onMemberRemoved[groupId]?.Invoke(memberToRemove);
                }
            }
            foreach (var memberToAdd in membersToAdd)
            {
                _groupsMembersMap[groupId].Add(memberToAdd.UserID, memberToAdd);
                _onMemberAdded[groupId]?.Invoke(memberToAdd);
            }
        }
        private void ApplyGroupMessagesChange(string groupId, List<Message> newMessages)
        {
            if (!_groupsMessagesMap.ContainsKey(groupId))
                _groupsMessagesMap.Add(groupId, new Dictionary<string, Message>());

            var storedMessageList = _groupsMessagesMap[groupId].Values.ToList();
            var messagesToAdd = newMessages.Except(storedMessageList);

            foreach (var messageToAdd in messagesToAdd)
            {
                _groupsMessagesMap[groupId].Add(messageToAdd.MessageID, messageToAdd);
                _onMessageAdded[groupId]?.Invoke(messageToAdd);
            }
        }
        private void ApplyGroupMembersLastOnlineTimeChange(string groupId, Dictionary<string, DateTime> newTimes)
        {
            if (!_onMemberLastOnlineTimeUpdated.ContainsKey(groupId))
                _onMemberLastOnlineTimeUpdated.Add(groupId, null);

            foreach (var memberNewTime in newTimes)
            {
                if (!_groupsMembersMap[groupId].ContainsKey(memberNewTime.Key)) return;
                var member = _groupsMembersMap[groupId][memberNewTime.Key];
                member.LastOnlineTime = memberNewTime.Value;
                _onMemberLastOnlineTimeUpdated[groupId]?.Invoke(member, memberNewTime.Value);
            }
        }

        #endregion
    }
}