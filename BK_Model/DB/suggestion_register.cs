
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class suggestion_register
    {
        [Key]
        public int id { get; set; }
        public string accountemail { get; set; }
        public string name { get; set; }
        public string html { get; set; }
        public Nullable<int> validate { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public System.Guid useremail_uuid { get; set; }
    }
}
