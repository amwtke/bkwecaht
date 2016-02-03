using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Index
{
    [ElasticType(Name = "biz")]
    public class BizIndex
    {
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "TimeStamp", Type = FieldType.Date)]
        public string TimeStamp { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UserName")]
        public string UserName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "LoggerName")]
        public string LoggerName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UserEmail")]
        public String UserEmail { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "FromUrl")]
        public string FromUrl { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "NowUrl")]
        public string NowUrl { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UserIP")]
        public string UserIP { get; set; }

        /// <summary>
        /// 功能模块的名称
        /// </summary>
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "ModelName")]
        public string ModelName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "SessionId")]
        public string SessionId { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "OpenId")]
        public string OpenId { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Name = "Message", Analyzer = "ik")]
        public string Message { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "User_UUID")]
        public string User_UUID { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Id")]
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Platform")]
        public string Platform { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UnUsed1")]
        public string UnUsed1 { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UnUsed2")]
        public string UnUsed2 { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UnUsed3")]
        public string UnUsed3 { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "HostName")]
        public string HostName { get; set; }
    }
}
