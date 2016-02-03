
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserPatent
    {
        //专利名
        public string PatentName { get; set; }
        //注册时间
        public Nullable<System.DateTime> RegistTime { get; set; }
        //注册地点
        public string RegistAddress { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
