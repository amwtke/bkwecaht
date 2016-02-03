
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ExperimentSolution
    {
        [Key]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string ServiceAccount { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string PriceUnit { get; set; }
        public Nullable<int> Days { get; set; }
        public string DaysUnit { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string Currency { get; set; }
        public string SolutionDetails { get; set; }
    }
}
