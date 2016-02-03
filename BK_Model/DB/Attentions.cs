
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Attentions
    {
        [Key]
        public long Id { get; set; }
        public Nullable<System.DateTime> AttentionTime { get; set; }
        public System.Guid ToAccountEmail_uuid { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
