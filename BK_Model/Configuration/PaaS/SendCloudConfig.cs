using BK.Model.Configuration.Att;
namespace BK.Model.Configuration.PaaS
{
    [BKConfig("PaaS", "SendCloud")]
    public class SendCloudConfig : IConfigModel
    {
        [BKKey("SMS_API_User")]
        public string SMS_API_User { get; set; }

        [BKKey("SMS_API_Key")]
        public string SMS_API_Key { get; set; }

        [BKKey("RegisterValidation_TempleteId")]
        public string RegisterValidation_TempleteId { get; set; }

        public void init()
        {
        }
    }
}
