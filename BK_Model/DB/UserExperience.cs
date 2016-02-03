
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserExperience
    {
        //��λ����
        public string WorkUnit { get; set; }
        //ְλ
        public string Position { get; set; }
        //��ʼʱ��
        public Nullable<System.DateTime> StartTime { get; set; }
        //����ʱ��
        public Nullable<System.DateTime> EndTime { get; set; }
        //����
        public string Address { get; set; }
        //����
        public Nullable<bool> IsUpToNow { get; set; }
        //����
        public string Country { get; set; }
        //ʡ
        public string Province { get; set; }
        //��
        public string City { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
