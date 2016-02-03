using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.CommonLib.Util
{
    public static class AsyncHelper
    {
        /// <summary>
        /// 将一个方法function异步运行，在执行完毕时执行回调callback.
        /// </summary>
        /// <param name="funcToRun——不带参数的Action"></param>
        /// <param name="callBack——不带参数的Action"></param>
        public static async void RunAsync(Action funcToRun, Action callBack)
        {
            Func<Task> taskFunc = () =>
            {
                return Task.Run(() =>
                {
                    funcToRun();
                });
            };
            await taskFunc();
            if (callBack != null)
                callBack();
        }

        public static async void RunAsync<TIn>(Action<TIn> funcToRun, TIn arg, Action callBack)
        {
            Func<Task> taskFunc = () =>
            {
                return Task.Run(() =>
                {
                    funcToRun(arg);
                });
            };
            await taskFunc();
            if (callBack != null)
                callBack();
        }
        /// <summary>
        /// webapi调用
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="funcToRun"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task RunAsync<TIn>(Action<TIn> funcToRun, TIn arg)
        {
            Func<Task> taskFunc = () =>
            {
                return Task.Run(() =>
                {
                    funcToRun(arg);
                });
            };
            await taskFunc();
        }

        public static async void RunAsync<TIn,TIn2>(Action<TIn,TIn2> funcToRun, TIn arg,TIn2 arg2 ,Action callBack)
        {
            Func<Task> taskFunc = () =>
            {
                return Task.Run(() =>
                {
                    funcToRun(arg,arg2);
                });
            };
            await taskFunc();
            if (callBack != null)
                callBack();
        }

        public static async void RunAsync<TIn, TResult>(Func<TIn, TResult> funcToRun, TIn arg, Action<TResult> callBack)
        {
            Func<Task<TResult>> taskFunc = () =>
            {
                return Task<TResult>.Run<TResult>(() =>
                {
                    return funcToRun(arg);
                });
            };
            TResult t = await taskFunc();
            if (callBack != null)
                callBack(t);
        }


        public static async void RunAsync<TResult>(Func<TResult> funcToRun, Action<TResult> callBack)
        {
            Func<Task<TResult>> taskToRun = () =>
            {
                return Task<TResult>.Run<TResult>(
                    () =>
                    {
                        return funcToRun();
                    }
                );
            };
            TResult r = await taskToRun();
            if (callBack != null)
                callBack(r);
        }
    }
}
