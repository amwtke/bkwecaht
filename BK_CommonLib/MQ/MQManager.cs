﻿using BK.CommonLib.Log;
using BK.CommonLib.Util;
using BK.Model.Configuration.MQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.MQ
{
    public static class MQManager
    {
        static List<Type> _innerConifgTypes = new List<Type>();

        static Dictionary<Type, IConnection> _P_Connection_Dic = new Dictionary<Type, IConnection>();
        static Dictionary<Type, IModel> _P_Channel_Dic = new Dictionary<Type, IModel>();


        static Dictionary<Type, List<IConnection>> _C_Connection_Dic = new Dictionary<Type, List<IConnection>>();
        static Dictionary<Type, List<IModel>> _C_Channel_Dic = new Dictionary<Type, List<IModel>>();
        static Dictionary<Type, Action<BasicDeliverEventArgs,IModel>> _C_Process_Dic = new Dictionary<Type, Action<BasicDeliverEventArgs, IModel>>();

        public static void CloseAll()
        {
            foreach (var v in _P_Channel_Dic.Values)
            {
                if(v.IsOpen)
                {
                    v.Close();
                }
            }

            foreach (var v in _P_Connection_Dic.Values)
            {
                if(v.IsOpen)
                {
                    v.Close(100);
                }
            }

            foreach (var v in _C_Channel_Dic.Values)
            {
                foreach (var vv in v)
                {
                    if (vv.IsOpen)
                        vv.Close();
                }
            }

            foreach (var v in _C_Connection_Dic.Values)
            {
                foreach(var vv in v)
                {
                    if (vv.IsOpen)
                    {
                        vv.Close(100);
                    }
                }
                
            }
        }

        public static void RegisterConsumerProcessor<T>(Action<BasicDeliverEventArgs, IModel> process) where T : class, IMQConfig
        {
            _C_Process_Dic[typeof(T)] = process;
        }

        static MQManager()
        {
            init();
        }

        private static void init()
        {
            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(delegate (Assembly ass)
            {
                if (ass.ToString().ToLower().Contains("bk.model"))
                {
                    ass.GetTypes().ToList().ForEach(delegate (Type at)
                    {
                        //Console.WriteLine(at.ToString());
                        at.GetInterfaces().ToList().ForEach(delegate (Type intface)
                        {
                            if (intface.ToString().Contains("IMQConfig"))
                                _innerConifgTypes.Add(at);
                        });
                    });
                }
            });
        }

        private static bool Prepare_P_MQ(Type t)
        {
            if (_P_Connection_Dic.ContainsKey(t) && _P_Channel_Dic.ContainsKey(t))
                return true;

            //反射创建配置对象
            if (!_innerConifgTypes.Contains(t)) return false;
            IMQConfig target = Configuration.BK_ConfigurationManager.GetConfigByType(t) as IMQConfig;

            if (target == null) return false;
            //创建TCP连接
            IConnection connection = null; IModel channel = null;
            if (_P_Connection_Dic.ContainsKey(t))
            {
                channel = _P_Connection_Dic[t].CreateModel();
                _P_Channel_Dic[t] = channel;
            }
            else
            {
                var factory = new ConnectionFactory();
                factory.HostName = target.HostName;
                factory.UserName = target.UserName;
                factory.Password = target.Password;
                factory.Port = Convert.ToInt32(target.Port);
                connection = factory.CreateConnection();
                channel = connection.CreateModel();

                _P_Channel_Dic[t] = channel;
                _P_Connection_Dic[t] = connection;
            }
            //配置连接属性
            bool durable = true;
            _P_Channel_Dic[t].ExchangeDeclare(target.ExchangeName, "direct", durable);
            return true;
        }
        public static bool Prepare_P_MQ<T>() where T :class,IMQConfig
        {
            return Prepare_P_MQ(typeof(T));
        }
        public static void Prepare_All_P_MQ()
        {
            _innerConifgTypes.ForEach(delegate (Type t)
            {
                if (!Prepare_P_MQ(t))
                    throw new Exception("类型t=" + t.ToString() + "的P初始化失败！");
            });
        }


        /// <summary>
        /// 如果有多个C，RouteKey是随机数%numberofC的。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<bool> SendMQ<T>(object o) where T : class, IMQConfig
        {
            Type t = typeof(T);
            if (!_P_Channel_Dic.ContainsKey(t))
                if (!Prepare_P_MQ(t))
                    return false;
            var channel = _P_Channel_Dic[t];
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            Byte[] payLoad = BinarySerializationHelper.SerializeObject(o);
            IMQConfig target = Configuration.BK_ConfigurationManager.GetConfigByType(t) as IMQConfig;
            int numberOfC = Convert.ToInt32(target.NumberOfC);
            try
            {
                await AsyncHelper.RunAsync<IModel>(delegate (IModel model)
                {
                    //0,1,2,3,...Number of c
                    int rand = new Random().Next(numberOfC) % numberOfC;
                    model.BasicPublish(target.ExchangeName, rand.ToString(), properties, payLoad);

                }, channel);
                return true;
            }
            catch(Exception ex)
            {
                BKLogger.LogError(typeof(MQManager), ex);
                return false;
            }

        }

        /// <summary>
        /// 如果有多个C，RouteKey是随机数%numberofC的。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool SendMQ_TB<T>(object o) where T : class, IMQConfig
        {
            Type t = typeof(T);
            if (!_P_Channel_Dic.ContainsKey(t))
                if (!Prepare_P_MQ(t))
                    return false;
            var channel = _P_Channel_Dic[t];
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            Byte[] payLoad = BinarySerializationHelper.SerializeObject(o);
            IMQConfig target = Configuration.BK_ConfigurationManager.GetConfigByType(t) as IMQConfig;
            int numberOfC = Convert.ToInt32(target.NumberOfC);

            //AsyncHelper.RunAsync<IModel>(delegate (IModel model) {
            //0,1,2,3,...Number of c
            int rand = new Random().Next(numberOfC) % numberOfC;
            channel.BasicPublish(target.ExchangeName, rand.ToString(), properties, payLoad);
            //Console.WriteLine("Thead Id:" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            //}, channel, null);
            return true;
        }

        private static void Prepare_C_MQ(Type t,bool isTempQueue=false)
        {
            if (!_C_Process_Dic.ContainsKey(t))
                throw new Exception("类型:"+t.ToString()+" 没有注册process，请先注册！");
            //保证一个channel+Exchange+Queue只被初始化一次。
            if (_C_Connection_Dic.ContainsKey(t) && _C_Channel_Dic.ContainsKey(t))
            {
                return;
            }

            //反射创建配置对象
            if (!_innerConifgTypes.Contains(t))
                throw new Exception("_innerConifgTypes中"+"没有类型："+t.ToString()+" 可能是初始化错误！");
            IMQConfig target = Configuration.BK_ConfigurationManager.GetConfigByType(t) as IMQConfig;

            if (target == null)
                throw new Exception("配置信息读取错误，找不到配置对象！t="+t.ToString());

            //创建TCP连接
            

            InvokeConsumer(t,target,_C_Process_Dic[t], isTempQueue);
        }

        private static void InvokeConsumer(Type t,IMQConfig target, Action<BasicDeliverEventArgs, IModel> process,bool isTempQueue)
        {
            try
            {
                if (!_C_Connection_Dic.ContainsKey(t))
                    _C_Connection_Dic[t] = new List<IConnection>();
                if (!_C_Channel_Dic.ContainsKey(t))
                    _C_Channel_Dic[t] = new List<IModel>();

                int numberofC = Convert.ToInt32(target.NumberOfC);
                if (numberofC > 0)
                {
                    for (int i = 0; i < numberofC; i++)
                    {
                        IConnection connection = null; IModel channel = null;

                        var factory = new ConnectionFactory();
                        factory.HostName = target.HostName;
                        factory.UserName = target.UserName;
                        factory.Password = target.Password;
                        factory.Port = Convert.ToInt32(target.Port);
                        connection = factory.CreateConnection();
                        channel = connection.CreateModel();

                        _C_Channel_Dic[t].Add(channel);
                        _C_Connection_Dic[t].Add(connection);

                        //配置连接属性
                        bool durable = true;
                        channel.ExchangeDeclare(target.ExchangeName, "direct", durable);

                        string q_name = "";
                        if (!isTempQueue)
                        {
                            q_name = target.QueueName + "_" + i.ToString();
                            channel.QueueDeclare(q_name, durable, false, false, null);
                            channel.QueueBind(q_name, target.ExchangeName,i.ToString());
                        }
                        else
                        {
                            q_name = channel.QueueDeclare().QueueName;//temp queue
                            channel.QueueBind(q_name, target.ExchangeName, i.ToString());
                        }

                        channel.BasicQos(0, 1, false);
                        //begin consume
                        var consumer = new QueueingBasicConsumer(channel);
                        channel.BasicConsume(q_name, false, consumer);
                        AsyncHelper.RunAsync(delegate ()
                        {
                            while (true)
                            {
                                BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                                AsyncHelper.RunAsync<BasicDeliverEventArgs, IModel>(process, ea, channel, null);
                            }
                        }, null);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.LogErrorAsync(typeof(MQManager), ex);
                //TODO LOGerror
            }
            
        }
        private static void Prepare_C_MQ<T>(Action<BasicDeliverEventArgs,IModel> process=null)
        {
            try
            {
                Prepare_C_MQ(typeof(T));
            }
            catch
            {
                throw;
            }
        }

        public static bool Prepare_All_C_MQ()
        {
            _innerConifgTypes.ForEach(delegate (Type t)
            {
                try
                {
                    Prepare_C_MQ(t);
                }
                catch (Exception ex)
                {
                    //LogHelper.LogErrorAsync(typeof(MQManager), ex);
                    //TODO : log
                }
                    
            });
            return true;
        }


    }
}
