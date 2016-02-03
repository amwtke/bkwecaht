
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ekevent_register
    {
        [Key]
        public long id { get; set; }
        public long eventid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string workplace { get; set; }
        public string ps { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
    }
}
