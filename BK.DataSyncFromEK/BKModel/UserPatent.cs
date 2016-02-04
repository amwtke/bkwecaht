
namespace BK.DataSyncFromEK.BKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserPatent: IDBModelWithID
    {
        //专利名
        public string PatentName { get; set; }
        //注册时间
        public DateTime? RegistTime { get; set; }
        //注册地点
        public string RegistAddress { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
    }
}
