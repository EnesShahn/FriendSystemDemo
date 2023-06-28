using System.ComponentModel.DataAnnotations;

namespace Server.Models.Requests
{
    public class GroupRequestParameters
    {
        public string? GroupID { get; set; }
        public string? GroupName { get; set; }
        public string? MemberID { get; set; }
        public string? Message { get; set; }

    }
}
