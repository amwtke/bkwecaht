//------------------------------------------------------------------------------
//edmgen /mode:fullgeneration /c:"Data Source=rdsp5749258829142u3e.sqlserver.rds.aliyuncs.com,3433;Initial Catalog=EKSNS;Persist Security Info=True;User ID=qijing_db;Password=SQLSVR4294967296" /project:"C:\Users\sammy\Documents\Visual Studio 2015\Projects\EFGen_EKSNS" /entitycontainer:BKDBContext /namespace:BK.Model.DB /language:CSharp
//------------------------------------------------------------------------------

using BK.Model.DB;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BK.CommonLib.DB
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class BKDBContext : DbContext
    {
        #region Constructors
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
        }
        /// <summary>
        /// Initializes a new BKDBContext object using the connection string found in the 'BKDBContext' section of the application configuration file.
        /// </summary>
        public BKDBContext() : base("name=BKDBContext")
        {
            this.Configuration.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new BKDBContext object.
        /// </summary>
        public BKDBContext(string connectionString) : base(connectionString)
        {
            this.Configuration.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();

        #endregion

        #region ObjectSet Properties

        public virtual DbSet<EKToday> EKToday { get; set; }
        public virtual DbSet<GroupNews> GroupNews { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<qa_question> qa_question { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<AcademicCircleTopic> AcademicCircleTopic { get; set; }
        public virtual DbSet<AcademicCircleTopicFavorite> AcademicCircleTopicFavorite { get; set; }
        public virtual DbSet<AcademicCircleTopicNews> AcademicCircleTopicNews { get; set; }
        public virtual DbSet<AcademicCircleTopicPraise> AcademicCircleTopicPraise { get; set; }
        public virtual DbSet<Administrator> Administrator { get; set; }
        public virtual DbSet<article_download> article_download { get; set; }
        public virtual DbSet<article_praise> article_praise { get; set; }
        public virtual DbSet<article_reader> article_reader { get; set; }
        public virtual DbSet<article_request> article_request { get; set; }
        public virtual DbSet<ArticleNews> ArticleNews { get; set; }
        public virtual DbSet<Attentions> Attentions { get; set; }
        public virtual DbSet<BaseInfo> BaseInfo { get; set; }
        public virtual DbSet<businessrecruit_info> businessrecruit_info { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Conference> Conference { get; set; }
        public virtual DbSet<ConferenceParticipant> ConferenceParticipant { get; set; }
        public virtual DbSet<ConferenceRegister> ConferenceRegister { get; set; }
        public virtual DbSet<ConferenceResearchField> ConferenceResearchField { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<CooperativeProject> CooperativeProject { get; set; }
        public virtual DbSet<CooperativeProjectNews> CooperativeProjectNews { get; set; }
        public virtual DbSet<CooperativeProjectParticipant> CooperativeProjectParticipant { get; set; }
        public virtual DbSet<CooperativeProjectPraise> CooperativeProjectPraise { get; set; }
        public virtual DbSet<CooperativeProjectResearchField> CooperativeProjectResearchField { get; set; }
        public virtual DbSet<CopUserEquipment> CopUserEquipment { get; set; }
        public virtual DbSet<CopUserExperiment> CopUserExperiment { get; set; }
        public virtual DbSet<Country> Countrie { get; set; }
        public virtual DbSet<ekevent> ekevent { get; set; }
        public virtual DbSet<ekevent_register> ekevent_register { get; set; }
        public virtual DbSet<ektoday_type> ektoday_type { get; set; }
        public virtual DbSet<EKTodayResearchField> EKTodayResearchField { get; set; }
        public virtual DbSet<EquipmentName> EquipmentName { get; set; }
        public virtual DbSet<ExperimentName> ExperimentName { get; set; }
        public virtual DbSet<ExperimentProject> ExperimentProject { get; set; }
        public virtual DbSet<ExperimentSolution> ExperimentSolution { get; set; }
        public virtual DbSet<FriendLink> FriendLink { get; set; }
        public virtual DbSet<group_article> group_article { get; set; }
        public virtual DbSet<group_article_favorite> group_article_favorite { get; set; }
        public virtual DbSet<group_article_praise> group_article_praise { get; set; }
        public virtual DbSet<GroupBase> GroupBase { get; set; }
        public virtual DbSet<LeaveAMessage> LeaveAMessage { get; set; }
        public virtual DbSet<login_recommend> login_recommend { get; set; }
        public virtual DbSet<LogModifyEmail> LogModifyEmail { get; set; }
        public virtual DbSet<message_board> message_board { get; set; }
        public virtual DbSet<MessageSetting> MessageSetting { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<OtherExperiments> OtherExperiments { get; set; }
        public virtual DbSet<postdoctor_info> postdoctor_info { get; set; }
        public virtual DbSet<pre_register> pre_register { get; set; }
        public virtual DbSet<ProjectComment> ProjectComment { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<qa_answer> qa_answer { get; set; }
        public virtual DbSet<qa_favorite_question> qa_favorite_question { get; set; }
        public virtual DbSet<Recruit> Recruit { get; set; }
        public virtual DbSet<recruit_student> recruit_student { get; set; }
        public virtual DbSet<RecruitAdvantage> RecruitAdvantages { get; set; }
        public virtual DbSet<RecruitAdvantageInfo> RecruitAdvantageInfo { get; set; }
        public virtual DbSet<RecruitResearchField> RecruitResearchField { get; set; }
        public virtual DbSet<RecruitSignUp> RecruitSignUp { get; set; }
        public virtual DbSet<ResearchField> ResearchField { get; set; }
        public virtual DbSet<ShieldingText> ShieldingText { get; set; }
        public virtual DbSet<suggestion_register> suggestion_register { get; set; }
        
        public virtual DbSet<T_UnivsDep> T_UnivsDep { get; set; }
        public virtual DbSet<Univs> Univs { get; set; }
        public virtual DbSet<user_favorite> user_favorite { get; set; }
        public virtual DbSet<UserAcademic> UserAcademic { get; set; }
        public virtual DbSet<UserAcquaintance> UserAcquaintance { get; set; }
        public virtual DbSet<UserArticle> UserArticle { get; set; }
        public virtual DbSet<UserAwards> UserAwards { get; set; }
        public virtual DbSet<UserContacts> UserContacts { get; set; }
        public virtual DbSet<UserEducation> UserEducation { get; set; }
        public virtual DbSet<UserExperience> UserExperience { get; set; }
        public virtual DbSet<UserGroup> UserGroup { get; set; }
        public virtual DbSet<UserInvitation> UserInvitation { get; set; }
        public virtual DbSet<UserNews> UserNews { get; set; }
        public virtual DbSet<UserPatent> UserPatent { get; set; }
        public virtual DbSet<VisitBetweenUser> VisitBetweenUser { get; set; }
        public virtual DbSet<wechat_oa> wechat_oa { get; set; }
        public virtual DbSet<UserCourse> UserCourse { get; set; }
        public virtual DbSet<UserSkill> UserSkill { get; set; }
        #endregion
        

    }

    #endregion

    #region Entities

    #endregion


}
