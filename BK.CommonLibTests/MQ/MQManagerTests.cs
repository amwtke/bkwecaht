﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using BK.CommonLib.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace BK.CommonLib.MQ.Tests
{
    [TestClass()]
    public class MQManagerTests
    {
        [TestMethod()]
        public void Prepare_All_C_MQTest()
        {
            MQManager.RegisterConsumerProcessor<BK.Model.Configuration.MQ.TestMQ>(delegate (BasicDeliverEventArgs ar,IModel channel)
            {
                var body = ar.Body;
                //RabbitObject rabbitObject = SerializationUnit.DeserializeObject(body) as RabbitObject;
                string s = Encoding.UTF8.GetString(body);

                //int dots = message.Split('.').Length - 1;
                //Thread.Sleep(dots * 1000);

                //Console.WriteLine("DateTime {0}", rabbitObject.TestDT.ToString());
                //Console.WriteLine("TestFloat {0}", rabbitObject.TestFloat.ToString());
                //Console.WriteLine("TestInt {0}", rabbitObject.TestInt.ToString());
                //Console.WriteLine("TestString {0}", rabbitObject.TestString.ToString());
                Console.WriteLine(s);
                Console.WriteLine("Thread ID" + System.Threading.Thread.CurrentThread.ManagedThreadId);

                channel.BasicAck(ar.DeliveryTag, false);
            });
            MQ.MQManager.Prepare_All_C_MQ();
            //Console.ReadKey();
            Assert.IsFalse(false);
        }
    }
}