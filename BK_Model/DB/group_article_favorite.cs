
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class group_article_favorite
    {
        [Key]
        public long id { get; set; }
        public Nullable<long> user_fav_article { get; set; }
        public Nullable<System.DateTime> add_time { get; set; }
        public System.Guid user_account_uuid { get; set; }
    }
}
