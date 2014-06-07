using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Invert.uFrame.Editor.UnitTests
{
    [TestClass]
    public class DependencyInjectionTests
    {
        public uFrameContainer Container { get; set; }

        [TestInitialize]
        public void Init()
        {
            
        }
        [TestMethod]
        public void TestResolveMapping()
        {
            Container = new uFrameContainer();
            Container.Register<ITestInterface, ConcreteTestClassA>();
            var resolve = Container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(resolve,typeof(ConcreteTestClassA));
        }

        [TestMethod]
        public void TestResolveInstance()
        {
            Container = new uFrameContainer();
            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassA() { Name = "Test"});
            var resolve = Container.Resolve<ITestInterface>();
            Assert.IsInstanceOfType(resolve, typeof(ConcreteTestClassA));
            Assert.AreEqual(resolve.Name, "Test");
        }

        [TestMethod]
        public void TestResolveAll()
        {
            Container = new uFrameContainer();
            //Container.RegisterInstance<ITestInterface>(new ConcreteTestClassA() { Name = "TestA" }, "TestA");
            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassA() { Name = "TestA" }, "TestA");
            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassB() { Name = "TestB" }, "TestB");
            var resolve = Container.ResolveAll<ITestInterface>().ToArray();

            Assert.AreEqual(2, resolve.Length);
            Assert.IsTrue(resolve.Any(p=>p.GetType() == typeof(ConcreteTestClassA)));
            Assert.IsTrue(resolve.Any(p=>p.GetType() == typeof(ConcreteTestClassB)));
        }
        [TestMethod]
        public void TestResolveAllWithNoName()
        {
            Container = new uFrameContainer();
            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassA() { Name = "TestA" });

            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassA() { Name = "TestA" }, "TestA");
            Container.RegisterInstance<ITestInterface>(new ConcreteTestClassB() { Name = "TestB" }, "TestB");
            var resolve = Container.ResolveAll<ITestInterface>().ToArray();

            Assert.AreEqual(2, resolve.Length);
            Assert.IsTrue(resolve.Any(p => p.GetType() == typeof(ConcreteTestClassA)));
            Assert.IsTrue(resolve.Any(p => p.GetType() == typeof(ConcreteTestClassB)));
        }
        [TestMethod]
        public void TestPrimitiveResolve()
        {
            Container = new uFrameContainer();
            Container.RegisterInstance(typeof(ITestInterface), new ConcreteTestClassA() { Name = "TestA" },"Test");
            Container.RegisterInstance(typeof(ITestInterface), new ConcreteTestClassA() { Name = "TestA" },"Test2");
            Container.RegisterInstance(typeof(ITestInterface), new ConcreteTestClassA() { Name = "TestA" },"Test3");
            var results = Container.ResolveAll<ITestInterface>().ToArray();
            Assert.AreEqual(3, results.Length);

        }
    }

    public class AdapterClass
    {
        
    }

    public interface ITestInterface
    {
        string Name { get; set; }
    }

    public class ConcreteTestClassA : ITestInterface
    {
        public string Name { get; set; }
    }
    public class ConcreteTestClassB : ITestInterface
    {
        public string Name { get; set; }
    }
}
