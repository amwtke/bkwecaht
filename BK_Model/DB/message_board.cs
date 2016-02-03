
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class message_board
    {
        [Key]
        public long id { get; set; }
        public string news_info { get; set; }
        public System.DateTime create_time { get; set; }
        public Nullable<bool> IsAnonymity { get; set; }
        public System.Guid account_email_uuid { get; set; }
        public System.Guid creator_uuid { get; set; }
    }
}
