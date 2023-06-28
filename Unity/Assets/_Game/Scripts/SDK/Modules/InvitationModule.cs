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
    public class InvitationModule : BaseModule
    {
        private const float InvitationAddedChangeCheckInterval = 2;

        //  Only storing Pending invitations
        private Dictionary<string, Invitation> _invitations = new Dictionary<string, Invitation>();

        private Action<Invitation> _onInvitationAdded;
        private bool _listenForInvitationAdded;
        private bool _isFetchingInvitations;
        private float _lastInvitationAddedCheckTime;

        #region Inherited Methods
        public override void OnUpdate()
        {
            CheckForChanges();
        }
        #endregion

        #region Stored Data Getters
        public List<Invitation> GetStoredInvitations()
        {
            return _invitations.Values.ToList();//TODO: Make GC free
        }
        #endregion

        #region API Calls
        public async Task<Response<List<Invitation>>> FetchInvitations()
        {
            var response = await HTTPHelper.GetDataAsync<Response<List<Invitation>>>($"/Invitation/GetPendingInvitations", Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log($"FetchInvitations Error: {response.Message}");
                return response;
            }

            UpdateInvitationsChange(response.Data);

            return response;
        }
        public async Task<BaseResponse> AcceptInvitation(string invitationId)
        {
            var requestData = new InvitationRequestParameters
            {
                InvitationID = invitationId
            };
            var response = await HTTPHelper.PostDataAsync<BaseResponse>("/Invitation/AcceptInvitation", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log($"AcceptInvitation Error: {response.Message}");
                return response;
            }

            _invitations.Remove(invitationId);

            return response;
        }
        public async Task<BaseResponse> RejectInvitation(string invitationId)
        {
            var requestData = new InvitationRequestParameters
            {
                InvitationID = invitationId
            };
            var response = await HTTPHelper.PostDataAsync<BaseResponse>("/Invitation/RejectInvitation", requestData, Client.CurrentIdToken);

            if (!response.Success)
            {
                GameDebugger.Log($"RejectInvitation Error: {response.Message}");
                return response;
            }

            _invitations.Remove(invitationId);

            return response;
        }
        #endregion

        #region Events
        public void AddOnInvitationAddedListener(Action<Invitation> onInvitationAddedCallback)
        {
            _onInvitationAdded += onInvitationAddedCallback;
            CheckShouldListenToInvitationsChange();
        }
        public void RemoveOnInvitationAddedListener(Action<Invitation> onInvitationAddedCallback)
        {
            _onInvitationAdded -= onInvitationAddedCallback;
            CheckShouldListenToInvitationsChange();
        }
        private void CheckShouldListenToInvitationsChange()
        {
            _listenForInvitationAdded = _onInvitationAdded != null;
        }
        #endregion

        #region Preiodic Checks
        private async void CheckForChanges()
        {
            if (_listenForInvitationAdded && Time.time - _lastInvitationAddedCheckTime > InvitationAddedChangeCheckInterval)
            {
                _lastInvitationAddedCheckTime = Time.time;
                if (!_isFetchingInvitations)
                {
                    DOInvitationFetch();
                }
            }
        }
        private async void DOInvitationFetch()
        {
            _isFetchingInvitations = true;
            await FetchInvitations();
            _isFetchingInvitations = false;
        }
        #endregion

        #region Apply Changes Section
        private void UpdateInvitationsChange(List<Invitation> newInvitations)
        {
            var storedFriends = _invitations.Values.ToList();
            var invitationsToAdd = newInvitations.Except(storedFriends);

            foreach (var invitationToAdd in invitationsToAdd)
            {
                if (!_invitations.ContainsKey(invitationToAdd.InvitationID))
                {
                    _invitations.Add(invitationToAdd.InvitationID, invitationToAdd);
                    _onInvitationAdded?.Invoke(invitationToAdd);
                }
            }
        }
        #endregion
    }
}