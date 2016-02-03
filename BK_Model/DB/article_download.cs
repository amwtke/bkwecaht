
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class article_download
    {
        [Key]
        public long id { get; set; }
        public Nullable<long> article_id { get; set; }
        public Nullable<System.DateTime> create_time { get; set; }
        public System.Guid user_host_uuid { get; set; }
        public System.Guid user_guest_uuid { get; set; }
    }
}
