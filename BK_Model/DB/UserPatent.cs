
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserPatent
    {
        //ר����
        public string PatentName { get; set; }
        //ע��ʱ��
        public Nullable<System.DateTime> RegistTime { get; set; }
        //ע��ص�
        public string RegistAddress { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
