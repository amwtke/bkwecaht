
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserAcademic
    {
        [Key]
        public long Id { get; set; }
        public string Tutor { get; set; }
        public string Association { get; set; }
        public string AssociationPost { get; set; }
        public string Magazine { get; set; }
        public string MagazinePost { get; set; }
        public string Fund { get; set; }
        public string FundPost { get; set; }
        public string Academician { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
