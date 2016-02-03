
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ExperimentName
    {
        [Key]
        public long Id { get; set; }
        public string ExpName { get; set; }
    }
}
