
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ShieldingText
    {
        [Key]
        public long ID { get; set; }
        public string Text { get; set; }
    }
}
