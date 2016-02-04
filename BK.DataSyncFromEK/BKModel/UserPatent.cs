
namespace BK.DataSyncFromEK.BKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserPatent: IDBModelWithID
    {
        //ר����
        public string PatentName { get; set; }
        //ע��ʱ��
        public DateTime? RegistTime { get; set; }
        //ע��ص�
        public string RegistAddress { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
    }
}
