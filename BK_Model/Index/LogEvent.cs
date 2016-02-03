using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Model.Index
{
    public enum LogLevel
    {
        INFO,
        ERROR,
        WARNING,
        DEBUG
    }
    [ElasticType(Name = "event")]
    public class LogEvent
    {
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "TimeStamp", Type = FieldType.Date)]
        public string TimeStamp { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Name = "Message", Analyzer = "ik")]
        public string Message { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Name = "MessageObject")]
        public string MessageObject { get; set; }

        [ElasticProperty(Name = "Exception", Index = FieldIndexOption.Analyzed)]
        public String Exception { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "LoggerName")]
        public string LoggerName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Domain")]
        public string Domain { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Id")]
        public string Id { get; set; }
        
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Level")]
        public string Level { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "ClassName")]
        public string ClassName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "FileName")]
        public string FileName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Name")]
        public string Name { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "FullInfo")]
        public string FullInfo { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "MethodName")]
        public string MethodName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "OS")]
        public string OS { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "Properties")]
        public string Properties { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "UserName")]
        public string UserName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "ThreadName")]
        public string ThreadName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Name = "HostName")]
        public string HostName { get; set; }
    }
}
