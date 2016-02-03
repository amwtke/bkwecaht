
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class article_request
    {
        [Key]
        public long id { get; set; }
        public string article_id { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> create_time { get; set; }
        public System.Guid user_host_uuid { get; set; }
        public System.Guid user_guest_uuid { get; set; }
    }
}
