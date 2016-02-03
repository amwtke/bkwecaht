
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class CopUserEquipment
    {
        public string Name { get; set; }
        [Key]
        public long Id { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Currency { get; set; }
        public string Unit { get; set; }
        public string Details { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
