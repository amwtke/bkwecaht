
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class UserSkill
    {
        [Key]
        public long Id
        { get; set; }
        //Ãû³Æ
        public string SkillName
        { get; set; }
        public DateTime? CreateTime
        { get; set; }
        public Guid AccountEmail_uuid
        { get; set; }
    }
}
