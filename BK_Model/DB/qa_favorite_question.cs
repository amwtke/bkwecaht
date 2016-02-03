
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class qa_favorite_question
    {
        [Key]
        public long id { get; set; }
        public Nullable<long> fav_question { get; set; }
        public Nullable<System.DateTime> add_time { get; set; }
        public System.Guid user_account_uuid { get; set; }
    }
}
