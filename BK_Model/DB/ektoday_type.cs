
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ektoday_type
    {
        [Key]
        public long id { get; set; }
        public string ektoday_type1 { get; set; }
    }
}
