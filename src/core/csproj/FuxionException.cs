using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class FuxionException : Exception
    {
        public FuxionException() : base() { }
        public FuxionException(string message) : base(message) { }
        public FuxionException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class FuxionAggregateException : AggregateException
    {
        public FuxionAggregateException() : base() { }
        public FuxionAggregateException(string message) : base(message) { }
        public FuxionAggregateException(Exception[] innerExceptions) : base(innerExceptions) { }
        public FuxionAggregateException(IEnumerable<Exception> innerExceptions) : base(innerExceptions) { }
        public FuxionAggregateException(string message, Exception innerException) : base(message, innerException) { }
        public FuxionAggregateException(string message, params Exception[] innerExceptions) : base(message,innerExceptions) { }
        public FuxionAggregateException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions) { }
    }
    public class FuxionAggregateException<TInnerExceptions> : AggregateException where TInnerExceptions : Exception
    {
        public FuxionAggregateException() : base() { }
        public FuxionAggregateException(string message) : base(message) { }
        public FuxionAggregateException(TInnerExceptions[] innerExceptions) : base(innerExceptions) { }
        public FuxionAggregateException(IEnumerable<TInnerExceptions> innerExceptions) : base(innerExceptions) { }
        public FuxionAggregateException(string message, TInnerExceptions innerException) : base(message, innerException) { }
        public FuxionAggregateException(string message, params TInnerExceptions[] innerExceptions) : base(message, innerExceptions) { }
        public FuxionAggregateException(string message, IEnumerable<TInnerExceptions> innerExceptions) : base(message, innerExceptions) { }
    }
}
