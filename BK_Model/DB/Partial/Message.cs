using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BK.Model.DB
{
    public partial class Message
    {
        [NotMapped]
        public virtual object RelationObj { get; set; }
    }
}
