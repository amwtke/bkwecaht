
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class businessrecruit_info
    {
        [Key]
        public long id { get; set; }
        public string accountemail { get; set; }
        public string name { get; set; }
        public string @abstract { get; set; }
        public string country { get; set; }
        public string university { get; set; }
        public string headpic { get; set; }
        public string bodytext { get; set; }
        public string researchfieldstr { get; set; }
        public Nullable<long> researchfield { get; set; }
        public Nullable<System.DateTime> recommend { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
    }
}
