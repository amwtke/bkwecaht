using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BK.Model.DB
{
    public partial class UserInfo
    {

        #region 其他后添加属性

        [NotMapped]
        public virtual string DegreeStr
        {
            get
            {
                if (IsBusiness == 2)
                {
                    if (Enrollment == null)
                        switch (Degree)
                        {
                            case 1:
                                return "本科生";
                            case 2:
                                return "硕士生";
                            case 3:
                                return "博士生";
                            case 4:
                                return "专科生";
                            default:
                                return "";
                        }
                    else
                    {
                        int years = (DateTime.Now - new DateTime((short)Enrollment, 7, 1)).Days/365;
                        switch (Degree)
                        {
                            case 1:
                                switch (years)
                                {
                                    case 0:
                                        return "大一";
                                    case 1:
                                        return "大二";
                                    case 2:
                                        return "大三";
                                    case 3:
                                        return "大四";
                                    default:
                                        return "已毕业";
                                }
                            case 2:
                                switch (years)
                                {
                                    case 0:
                                        return "研一";
                                    case 1:
                                        return "研二";
                                    case 2:
                                        return "研三";
                                    default:
                                        return "已毕业";
                                }
                            case 3:
                                switch (years)
                                {
                                    case 0:
                                        return "博一";
                                    case 1:
                                        return "博二";
                                    case 2:
                                        return "博三";
                                    case 4:
                                        return "博四";
                                    case 5:
                                        return "博五";
                                    default:
                                        return "已毕业";
                                }
                            case 4:
                                switch (years)
                                {
                                    case 0:
                                        return "专一";
                                    case 1:
                                        return "专二";
                                    case 2:
                                        return "专三";
                                    default:
                                        return "已毕业";
                                }
                            default:
                                return "";
                        }

                    }
                }
                else
                {
                    switch (Degree)
                    {
                        case 1:
                            return "学士";
                        case 2:
                            return "硕士";
                        case 3:
                            return "博士";
                        default:
                            return "";
                    }
                }
            }
        }

        [ForeignKey("ResearchFieldId")]
        public virtual ResearchField ResearchField { get; set; }

        [NotMapped]
        //是否联系人
        public virtual string IsContact{ get; set; }
        [NotMapped]
        //是否点过赞
        public virtual string IsFavorite { get; set; }
        [NotMapped]
        //好友数量
        public virtual int NumOfContacts { get; set; }
        [NotMapped]
        //看过别人的数量
        public virtual int NumOfBeenTo { get; set; }
        [NotMapped]
        //访客数量
        public virtual int NumOfVisitor { get; set; }
        [NotMapped]
        //被赞的数量
        public virtual int NumOfFavorite { get; set; }
        [NotMapped]
        //信息完成度
        public virtual int InformationCompletionPercent { get; set; }
        [NotMapped]
        //家乡的字符串
        public virtual string Hometown { get; set; }
        [NotMapped]
        //距离
        public virtual double Distance { get; set; }

        [NotMapped]
        public virtual int ArticleNum { get; set; }
        [NotMapped]
        public virtual int IsAcquaintance { get; set; }
        [NotMapped]
        public virtual string IsShow { get; set; }
        [NotMapped]
        public virtual string Is2ndDegree { get; set; }
        [NotMapped]
        public virtual int NumOfSignUp { get; set; }

        #endregion

        #region Rewrite Equals and HashCode
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if ((obj == null) || (obj.GetType() != GetType())) return false;
            UserInfo castObj = (UserInfo)obj;
            return (castObj != null) && (uuid == castObj.uuid);
        }
        public override int GetHashCode()
        {
            int hash = 57;
            hash = 27 * hash * uuid.GetHashCode();
            return hash;
        }
        #endregion
    }
}
