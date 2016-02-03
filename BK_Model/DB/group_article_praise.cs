
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class group_article_praise
    {
        [Key]
        public long id { get; set; }
        public long articleid { get; set; }
        public Nullable<System.DateTime> praisetime { get; set; }
        public System.Guid useraccount_uuid { get; set; }
    }
}
