namespace Server
{
    public static class CollectionConstants
    {
        public const string UsersCollection = "Users";
        public const string GroupsCollections = "Groups";
        public const string InvitationCollection = "Invitations";
        public const string FriendshipCollection = "Friendships";
        public const string MessagesCollection = "Messages";
    }
    public static class FieldConstants
    {
        public const string User1ID = "User1ID";
        public const string User2ID = "User2ID";

		public const string SenderID = "SenderID";
		public const string ReceiverID = "ReceiverID";
		public const string InvitationStatus = "Status";

		public const string GroupCreatorID = "CreatorID";
		public const string GroupMembers = "Members";

		public const string CreateTime = "CreateTime";
		public const string LastOnlineTime = "LastOnlineTime";
	}
}