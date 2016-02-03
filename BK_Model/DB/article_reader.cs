
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class article_reader
    {
        [Key]
        public long id { get; set; }
        public long articleid { get; set; }
        public Nullable<System.DateTime> readtime { get; set; }
        public System.Guid useraccount_uuid { get; set; }
    }
}
