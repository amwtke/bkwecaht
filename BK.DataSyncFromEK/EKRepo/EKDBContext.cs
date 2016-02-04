
using BK.DataSyncFromEK.EKModel;
using System.Data.Entity;

namespace BK.DataSyncFromEK
{
    #region Contexts

    public partial class EKDBContext : DbContext
    {
        #region Constructors
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
        }

        public EKDBContext() : base("name=EKSNS")
        {
            this.Configuration.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        public EKDBContext(string connectionString) : base(connectionString)
        {
            this.Configuration.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();

        #endregion

        #region ObjectSet Properties

        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<UserAcademic> UserAcademic { get; set; }
        public virtual DbSet<UserArticle> UserArticle { get; set; }
        public virtual DbSet<UserAwards> UserAwards { get; set; }
        public virtual DbSet<UserContacts> UserContacts { get; set; }
        public virtual DbSet<UserEducation> UserEducation { get; set; }
        public virtual DbSet<UserExperience> UserExperience { get; set; }
        public virtual DbSet<UserPatent> UserPatent { get; set; }
        //public virtual DbSet<UserCourse> UserCourse { get; set; }
        //public virtual DbSet<UserSkill> UserSkill { get; set; }
        #endregion
        

    }

    #endregion
    


}
