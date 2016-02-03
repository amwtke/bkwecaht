
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class OtherExperiments
    {
        [Key]
        public long Id { get; set; }
        public string AccountEmail { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDetails { get; set; }
    }
}
