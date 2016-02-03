
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class EquipmentName
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
