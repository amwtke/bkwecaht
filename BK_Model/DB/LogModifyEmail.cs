
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class LogModifyEmail
    {
        [Key]
        public long Id { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
        public long Uid { get; set; }
    }
}
