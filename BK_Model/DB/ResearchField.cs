
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ResearchField")]
    public partial class ResearchField
    {
        [Key]
        public long ID { get; set; }
        public string FieldName { get; set; }
        public Nullable<long> FatherID { get; set; }
    }
}
