using BK.CommonLib.DB.Context;
using BK.Model.DB.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Repositorys
{
    public class MessageRepository : IDisposable
    {
        private bool disposed = false;
        private ChatMessageLogContext context;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
                disposed = true;
            }
        }
        #region common
        public MessageRepository()
        {
            this.context = new ChatMessageLogContext();
        }
        public MessageRepository(ChatMessageLogContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region operation

        public async Task<bool> AddChatLogAsync(Guid uuid,string from, string to, string message,string SessionId,DateTime timeStamp)
        {
            WeChatMessageMSSQL o = new WeChatMessageMSSQL();
            o.From = from.ToUpper();
            o.To = to.ToUpper();
            o.Uuid = uuid;
            o.Message = message;
            o.TimeStamp = timeStamp;
            o.SessionId = SessionId;
            context.WeChatMessage.Add(o);
            if (await context.SaveChangesAsync() > 0)
                return true;
            return false;
        }

        public async Task<bool> AddEKCommentAsync(Guid uuid, Guid from, long to, string content, DateTime timeStamp)
        {
            EKCommentLog o = new EKCommentLog();
            o.content = content;
            o.timestamp = timeStamp;
            o.Uuid = uuid;
            o.to = to;
            o.from = from;
            context.EKComment.Add(o);
            if (await context.SaveChangesAsync() > 0)
                return true;
            return false;
        }

        public async Task<bool> AddPaperCommentAsync(Guid uuid, Guid from, long to, string content, DateTime timeStamp)
        {
            PaperCommentLog o = new PaperCommentLog();
            o.content = content;
            o.timestamp = timeStamp;
            o.Uuid = uuid;
            o.to = to;
            o.from = from;
            context.PaperComment.Add(o);
            if (await context.SaveChangesAsync() > 0)
                return true;
            return false;
        }

        public async Task<List<WeChatMessageMSSQL>> GetLogRecordsAsync(int fromIndex, int pageSize, string sessionId)
        {
            List<WeChatMessageMSSQL> list = null;
            int itemCount = 0;
            itemCount = await (from uc in context.WeChatMessage
                               where uc.SessionId == sessionId
                               select uc).CountAsync();
            if (itemCount > 0 && itemCount > fromIndex)
            {
                if (fromIndex + pageSize > itemCount)
                {
                    pageSize = itemCount - fromIndex;
                }

                list = await (from uc in context.WeChatMessage
                              where uc.SessionId == sessionId
                              orderby uc.TimeStamp descending
                              select uc).Skip((fromIndex)).Take(pageSize).ToListAsync();
            }
            return list;
        }

        public async Task<List<EKCommentLog>> GetEKCommentAsync(long ekId,int fromIndex, int pageSize)
        {
            List<EKCommentLog> list = null;
            int itemCount = 0;
            itemCount = await (from uc in context.EKComment
                               where uc.to == ekId
                               select uc).CountAsync();
            if (itemCount > 0 && itemCount > fromIndex)
            {
                if (fromIndex + pageSize > itemCount)
                {
                    pageSize = itemCount - fromIndex;
                }

                list = await (from uc in context.EKComment
                              where uc.to == ekId
                              orderby uc.timestamp descending
                              select uc).Skip((fromIndex)).Take(pageSize).ToListAsync();
            }
            return list;
        }

        public async Task<List<PaperCommentLog>> GetPaperCommentAsync(long id, int fromIndex, int pageSize)
        {
            List<PaperCommentLog> list = null;
            int itemCount = 0;
            itemCount = await (from uc in context.PaperComment
                               where uc.to == id
                               select uc).CountAsync();
            if (itemCount > 0 && itemCount > fromIndex)
            {
                if (fromIndex + pageSize > itemCount)
                {
                    pageSize = itemCount - fromIndex;
                }

                list = await (from uc in context.PaperComment
                              where uc.to == id
                              orderby uc.timestamp descending
                              select uc).Skip((fromIndex)).Take(pageSize).ToListAsync();
            }
            return list;
        }

        #endregion
    }
}
