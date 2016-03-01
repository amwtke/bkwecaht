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

        [BKKey("EDM_API_User")]
        public string EDM_API_User { get; set; }

        [BKKey("SVR_API_User")]
        public string SVR_API_User { get; set; }

        [BKKey("API_Key")]
        public string API_Key { get; set; }



        [BKKey("ResetPasswordValidation_EmailTempleteId")]
        public string ResetPasswordValidation_EmailTempleteId { get; set; }


        [BKKey("RegisterValidation_TempleteId")]
        public string RegisterValidation_TempleteId { get; set; }

        public void init()
        {
        }
    }
}
