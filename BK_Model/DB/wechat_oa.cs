
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class wechat_oa
    {
        [Key]
        public System.Guid uuid { get; set; }
        public string wechat_openid { get; set; }
        public string wechat_unionid { get; set; }
        public DateTime oadate { get; set; }
    }
}
