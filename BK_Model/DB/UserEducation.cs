
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserEducation
    {
        //学校
        public string School { get; set; }
        //学位 1234
        public Nullable<short> Degree { get; set; }
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
        //专业
        public string Professional { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
