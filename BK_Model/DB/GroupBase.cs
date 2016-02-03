
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class GroupBase
    {
        [Key]
        public long Id { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string GroupImage { get; set; }
        public string GroupTags { get; set; }
        public Nullable<int> GroupType { get; set; }
        public Nullable<int> GroupStatus { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public System.Guid GroupOwner_uuid { get; set; }
    }
}
