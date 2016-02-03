
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserArticle
    {
        //标题
        public string Title { get; set; }
        //作者
        public string Author { get; set; }
        //发布时间
        public DateTime? PublishTime { get; set; }
        //原文下载地址 默认空 有值的话 照抄
        public string ArticlePath { get; set; }
        //期刊名
        public string PostMagazine { get; set; }
        //废弃 留空
        public int? StartPage { get; set; }
        //废弃 留空
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
