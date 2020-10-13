using System;
using System.Collections.Generic;

namespace Fuxion.Collections.Generic
{
    public class GenericEqualityComparer<T> : EqualityComparer<T>
    {
        public GenericEqualityComparer(Func<T?, T?, bool> equalsFunction, Func<T, int> getHashCodeFunction)
        {
            this.equalsFunction = equalsFunction;
            this.getHashCodeFunction = getHashCodeFunction;
        }
        private readonly Func<T?, T?, bool> equalsFunction;
        private readonly Func<T, int> getHashCodeFunction;
        public override bool Equals(T? x, T? y) { return equalsFunction.Invoke(x, y); }
        public override int GetHashCode(T obj) { return getHashCodeFunction.Invoke(obj); }
    }
}
