using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.CommonLib.DB.Repositorys;
using BK.CommonLib.Log;
using BK.CommonLib.MQ;
using BK.Model.DB;
using BK.Model.MQ;

namespace BK.NoticeService
{
    public static class NoticeHelper
    {
        public static async Task<long> SaveNoticeToSql(NoticeMQ notice)
        {
            Message msg = WeChatNoticeHelper.GetMessageType(notice);
            using(NoticeRepository noticeRepository = new NoticeRepository())
            {
                try
                {
                    if(await SaveOrNot(msg))
                        return await noticeRepository.SaveNotice(msg);
                    else
                        return 0;
                }
                catch(Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(NoticeHelper), ex);
                    return 0;
                }
            }
        }


        public static async Task<bool> SaveOrNot(Message msg)
        {
            using(NoticeRepository noticeRepository = new NoticeRepository())
            {
                bool result = true;
                try
                {
                    if(msg.ID == 0)
                    {
                        switch(msg.MsgType)
                        {
                            case (int)NoticeType.Visitor_Add:
                            case (int)NoticeType.Favorite_Add:
                                result = !await noticeRepository.CheckNoticeExist(msg);
                                break;
                            default:
                                break;
                        }
                    }

                }
                catch(Exception ex)
                {
                    LogHelper.LogErrorAsync(typeof(NoticeHelper), ex);
                }
                return result;
            }
        }





    }
}
