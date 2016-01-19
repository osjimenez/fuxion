#undef DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Graph
{
    public class Graph<T> where T : class
    {
        List<Edge<T>> Edges = new List<Edge<T>>();
        bool _allowCycles = false;
        public bool AllowCycles
        {
            get { return _allowCycles; }
            set
            {
                if (_allowCycles != value)
                {
                    _allowCycles = value;
                    if (!AllowCycles && HasCycles()) throw new GraphCyclicException();
                }
            }
        }
        public void AddEdge(T source, T target)
        {
            Edges.Add(new Edge<T>(source, target));
            if (!AllowCycles && HasCycles()) throw new GraphCyclicException();
        }
        public void AddEdges(params T[][] edges)
        {
            foreach (var edge in edges)
                Edges.Add(new Edge<T>(edge[0], edge[1]));
            if (!AllowCycles && HasCycles()) throw new GraphCyclicException();
        }
        public bool HasCycles()
        {
            var graph = Edges.Select(e => e.Source).Concat(Edges.Select(e => e.Target)).Distinct().Select(v => new Vertex<T>(v)).ToList();
            foreach(var vertex in graph)
            {
                var dep = Edges.Where(e => e.Source == vertex.Value).Select(e => e.Target);
                vertex.Dependencies = graph.Where(v => dep.Contains(v.Value)).ToList();
            }
            var detector = new StronglyConnectedComponentFinder<T>();
            var components = detector.DetectCycle(graph);
            return components.Cycles().Any();
        }
        public IEnumerable<T> GetDescendants(T vertex, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            Debug.WriteLine("DESCENDANTS of " + vertex);
            var res = new List<T>();
            var currentLevel = Edges.Where(e => comparer.Equals(e.Source, vertex)).Select(e => e.Target).ToList();
            Debug.WriteLine("First level: " + currentLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));
            while (currentLevel.Any())
            {
                res.AddRange(currentLevel);
                var nextLevel = Edges.Where(e => currentLevel.Contains(e.Source, comparer)).Select(e => e.Target).ToList();
                Debug.WriteLine("Next level: " + nextLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));

                currentLevel.Clear();
                currentLevel.AddRange(nextLevel);
            }
            res = res.Distinct(comparer).ToList();
            Debug.WriteLine("RESULT: " + res.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));
            return res;
        }

        public IEnumerable<T> GetAscendants(T vertex, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            Debug.WriteLine("ASCENDANTS of " + vertex);
            var res = new List<T>();
            var currentLevel = Edges.Where(e => comparer.Equals(e.Target, vertex)).Select(e => e.Source).ToList();
            Debug.WriteLine("First level: " + currentLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));
            while (currentLevel.Any())
            {
                res.AddRange(currentLevel);
                var nextLevel = Edges.Where(e => currentLevel.Contains(e.Target, comparer)).Select(e => e.Source).ToList();
                Debug.WriteLine("Next level: " + nextLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));

                currentLevel.Clear();
                currentLevel.AddRange(nextLevel);
            }
            res = res.Distinct(comparer).ToList();
            Debug.WriteLine("RESULT: " + res.Aggregate("", (c, n) => c.ToString() + " -> " + n.ToString()));
            return res;
        }
    }
    public class StronglyConnectedComponent<T> : IEnumerable<Vertex<T>>
    {
        private LinkedList<Vertex<T>> list;

        public StronglyConnectedComponent()
        {
            list = new LinkedList<Vertex<T>>();
        }

        public StronglyConnectedComponent(IEnumerable<Vertex<T>> collection)
        {
            list = new LinkedList<Vertex<T>>(collection);
        }

        public void Add(Vertex<T> vertex)
        {
            list.AddLast(vertex);
        }

        public IEnumerator<Vertex<T>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool IsCycle { get { return list.Count > 1; } }
    }
    /// <summary>
    /// Implementation of the Tarjan stronly connected components algorithm.
    /// </summary>
    /// <seealso cref="http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm"/>
    /// <seealso cref="http://stackoverflow.com/questions/261573/best-algorithm-for-detecting-cycles-in-a-directed-graph"/>
    public class StronglyConnectedComponentFinder<T>
    {
        private StronglyConnectedComponentList<T> stronglyConnectedComponents;
        private Stack<Vertex<T>> stack;
        private int index;

        /// <summary>
        /// Calculates the sets of strongly connected vertices.
        /// </summary>
        /// <param name="graph">Graph to detect cycles within.</param>
        /// <returns>Set of strongly connected components (sets of vertices)</returns>
        public StronglyConnectedComponentList<T> DetectCycle(IEnumerable<Vertex<T>> graph)
        {
            stronglyConnectedComponents = new StronglyConnectedComponentList<T>();
            index = 0;
            stack = new Stack<Vertex<T>>();
            foreach (var v in graph)
            {
                if (v.Index < 0)
                {
                    StrongConnect(v);
                }
            }
            return stronglyConnectedComponents;
        }

        private void StrongConnect(Vertex<T> v)
        {
            v.Index = index;
            v.LowLink = index;
            index++;
            stack.Push(v);

            foreach (Vertex<T> w in v.Dependencies)
            {
                if (w.Index < 0)
                {
                    StrongConnect(w);
                    v.LowLink = Math.Min(v.LowLink, w.LowLink);
                }
                else if (stack.Contains(w))
                {
                    v.LowLink = Math.Min(v.LowLink, w.Index);
                }
            }

            if (v.LowLink == v.Index)
            {
                var scc = new StronglyConnectedComponent<T>();
                Vertex<T> w;
                do
                {
                    w = stack.Pop();
                    scc.Add(w);
                } while (v != w);
                stronglyConnectedComponents.Add(scc);
            }

        }
    }
    public class StronglyConnectedComponentList<T> : IEnumerable<StronglyConnectedComponent<T>>
    {
        private LinkedList<StronglyConnectedComponent<T>> collection;

        public StronglyConnectedComponentList()
        {
            collection = new LinkedList<StronglyConnectedComponent<T>>();
        }

        public StronglyConnectedComponentList(IEnumerable<StronglyConnectedComponent<T>> collection)
        {
            this.collection = new LinkedList<StronglyConnectedComponent<T>>(collection);
        }

        public void Add(StronglyConnectedComponent<T> scc)
        {
            collection.AddLast(scc);
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        public IEnumerator<StronglyConnectedComponent<T>> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public IEnumerable<StronglyConnectedComponent<T>> IndependentComponents()
        {
            return this.Where(c => !c.IsCycle);
        }

        public IEnumerable<StronglyConnectedComponent<T>> Cycles()
        {
            return this.Where(c => c.IsCycle);
        }
    }
}
