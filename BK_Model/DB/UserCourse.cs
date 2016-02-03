
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class UserCourse
    {
        [Key]
        public long Id
        { get; set; }
        //����
        public string CourseName
        { get; set; }
        public DateTime? CreateTime
        { get; set; }
        public Guid AccountEmail_uuid
        { get; set; }
    }
}
