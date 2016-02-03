
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class CopUserExperiment
    {
        public string Name { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> UseDays { get; set; }
        public string Details { get; set; }
        [Key]
        public long Id { get; set; }
        public string PriceUnit { get; set; }
        public string DaysUnit { get; set; }
        public string Currency { get; set; }
        public Nullable<bool> IsRecommend { get; set; }
        public Nullable<System.DateTime> RecommendTime { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
