using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class TaskManagerTest
    {
        public TaskManagerTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        #region Help methods
        private Task Start_void()
        {
            output.WriteLine($"Start at {DateTime.Now.ToLongTimeString()}");
            return null;
        }
        private Task<int> Start_int()
        {
            output.WriteLine($"Start at {DateTime.Now.ToLongTimeString()}");
            return null;
        }
        private void Do_void(Task task, params int[] pars)
        {
            output.WriteLine($"Parameters = '{pars.Aggregate("", (a, c) => $"{a}-{c}", a => a.Trim('-'))}'");
            try
            {
                task.Sleep(TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                output.WriteLine("Exception: " + ex.Message);
                Assert.True(false, "Task is not managed by TaskManager");
            }
            output.WriteLine($"Finished at {DateTime.Now.ToLongTimeString()}");
        }
        private async Task Do_void_async(Task task, params int[] pars)
        {
            output.WriteLine($"Parameters = '{pars.Aggregate("", (a, c) => $"{a}-{c}", a => a.Trim('-'))}'");
            await Task.Delay(100);
            try
            {
                task.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception ex)
            {
                output.WriteLine("Exception: " + ex.Message);
                Assert.True(false, "Task is not managed by TaskManager");
            }
            output.WriteLine($"Finished at {DateTime.Now.ToLongTimeString()}");
        }
        private int Do_int(Task task, params int[] pars)
        {
            Do_void(task, pars);
            return 2;
        }
        private async Task<int> Do_int_async(Task task, params int[] pars)
        {
            await Do_void_async(task, pars);
            return 2;
        }
        private void Assert_void(Task task)
        {
            if (task is Task<Task>)
            {
                output.WriteLine("Return task is Task<Task>> !!!");
                (task as Task<Task>).Unwrap().Wait();
            }
            else task.Wait();
        }
        private void Assert_int(Task<int> task)
        {
            var res = task.WaitResult();
            Assert.Equal(res, 2);
        }
        #endregion
        #region void_StartNew
        [Fact]
        public void TaskManager_void_StartNew()
        {
            var task = Start_void();
            task = TaskManager.StartNew(() => Do_void(task));
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1()
        {
            var task = Start_void();
            task = TaskManager.StartNew(p1 => Do_void(task, p1), 1);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2) => Do_void(task, p1, p2), 1, 2);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3) => Do_void(task, p1, p2, p3), 1, 2, 3);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4) => Do_void(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5) => Do_void(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6) => Do_void(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7) => Do_void(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8) => Do_void(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_void(task);
        }
        //[Fact]
        //public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    var task = Start_void();
        //    task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8, p9) => Do_void(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_void(task);
        //}
        #endregion
        #region void_StartNew_async
        [Fact]
        public void TaskManager_void_StartNew_async()
        {
            var task = Start_void();
            task = TaskManager.StartNew(async () => await Do_void_async(task));
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_Startnew_async_p1()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async p1 => await Do_void_async(task, p1), 1);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2) => await Do_void_async(task, p1, p2), 1, 2);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3) => await Do_void_async(task, p1, p2, p3), 1, 2, 3);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4) => await Do_void_async(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5) => await Do_void_async(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6) => await Do_void_async(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_void(task);
        }
        //[Fact]
        //public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    Task task = Start_void();
        //    task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8, p9) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_void(task);
        //}
        #endregion
        #region int_StartNew
        [Fact]
        public void TaskManager_int_StartNew()
        {
            var task = Start_int();
            task = TaskManager.StartNew(() => Do_int(task));
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1()
        {
            var task = Start_int();
            task = TaskManager.StartNew(p1 => Do_int(task, p1), 1);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2) => Do_int(task, p1, p2), 1, 2);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3) => Do_int(task, p1, p2, p3), 1, 2, 3);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4) => Do_int(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5) => Do_int(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6) => Do_int(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7) => Do_int(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8) => Do_int(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_int(task);
        }
        //[Fact]
        //public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    var task = Start_int();
        //    task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8, p9) => Do_int(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_int(task);
        //}
        #endregion
        #region int_StartNew_async
        [Fact]
        public void TaskManager_int_StartNew_async()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async () => await Do_int_async(task));
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async p1 => await Do_int_async(task, p1), 1);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2) => await Do_int_async(task, p1, p2), 1, 2);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3) => await Do_int_async(task, p1, p2, p3), 1, 2, 3);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4) => await Do_int_async(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5) => await Do_int_async(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6) => await Do_int_async(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_int(task);
        }
        //[Fact]
        //public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    var task = Start_int();
        //    task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8, p9) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_int(task);
        //}
        #endregion
    }
}
