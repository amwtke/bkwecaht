
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Country
    {
        [Key]
        public long Id { get; set; }
        public string CountryName { get; set; }
    }
}
