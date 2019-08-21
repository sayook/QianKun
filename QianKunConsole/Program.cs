﻿using QianKunHelper.LogHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CheckManagerBLL;
using CheckManagerModel;
using QianKunHelper;
using QianKunHelper.CacheHelper;
using QianKunHelper.DBHelper;
using QianKunHelper.WebApiHelper;
using Autofac;

namespace QianKunConsole
{
    class Program
    {
        private static QianKun QianKun = new QianKun();
        private static IContainer container { get; set; }
        static void Main(string[] args)
        {
            Console.WriteLine($"action[{Thread.CurrentThread.ManagedThreadId}]");
            Stopwatch st = new Stopwatch();//实例化类
            st.Start();//开始计时
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleOutput>().As<IOutput>();
            builder.RegisterType<TodayWriter>().As<IDateWriter>();
            container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IDateWriter>();
                writer.WriteDate();
            }

            Console.WriteLine("ok");

            st.Stop();//终止计时
            Console.WriteLine($"action[{Thread.CurrentThread.ManagedThreadId}]【耗时：{st.ElapsedMilliseconds}】");
            Console.ReadKey();


        }
    }
    public class Parent
    {
        public Parent()
        {
            Console.WriteLine("parent");
        }
    }

    public class QianKun : Parent
    {
        public string x;
        ILog _log;
        public QianKun()
        {
            Console.WriteLine("qiankeun");
            _log = LogManager.GetLog<QianKun>();
        }

        #region async&await
        public async Task<int> Action()
        {
            Console.WriteLine("喝茶1...");
            var title1 = await LookNewsTitleAsync(2000);
            var title2 = await LookNewsTitleAsync(1000);
            Console.WriteLine("喝茶2...");
            Console.WriteLine(title1);
            Console.WriteLine(title2);
            return (title1 + title2).Length;
        }
        public Task<string> LookNewsTitleAsync(int sleep)
        {
            Console.WriteLine("LookNewsTitleAsync:" + Thread.CurrentThread.ManagedThreadId);
            return Task.Run(() => LookNewsTitle(sleep));
        }
        public string LookNewsTitle(int sleep)
        {
            Console.WriteLine("LookNewsTitle:" + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("查看新闻标题...");
            Thread.Sleep(sleep);
            var content = "中国海军成立70周年:" + sleep;
            Console.WriteLine(content);
            Console.WriteLine("查看完成...");
            return content;
        }

        public int A300()
        {
            Thread.Sleep(3000);
            Console.WriteLine($"A[{Thread.CurrentThread.ManagedThreadId}]");
            return 300;
        }
        public Task<int> B500Asycn()
        {
            return Task.Run(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine($"B[{Thread.CurrentThread.ManagedThreadId}]");
                return 500;
            });
        }
        public Task<int> B400Asycn()
        {
            return Task.Run(() =>
            {
                Thread.Sleep(4000);
                Console.WriteLine($"B[{Thread.CurrentThread.ManagedThreadId}]");
                return 400;
            });

        }
        #endregion

        #region Parallel
        public void ParallelTesting()
        {
            Parallel.For(0, 1001, new ParallelOptions { MaxDegreeOfParallelism = 1000 }, x =>
                {
                    var rep = WebapiHelper.Get<string>("https://localhost:5001/api/values/" + x);
                    _log.Info($"{x}:{rep.Data}:{Thread.CurrentThread.ManagedThreadId}");
                });
        }
        public void NotParallelTesting()
        {
            for (int i = 0; i < 220; i++)
            {
                try
                {
                    string key = $"key_{i}";
                    try
                    {
                        CacheManager.Cache.Set(key, Thread.CurrentThread.ManagedThreadId);
                        var value = CacheManager.Cache.Get(key);
                        _log.Debug($"{key}:{value}");
                    }
                    catch (Exception e)
                    {
                        _log.Debug($"{key}:【异常】{e.Message}");
                    }
                }
                catch (Exception e)
                {
                    var factroy = new Log4netLoggerFactroy();
                    factroy.GetLogger(typeof(Program)).Debug(e.Message);
                }
            }
        }
        #endregion

        #region HttpClient

        public void Post()
        {
            var reslut = WebapiHelper.Post<int>("https://localhost:44335/api/values", new Random().Next(100).ToString());
            _log.Debug(reslut.Data);
            Console.WriteLine(reslut.Data);
        }

        public void Get()
        {
            _log.Debug("");

        }

        #endregion

        #region 反射



        #endregion
    }
    public interface IOutput
    {
        void Write(string content);
    }
    public class ConsoleOutput : IOutput
    {
        public void Write(string content)
        {
            Console.WriteLine(content);
        }
    }
    public interface IDateWriter
    {
        void WriteDate();
    }
    public class TodayWriter : IDateWriter
    {
        private IOutput _output;
        public TodayWriter(IOutput output)
        {
            _output = output;
        }

        public void WriteDate()
        {
            _output.Write(DateTime.Today.ToShortDateString());
        }
    }
}
