using BK.CommonLib.Log;
using BK.DataSyncFromEK.EKModel;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Reflection;

namespace BK.DataSyncFromEK
{
    public partial class DataSyncService: ServiceBase
    {
        private List<Guid> addstu = new List<Guid>();
        private List<Guid> addpro = new List<Guid>();
        private System.Timers.Timer t = new System.Timers.Timer(); //设置时间间隔为5秒
        public DataSyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.LogInfoAsync(typeof(DataSyncService), "BK.DataSyncFromEK服务启动!");
            t.Interval = new TimeSpan(24, 0, 0).TotalMilliseconds;
            t.AutoReset = true;
            t.Elapsed += async delegate (object sender, System.Timers.ElapsedEventArgs e) {
                await init();
            };
            t.Enabled = true;

            TimeSpan setTime = TimeSpan.FromHours(2);
            TimeSpan remain = setTime > DateTime.Now.TimeOfDay ? setTime.Subtract(DateTime.Now.TimeOfDay) : setTime.Add(TimeSpan.FromDays(1)).Subtract(DateTime.Now.TimeOfDay);
            System.Timers.Timer ft = new System.Timers.Timer(remain.TotalMilliseconds);
            ft.AutoReset = false;
            ft.Elapsed += async delegate (object sender, System.Timers.ElapsedEventArgs e) {
                t.Start();
                await init();
            };
            ft.Enabled = true;
            ft.Start();
        }

