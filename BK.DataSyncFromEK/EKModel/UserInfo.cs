
namespace BK.DataSyncFromEK.EKModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserInfo
    {
        [Key]
        public Guid uuid
        { get; set; }
        public short? Degree { get; set; }
        public string AccountEmail { get; set; }
        public string Unit { get; set; }
        public string Faculty { get; set; }
        public string Position { get; set; }
        public string Address { get; set; }
        public string QQ { get; set; }
        public string Phone { get; set; }
        public string UserIntroduction { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public int? IsBusiness { get; set; }
        public bool? IsPass { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Photo { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public bool? IsRecommend { get; set; }
        public DateTime? RecommendTime { get; set; }
        public string Interests { get; set; }
        public long? ResearchFieldId { get; set; }
        public long? SubResearchFieldId { get; set; }
        public DateTime? Birthday { get; set; }
        public string PostCode { get; set; }
        public string IdentifyingCode { get; set; }
        public int? IsLogin { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? LoginTimes { get; set; }
        public int? VisitorNum { get; set; }
        //public short? Enrollment { get; set; }
        //public string HometownCountry { get; set; }
        //public string HometownProvince { get; set; }
        //public string HometownCity { get; set; }
        //public string Gender { get; set; }

    }
}
