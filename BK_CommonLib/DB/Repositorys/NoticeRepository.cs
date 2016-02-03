using BK.CommonLib.DB.Context;
using BK.CommonLib.Util;
using BK.Model.DB;
using BK.Model.MQ;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Repositorys
{
    public class NoticeRepository: IDisposable
    {
        private bool disposed = false;
        private BKDBContext context;
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    context.Dispose();
                }
                disposed = true;
            }
        }
        #region common
        public NoticeRepository()
        {
            this.context = new BKDBContext();
        }
        public NoticeRepository(BKDBContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Notice

        public async Task<long> SaveNotice(Message input)
        {
            if(input.ID == 0)
            {
                context.Messages.Add(input);
            }
            else if(context.Entry(input).State == EntityState.Detached)
            {
                RepositoryHelper.UpdateContextItem(context, input);
            }
            if(await context.SaveChangesAsync() > 0)
                return input.ID;
            else
                return 0;
        }
        public async Task<bool> CheckNoticeExist(Message input)
        {
            var item = await (from uc in context.Messages
                              where uc.Receiver_uuid == input.Receiver_uuid
                              && uc.RelationID_uuid == input.RelationID_uuid
                              && uc.MsgType == input.MsgType
                              select uc).FirstOrDefaultAsync();
            if(item != null)
                return true;
            else
                return false;
        }

        public async Task<Message> GetNotice(long id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<List<Message>> GetNotice(int fromIndex, int pageSize, Guid uuid)
        {
            List<Message> list = null;
            int itemCount = 0;
            itemCount = await (from uc in context.Messages
                               where uc.Receiver_uuid == uuid
                               orderby uc.SendTime descending
                               select uc).CountAsync();
            if(itemCount > 0 && itemCount > fromIndex)
            {
                if(fromIndex + pageSize > itemCount)
                {
                    pageSize = itemCount - fromIndex;
                }

                list = await (from uc in context.Messages
                              where uc.Receiver_uuid == uuid
                              orderby uc.SendTime descending
                              select uc).Skip((fromIndex)).Take(pageSize).ToListAsync();
            }
            return list;
        }


        #endregion
    }
}
