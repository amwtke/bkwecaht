
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class recruit_student
    {
        [Key]
        public long id { get; set; }
        public string teacher { get; set; }
        public string student { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public Nullable<bool> available { get; set; }
    }
}
