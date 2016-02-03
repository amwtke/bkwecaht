using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.DB
{
    [Table("BK_Configuration")]
    public class BKConfigItem
    {
        [Key]
        public int Id { get; set; }
        public string Domain { get; set; }

        public string Module { get; set; }
        public string Function { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
