using BK.Model.DB.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.DB.Context
{
    public class ChatMessageLogContext: DbContext
    {
        public ChatMessageLogContext() : base("name=BKDBContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<WeChatMessageMSSQL> WeChatMessage { get; set; }
        public DbSet<EKCommentLog> EKComment { get; set; }
        public DbSet<PaperCommentLog> PaperComment { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
