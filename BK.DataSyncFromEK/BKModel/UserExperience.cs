
namespace BK.DataSyncFromEK.BKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserExperience: IDBModelWithID
    {
        //��λ����
        public string WorkUnit { get; set; }
        //ְλ
        public string Position { get; set; }
        //��ʼʱ��
        public DateTime? StartTime { get; set; }
        //����ʱ��
        public DateTime? EndTime { get; set; }
        //����
        public string Address { get; set; }
        //����
        public bool? IsUpToNow { get; set; }
        //����
        public string Country { get; set; }
        //ʡ
        public string Province { get; set; }
        //��
        public string City { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
    }
}
