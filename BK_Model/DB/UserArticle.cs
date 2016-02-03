
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserArticle
    {
        //����
        public string Title { get; set; }
        //����
        public string Author { get; set; }
        //����ʱ��
        public DateTime? PublishTime { get; set; }
        //ԭ�����ص�ַ Ĭ�Ͽ� ��ֵ�Ļ� �ճ�
        public string ArticlePath { get; set; }
        //�ڿ���
        public string PostMagazine { get; set; }
        //���� ����
        public int? StartPage { get; set; }
        //���� ����
        public int? EndPage { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
        public string Status { get; set; }
    }

    public enum PaperStatus
    {
        Added=1,
        Deleted=0,
    }
}
