using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BK.Model.DB;
namespace BK.CommonLib.DB.Context
{
    public class UserInfoContext : DbContext
    {
        public UserInfoContext() : base("name=BKDBContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<UserAcademic> Academics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
