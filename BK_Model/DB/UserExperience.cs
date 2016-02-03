
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserExperience
    {
        //单位名称
        public string WorkUnit { get; set; }
        //职位
        public string Position { get; set; }
        //开始时间
        public Nullable<System.DateTime> StartTime { get; set; }
        //结束时间
        public Nullable<System.DateTime> EndTime { get; set; }
        //留空
        public string Address { get; set; }
        //至今
        public Nullable<bool> IsUpToNow { get; set; }
        //国家
        public string Country { get; set; }
        //省
        public string Province { get; set; }
        //市
        public string City { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
