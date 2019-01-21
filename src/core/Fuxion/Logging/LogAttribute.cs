//using PostSharp.Aspects;
//using PostSharp.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace Fuxion.Logging
//{
//    [PSerializable]
//    public class LogAttribute : OnMethodBoundaryAspect
//    {
//        public LogAttribute() : this(true, true, null, null) { }
//        public LogAttribute(bool includeFullSignature) : this(includeFullSignature, true, null, null) { }
//        public LogAttribute(params string[] parameterNamesToInclude) : this(true, true, parameterNamesToInclude, null) { }
//        public LogAttribute(bool includeFullSignature, params string[] parameterNamesToInclude) : this(includeFullSignature, true, parameterNamesToInclude, null) { }
//        public LogAttribute(params Type[] parameterTypesToInclude) : this(true, true, null, parameterTypesToInclude) { }
//        public LogAttribute(bool includeFullSignature, params Type[] parameterTypesToInclude) : this(includeFullSignature, true, null, parameterTypesToInclude) { }
//        public LogAttribute(bool includeFullSignature, bool includeReturn, string[] parameterNamesToInclude, params Type[] parameterTypesToInclude)
//        {
//            this.includeFullSignature = includeFullSignature;
//            this.includeReturn = includeReturn;
//            this.parameterTypesToInclude = parameterTypesToInclude ?? new Type[] { };
//            this.parameterNamesToInclude = parameterNamesToInclude ?? new string[] { };
//        }
//        public class LogExecutionTag
//        {
//            public string Result { get; set; } = "";
//            public Dictionary<string, object> ParametersValues { get; set; } = new Dictionary<string, object>();
//        }
//        bool includeFullSignature;
//        bool includeReturn;
//        Type[] parameterTypesToInclude;
//        string[] parameterNamesToInclude;
//        private Dictionary<string, object> SetParametersValuesByType(MethodExecutionArgs args) {
//            var res = new Dictionary<string, object>();
//            var pars = args.Method.GetParameters();
//            for (var i = 0; i < pars.Length; i++)
//            {
//                var par = pars[i];
//                if (parameterTypesToInclude.Any(p => p.GetTypeInfo().IsAssignableFrom(args.Arguments[i].GetType().GetTypeInfo())))
//                    res[par.Name] = args.Arguments[pars.ToList().IndexOf(par)];
//            }
//            return res;
//        }
//        private Dictionary<string, object> SetParametersValuesByName(MethodExecutionArgs args)
//        {
//            var res = new Dictionary<string, object>();
//            var pars = args.Method.GetParameters();
//            for (var i = 0; i < pars.Length; i++)
//            {
//                var par = pars[i];
//                if(parameterNamesToInclude.Contains(par.Name))
//                    res[par.Name] = args.Arguments[pars.ToList().IndexOf(par)];
//            }
//            return res;
//        }
//        private string GetParameters(bool useFullNames, bool includeParametersNames, MethodBase method, object arg)
//        {
//            var res = new StringBuilder();
//            var pars = method.GetParameters();
//            var mea = (MethodExecutionArgs)arg;
//            for (var i = 0; i < pars.Length; i++)
//            {
//                var par = pars[i];
//                if (i == 0 && method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
//                    res.Append("this ");
//                if (par.ParameterType.IsByRef)
//                    res.Append("ref ");
//                else if (par.IsOut)
//                    res.Append("out ");
//                res.Append((mea.Arguments[i] != null ? mea.Arguments[i].GetType() : par.ParameterType).GetSignature(useFullNames));
//                if (includeParametersNames)
//                    res.Append(" " + par.Name);
//                if (i < pars.Length - 1)
//                    res.Append(", ");
//            }
//            return res.ToString();
//        }
//        public override void OnEntry(MethodExecutionArgs args)
//        {
//            try
//            {
//                var tag = new LogExecutionTag();
//                if (parameterTypesToInclude.Any())
//                    tag.ParametersValues = SetParametersValuesByType(args);
//                if (parameterNamesToInclude.Any())
//                    tag.ParametersValues = SetParametersValuesByName(args);
//                tag.Result = args.Method.GetSignature(false, false, true, false, true, true, GetParameters, args);
//                if (includeFullSignature || includeReturn || tag.ParametersValues.Any())
//                {
//                    tag.Result += "   ···";
//                }
//                if (includeFullSignature)
//                {
//                    tag.Result += "\r\nFULL SIGNATURE:\r\n" + args.Method.GetSignature(true, true, true, true, true, true);
//                }
//                if (tag.ParametersValues.Any())
//                {
//                    StringBuilder sb = new StringBuilder();
//                    tag.Result += "\r\nPARAMETERS:\r\n";
//                    // UNDONE - Oscar - Can apply identation to best reading
//                    tag.Result += tag.ParametersValues.Aggregate("", (agg, act) => $"{agg}\r\n{act.Value.GetType().GetSignature(includeFullSignature)} {act.Key}:\r\n{act.Value.ToJson()}\r\n", a => a.Trim('\r', '\n'));
//                }
//                args.MethodExecutionTag = tag;
//                LogManager.Create(args.Method.DeclaringType).Trace(tag.Result.Trim());
//                base.OnEntry(args);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }
//        //public override void OnSuccess(MethodExecutionArgs args)
//        //{
//        //    try
//        //    {
//        //        var tag = (LogExecutionTag)args.MethodExecutionTag;
//        //        if (includeReturn)
//        //        {
//        //            tag.Result += "\r\nRETURN - SUCCESS:";
//        //            if (args.ReturnValue != null && parameterTypesToInclude.Any(t => t.GetTypeInfo().IsAssignableFrom(args.ReturnValue.GetType().GetTypeInfo())))
//        //            {
//        //                tag.Result += "\r\n" + args.ReturnValue.ToJson();
//        //            }
//        //        }
//        //        LogManager.Create(args.Method.DeclaringType).Trace(tag.Result.Trim());
//        //        args.MethodExecutionTag = tag;
//        //        base.OnSuccess(args);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Debug.WriteLine(ex);
//        //    }
//        //}
//        public override void OnException(MethodExecutionArgs args)
//        {
//            try
//            {
//                var tag = (LogExecutionTag)args.MethodExecutionTag;
//                if (includeReturn)
//                {
//                    tag.Result += "\r\nRETURN - ERROR '" + args.Exception.GetType() + "': " + args.Exception.Message;
//                    var inner = args.Exception.InnerException;
//                    while (inner != null)
//                    {
//                        tag.Result += "\r\nINNER EXCEPTION '" + args.Exception.GetType() + "': " + inner.Message;
//                        inner = inner.InnerException;
//                    }
//                }
//                LogManager.Create(args.Method.DeclaringType).Error(tag.Result.Trim(), args.Exception);
//                args.MethodExecutionTag = tag;
//                base.OnException(args);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }
//    }
//}
