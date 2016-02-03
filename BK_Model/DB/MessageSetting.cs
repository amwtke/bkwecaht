
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class MessageSetting
    {
        public string Text { get; set; }
        public Nullable<int> Type { get; set; }
        [Key]
        public long ID { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
