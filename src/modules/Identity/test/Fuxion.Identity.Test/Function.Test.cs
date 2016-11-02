using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fuxion.Identity.Test.Mocks;
using static Fuxion.Identity.Functions;
namespace Fuxion.Identity.Test
{
    [TestClass]
    public class FunctionTest : BaseTestClass
    {
        [TestMethod]
        public void Validate()
        {
            Assert.IsFalse(new GuidFunction(Guid.NewGuid(), null).IsValid());
            Assert.IsFalse(new GuidFunction(Guid.NewGuid(), "").IsValid());
            Assert.IsFalse(new GuidFunction(Guid.NewGuid(), " ").IsValid());
            Assert.IsFalse(new GuidFunction(default(Guid), "valid").IsValid());
            Assert.IsTrue(new GuidFunction(Guid.NewGuid(), "valid").IsValid());

            Assert.IsFalse(new StringFunction(null).IsValid());
            Assert.IsFalse(new StringFunction("").IsValid());
            Assert.IsFalse(new StringFunction(" ").IsValid());
            Assert.IsTrue(new StringFunction("valid").IsValid());
        }
        //GuidFunction Read { get { return new GuidFunction(Guid.Parse("{00000000-0000-0000-0000-000000000001}"), "Read"); } }
        //StringFunction StrRead { get { return new StringFunction("Read"); } }
        //GuidFunction Write { get { return new GuidFunction(Guid.Parse("{00000000-0000-0000-0000-000000000002}"), "Write"); } }
        //StringFunction StrWrite { get { return new StringFunction("Write"); } }
        //GuidFunction Admin { get { return new GuidFunction(Guid.Parse("{00000000-0000-0000-0000-000000000003}"), "Admin"); } }
        //StringFunction StrAdmin { get { return new StringFunction("Admin"); } }


        [TestMethod]
        public void IncludesAndExcludes()
        {
            Assert.AreEqual(Read.GetAllInclusions().Count(), 1, "Read shouldn't include anything");
            Assert.AreEqual(Read.GetAllExclusions().Count(), 6, "Read should exclude 6 functions");
            Assert.AreEqual(Edit.GetAllInclusions().Count(), 2, "Edit should include 2 function");
            Assert.AreEqual(Edit.GetAllExclusions().Count(), 5, "Edit should exclude 5 functions");
            var Custom = AddCustom("CUSTOM", new[] { Read }, new[] { Manage });
            Assert.AreEqual(Custom.GetAllInclusions().Count(), 2, "Custom should include 1 function");
            Assert.AreEqual(Custom.GetAllExclusions().Count(), 3, "Custom should exclude 3 functions");
        }

        //[TestMethod]
        //public void WhenGraph_CheckDescendants()
        //{
        //    var g = GetGraph();
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Read), new Function[] { }), "Descendants of READ are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Edit), new[] { Read }), "Descendants of EDIT are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Create), new[] { Edit, Read }), "Descendants of CREEATE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Delete), new[] { Edit, Read }), "Descendants of DELETE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Manage), new[] { Create, Delete, Edit, Read }), "Descendants of MANAGE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetDescendants(Admin), new[] { Manage, Create, Delete, Edit, Read }), "Descendants of ADMIN are incorrect");
        //}
        //[TestMethod]
        //public void WhenGraph_CheckAscendants()
        //{
        //    var g = GetGraph();
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Read), new[] { Edit, Create, Delete, Manage, Admin }), "Ascendants of READ are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Edit), new[] { Create, Delete, Manage, Admin }), "Ascendants of EDIT are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Create), new[] { Manage, Admin }), "Ascendants of CREATE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Delete), new[] { Manage, Admin }), "Ascendants of DELETE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Manage), new[] { Admin }), "Ascendants of MANAGE are incorrect");
        //    Assert.IsTrue(Enumerable.SequenceEqual(g.GetAncesdants(Admin), new Function[] { }), "Ascendants of ADMIN are incorrect");
        //}
        //[TestMethod]
        //public void WhenGraph_DetectCycles()
        //{
        //    var g = GetGraph();
        //    Assert.IsFalse(g.HasCycles());
        //    try {
        //        g.AddEdge(Read, Manage);
        //        Assert.Fail();
        //    }
        //    catch (GraphCyclicException) { }
        //    g.AllowCycles = true;
        //    g.AddEdge(Read, Manage);
        //    Assert.IsTrue(g.HasCycles());
        //}
        //private AdjacencyGraph<Function, Edge<Function>> GetFunctionsGraph()
        //{
        //    var read = new Function(Guid.NewGuid(), "READ");
        //    var edit = new Function(Guid.NewGuid(), "EDIT");
        //    var create = new Function(Guid.NewGuid(), "CREATE");
        //    var delete = new Function(Guid.NewGuid(), "DELETE");
        //    var manage = new Function(Guid.NewGuid(), "MANAGE");
        //    var admin = new Function(Guid.NewGuid(), "ADMIN");

        //    var graph = new AdjacencyGraph<Function, Edge<Function>>(false);
        //    graph.AddVertexRange(new[] { read, edit, create, delete, manage, admin });

        //    graph.AddEdge(new Edge<Function>(admin, manage));
        //    graph.AddEdge(new Edge<Function>(manage, create));
        //    graph.AddEdge(new Edge<Function>(manage, delete));
        //    graph.AddEdge(new Edge<Function>(create, edit));
        //    graph.AddEdge(new Edge<Function>(delete, edit));
        //    graph.AddEdge(new Edge<Function>(edit, read));

        //    return graph;
        //}
        //[TestMethod]
        //public void WhenGraph()
        //{
        //    var graph = GetFunctionsGraph();

        //    // Use IVertexListGraph<Function,Edge<Function>>.IsDirectedAcyclicGraph() to search for cycles
        //    Assert.IsTrue(graph.IsDirectedAcyclicGraph(), "Contains cycles, but shouldn't");

        //    graph.AddEdge(new Edge<Function>(
        //        graph.Vertices.Single(v => v.Name == "MANAGE"),
        //        graph.Vertices.Single(v => v.Name == "ADMIN")));
        //    Assert.IsFalse(graph.IsDirectedAcyclicGraph(), "Don't contains cycles, but should");
        //}
        //[TestMethod]
        //public void WhenGraph_GetDescendants()
        //{
        //    var graph = GetFunctionsGraph();
        //    var res = graph.TopologicalSort();
        //    var dfs = new DepthFirstSearchAlgorithm<Function, Edge<Function>>(graph);
        //    dfs.InitializeVertex += vertex => Console.WriteLine($"Vertex initialize = {vertex.Name}");
        //    dfs.DiscoverVertex += vertex => Console.WriteLine($"Vertex discovered = {vertex.Name}");
        //    dfs.ExamineEdge += edge => Console.WriteLine($"Edge examined = {edge.Source.Name} => {edge.Target.Name}");
        //    dfs.TreeEdge += edge => Console.WriteLine($"Edge tree = {edge.Source.Name} => {edge.Target.Name}");
        //    dfs.FinishVertex += vertex => Console.WriteLine($"Vertex finished = {vertex.Name}");
        //    dfs.Compute(graph.Vertices.Single(v => v.Name == "CREATE"));
        //    Debug.WriteLine(res);
        //}
    }
}
