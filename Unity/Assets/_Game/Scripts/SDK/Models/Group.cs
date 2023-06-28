using System.Collections.Generic;

namespace SDK.Models
{
    public class Group
    {
        public string GroupID { get; set; }
        public string Name { get; set; }
        public string CreatorUID { get; set; }
        public List<string> Members { get; set; }

        public bool Equals(Group other) => GroupID == other.GroupID;
        public override bool Equals(object obj) => Equals(obj as Group);
        public override int GetHashCode() => (GroupID, Name, CreatorUID).GetHashCode();
    }
}
