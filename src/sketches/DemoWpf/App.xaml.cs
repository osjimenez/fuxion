using Fuxion;
using Fuxion.Factories;
using Fuxion.Licensing;
using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf
{
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            //Task.WhenAll(
            //    TaskManager.StartNew(() => { }),
            //    TaskManager.StartNew(() => { throw new Exception("FALLO"); })
            //).Wait();
            //var errors = new List<string>();
            var t1 = TaskManager.StartNew(() => { throw new Exception("FALLO 1"); });
            var t2 = TaskManager.StartNew(() => { throw new Exception("FALLO 2"); });

            List<(Task task, Exception exception, T result)> WaitTasks<T>(params (Task task, T arg)[] tasks)
            {
                var errors = new List<(Task task, Exception exception, T args)>();
                foreach (var task in tasks)
                {
                    task.task.OnFaulted(ae => { errors.Add((task.task, ae.InnerExceptions.Count == 1 ? ae.InnerException : ae.Flatten(), default(T))); });
                }
                try
                {
                    Task.WhenAll(tasks.Select(t => t.task)).Wait();
                    return null;
                }
                catch {
                    return errors;
                }
            }

            var res = WaitTasks((t1, "T1"), (t2, "T2"));

            if(res != null)
            {
                foreach(var r in res)
                {
                    
                }
                Debug.WriteLine("");
            }


            Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));
            new LogWindow().Show();
            //new TimeProvider().Show();
        }
    }
}
