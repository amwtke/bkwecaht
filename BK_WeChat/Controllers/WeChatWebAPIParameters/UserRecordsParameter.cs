using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BK.Model.DB;
using System.Collections.Concurrent;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    //8大项
    public class UserRecordsParameter : BaseParameter
    {
        public Guid uuid
        { get; set; }
        public long id
        { get; set; }

        //对象的id号可以用factory来获取。
        public string typeid { get; set; }
        //学术地位
        public UserAcademic userAcademic
        { get; set; }
        //论文
        public UserArticle userArticle
        { get; set; }
        //自助奖励
        public UserAwards userAwards
        { get; set; }
        //教育经历
        public UserEducation userEducation
        { get; set; }
        //工作经历
        public UserExperience userExperience
        { get; set; }
        //专利
        public UserPatent userPatent
        { get; set; }
        //专长技能
        public UserSkill userSkill
        { get; set; }
        //课程
        public UserCourse userCourse
        { get; set; }
    }

    public static class RecordFactory
    {
        static ConcurrentDictionary<string, Type> _dic = new ConcurrentDictionary<string, Type>();

        public static Type GetTypeById(string id)
        {
            Type ret = null;
            _dic.TryGetValue(id, out ret);
            return ret;
        }

        public static void AddOrUpdate(string id, Type t)
        {
            _dic[id] = t;
        }

        static RecordFactory()
        {
            _dic.TryAdd("UserAcademic", typeof(UserAcademic));
            _dic.TryAdd("UserArticle", typeof(UserArticle));
            _dic.TryAdd("UserAwards", typeof(UserAwards));
            _dic.TryAdd("UserEducation", typeof(UserEducation));
            _dic.TryAdd("UserExperience", typeof(UserExperience));
            _dic.TryAdd("UserPatent", typeof(UserPatent));
            _dic.TryAdd("UserSkill", typeof(UserSkill));
            _dic.TryAdd("UserCourse", typeof(UserCourse));
        }
    }
}