        protected override void OnStop()
        {
            t.Stop();
            LogHelper.LogInfoAsync(typeof(DataSyncService), "BK.DataSyncFromEK服务结束！");
        }
        public async Task init()
        {
            try
            {
                LogHelper.LogInfoAsync(typeof(DataSyncService), "BK.DataSyncFromEK开始扫描Userinfo!");
                addstu.Clear();
                addpro.Clear();
                ProcessUserinfo();
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK扫描到教授{0}学生{1}!", addpro.Count, addstu.Count));
                Task<int> proUserAcademic = AddRecords<UserAcademic>(addpro);
                Task<int> proUserArticle = AddRecords<UserArticle>(addpro);
                Task<int> proUserAwards = AddRecords<UserAwards>(addpro);
                Task<int> proUserEducation = AddRecords<UserEducation>(addpro);
                Task<int> proUserExperience = AddRecords<UserExperience>(addpro);
                Task<int> proUserPatent = AddRecords<UserPatent>(addpro);

                Task<int> stuUserEducation = AddRecords<UserEducation>(addstu);
                Task<int> stuUserExperience = AddRecords<UserExperience>(addstu);
                Task<int> stuUserArticle = AddRecords<UserArticle>(addstu);
                //Task<int> stuUserCourse = AddRecords<UserCourse>(addstu);
                //Task<int> stuUserSkill = AddRecords<UserSkill>(addstu);


                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserAcademic", await proUserAcademic));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserArticle", await proUserArticle));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserAwards", await proUserAwards));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserEducation", await proUserEducation));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserExperience", await proUserExperience));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "proUserPatent", await proUserPatent));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "stuUserEducation", await stuUserEducation));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "stuUserExperience", await stuUserExperience));
                LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "stuUserArticle", await stuUserArticle));
                //LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "stuUserCourse", await stuUserCourse));
                //LogHelper.LogInfoAsync(typeof(DataSyncService), string.Format("BK.DataSyncFromEK {0} {1}!", "stuUserSkill", await stuUserSkill));

                addpro.Clear();
                addstu.Clear();
                LogHelper.LogInfoAsync(typeof(DataSyncService), "BK.DataSyncFromEK处理结束!");
            }
            catch(Exception ex)
            {
                LogHelper.LogErrorAsync(typeof(DataSyncService), ex);
            }
        }

        private void ProcessUserinfo()
        {
            using(UserRepository EKrepo = new UserRepository())
            {
                using(BKRepo.UserRepository BKrepo = new BKRepo.UserRepository())
                {
                    try
                    {
                        foreach(var ekui in EKrepo.GetAllUserInfo())
                        {
                            if(ekui.IsBusiness == 2 ||
                               ekui.IsBusiness == 0 && (ekui.IsLogin == 0 || ekui.IsLogin == 1))
                            {
                                BKModel.UserInfo bkui = BKrepo.GetUserInfoByUuid_TB(ekui.uuid);
                                if(bkui == null)
                                {
                                    BKrepo.AddUserInfoTemp(GetCopy(ekui));
                                    if(ekui.IsBusiness == 0)
                                        addpro.Add(ekui.uuid);
                                    else
                                        addstu.Add(ekui.uuid);
                                }
                                else
                                {
                                    if(ekui.LastLogin > bkui.LastLogin)
                                        Compare(ekui, bkui);
                                }
                            }
                        }
                        BKrepo.SaveContext();

                    }
                    catch(Exception ex)
                    {
                        LogHelper.LogErrorAsync(typeof(DataSyncService), ex);
                    }
                }
            }
        }

        private async Task<int> AddRecords<T>(List<Guid> ul) where T : class, IDBModelWithID
        {
            if(ul.Count > 0)
                using(UserRepository EKrepo = new UserRepository())
                {
                    using(BKRepo.UserRepository BKrepo = new BKRepo.UserRepository())
                    {
                        try
                        {
                            Type BKClass = Assembly.GetExecutingAssembly().GetType("BK.DataSyncFromEK.BKModel." + typeof(T).Name);
                            Type BKClassList = typeof(List<>).MakeGenericType(BKClass);
                            dynamic BKUserRecords = Activator.CreateInstance(BKClassList);
                            MethodInfo addMethod = BKClassList.GetMethod("Add");
                            foreach(var ur in ul)
                            {
                                List<T> EKUserRecords = await EKrepo.GetUserRecords<T>(ur);
                                foreach(var i in EKUserRecords)
                                {
                                    dynamic dynamici = Convert.ChangeType(i, BKClass);
                                    addMethod.Invoke(BKUserRecords, new object[] { dynamici });
                                }
                            }
                            return await BKrepo.AddUserRecordsTemp(BKUserRecords);
                        }
                        catch(Exception ex)
                        {
                            LogHelper.LogErrorAsync(typeof(DataSyncService), ex);
                            return 0;
                        }
                    }
                }
            else
                return 0;
        }

        private BKModel.UserInfo GetCopy(UserInfo obj)
        {
            BKModel.UserInfo ui = new BKModel.UserInfo();

            if(obj != null && obj.uuid != Guid.Empty)
            {
                foreach(PropertyInfo pi in obj.GetType().GetProperties())
                {
                    ui.GetType().GetProperty(pi.Name).SetValue(ui, pi.GetValue(obj));
                }
            }
            return ui;
        }

        private bool Compare(UserInfo obj, BKModel.UserInfo obj2)
        {
            bool result = false;

            if(obj != null && obj.uuid != Guid.Empty)
            {
                foreach(PropertyInfo pi in obj.GetType().GetProperties())
                {
                    if(pi.Name != "ResearchFieldId")
                    {
                        if(pi.GetValue(obj) != null)
                            if(obj2.GetType().GetProperty(pi.Name).GetValue(obj2) != null)
                            {
                                if(!obj2.GetType().GetProperty(pi.Name).GetValue(obj2).Equals(pi.GetValue(obj)))
                                {
                                    obj2.GetType().GetProperty(pi.Name).SetValue(obj2, pi.GetValue(obj));
                                    result = true;
                                }
                            }
                            else
                            {
                                obj2.GetType().GetProperty(pi.Name).SetValue(obj2, pi.GetValue(obj));
                                result = true;
                            }
                    }
                }
                if(obj2.ResearchFieldId != (obj.ResearchFieldId.ToString().Length > obj.SubResearchFieldId.ToString().Length ? obj.ResearchFieldId : obj.SubResearchFieldId))
                {
                    obj2.ResearchFieldId = (obj.ResearchFieldId.ToString().Length > obj.SubResearchFieldId.ToString().Length ? obj.ResearchFieldId : obj.SubResearchFieldId);
                }
            }

            return result;
        }
    }
}
