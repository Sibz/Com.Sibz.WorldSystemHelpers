using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using UnityEngine.TestTools;

namespace Sibz.WorldSystemHelpers.Tests
{
    public class ImportSystemsWithAttribute : TestBase
    {
        [Test]
        public void WhenGeneric_ShouldCreateSystem()
        {
            World.ImportSystemsWithAttribute<MyTestAttribute, TestSystemGroup>();
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }
        [Test]
        public void WhenPartiallyGeneric_ShouldCreateSystem()
        {
            World.ImportSystemsWithAttribute<TestSystemGroup>(typeof(MyTestAttribute));
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }

        [Test]
        public void WhenNotGeneric_ShouldCreateSystem()
        {
            World.ImportSystemsWithAttribute(typeof(MyTestAttribute), typeof(TestSystemGroup));
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }
    }

    public class ImportSystemsFromList : TestBase
    {
        private IEnumerable<Type> systems;
        [SetUp]
        public void SetUpList()
        {
            systems = new[] {typeof(TestSystem)};
        }
        [Test]
        public void WhenGeneric_ShouldCreateSystems()
        {
            World.ImportSystemsFromList<TestSystemGroup>(systems);
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }

        [Test]
        public void WhenNotGeneric_ShouldCreateSystems()
        {
            World.ImportSystemsFromList(systems, typeof(TestSystemGroup));
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }
    }

    public class TryCreateInGroupUsingUpdateInGroupAttribute : WithSystemTypeTests
    {
        protected override void ThrowMethod(Type type)
        {
            World.TryCreateInGroupUsingUpdateInGroupAttribute(type);
        }

        [Test]
        public void WhenNoAttribute_ShouldReturnFalse()
        {
            Assert.IsFalse(World.TryCreateInGroupUsingUpdateInGroupAttribute(typeof(TestSystem)));
        }

        [Test]
        public void WhenGroupDoesntExist_ShouldReturnFalse()
        {
            World w = new World("Test_WhenGroupDoesntExist_ShouldReturnFalse");
            bool result = w.TryCreateInGroupUsingUpdateInGroupAttribute(typeof(TestSystem3));
            w.Dispose();
            Assert.IsFalse(result);
        }

        [Test]
        public void WhenGroupDoesntExist_ShouldLogWarning()
        {
            World w = new World("Test_WhenGroupDoesntExist_ShouldReturnFalse");
            bool result = w.TryCreateInGroupUsingUpdateInGroupAttribute(typeof(TestSystem3));
            w.Dispose();
            LogAssert.Expect(LogType.Warning, new Regex(".*"));
        }

        [Test]
        public void WhenGroupExist_ShouldReturnTrue()
        {
            Assert.IsTrue(World.TryCreateInGroupUsingUpdateInGroupAttribute(typeof(TestSystem3)));
        }

        [Test]
        public void WhenGroupExist_ShouldAddGroup()
        {
            World.TryCreateInGroupUsingUpdateInGroupAttribute(typeof(TestSystem3));
            Assert.IsNotNull(World.GetExistingSystem<TestSystem3>());
        }

    }

    public class TryGetExistingSystem : WithSystemTypeTests
    {
        protected override void ThrowMethod(Type type)
        {
            World.TryGetExistingSystem(type, out ComponentSystem c);
        }

        [Test]
        public void WhenExists_ReturnsTrue()
        {
            Assert.IsTrue(World.TryGetExistingSystem(typeof(TestSystem2), out ComponentSystem s));
        }

        [Test]
        public void WhenDoesNotExist_ReturnsTrue()
        {
            Assert.False(World.TryGetExistingSystem(typeof(TestSystem), out ComponentSystem s));
        }
    }

    public class TryCreateInGroup : WithSystemTypeTests
    {
        protected override void ThrowMethod(Type type)
        {
            World.TryCreateInGroup<TestSystemGroup>(type);
        }

        [Test]
        public void WhenGroupExists_ShouldReturnTrue()
        {
            Assert.IsTrue(World.TryCreateInGroup<TestSystemGroup>(typeof(TestSystem)));
        }

        [Test]
        public void WhenGroupExists_GroupShouldBeAdded()
        {
            World.TryCreateInGroup<TestSystemGroup>(typeof(TestSystem));
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }

        [Test]
        public void WhenGroupDoesNotExist_ShouldReturnFalse()
        {
            World w = new World("Test_WhenGroupDoesNotExist_ShouldReturnFalse");
            bool result = w.TryCreateInGroup<TestSystemGroup>(typeof(TestSystem));
            w.Dispose();
            Assert.IsFalse(result);
        }

        [Test]
        public void WhenGroupDoesNotExist_ShouldLogWarning()
        {
            World w = new World("Test_WhenGroupDoesNotExist_ShouldLogWarning");
            w.TryCreateInGroup<TestSystemGroup>(typeof(TestSystem));
            w.Dispose();
            LogAssert.Expect(LogType.Warning, new Regex(".*"));
        }
    }
    public class CreateInGroup : WithSystemTypeTests
    {
        protected override void ThrowMethod(Type type)
        {
            World.CreateInGroup(type, TestGroup);
        }

        [Test]
        public void WhenComponentSystemIsNull_ShouldThrowArgumentNullException()
        {
            CatchWithParamName<ArgumentNullException>(() => { World.CreateInGroup(typeof(TestSystem), null); },
                typeof(WorldExtensions).GetMethod(nameof(WorldExtensions.CreateInGroup))?.GetParameters().Last().Name );
        }

        [Test]
        public void ShouldCreateSystem()
        {
            World.CreateInGroup(typeof(TestSystem), TestGroup);
            Assert.IsNotNull(World.GetExistingSystem<TestSystem>());
        }

        [Test]
        public void ShouldAddToGroupUpdateList()
        {
            World.CreateInGroup(typeof(TestSystem), TestGroup);
            TestGroup.Update();
            Assert.IsTrue(World.GetExistingSystem<TestSystem>().Updated);
        }

        [Test]
        public void ShouldSortOrder()
        {
            World.CreateInGroup(typeof(TestSystem), TestGroup);
            TestGroup.Update();
            Assert.IsTrue(World.GetExistingSystem<TestSystem2>().UpdatedAfterSystem1);
        }
    }

    public class ThrowIfNotComponentSystem : WithSystemTypeTests
    {
        protected override void ThrowMethod(Type type)
        {
            WorldExtensions.ThrowIfNotComponentSystem(type);
        }
    }

    public abstract class WithSystemTypeTests : TestBase
    {
        protected abstract void ThrowMethod(Type type);

        [Test]
        public void WhenSystemTypeParamNull_ShouldThrowArgumentNullException()
        {
            CatchWithParamName<ArgumentNullException>(() => { ThrowMethod(null); }, "systemType");
        }

        [Test]
        public void WhenSystemTypeParamNotComponentSystem_ShouldThrowArgumentException()
        {
            CatchWithParamName<ArgumentException>(() => { ThrowMethod(typeof(string)); }, "systemType");
        }

    }

    public abstract class TestBase
    {
        protected World World;
        protected TestSystemGroup TestGroup;


        [SetUp]
        public void SetUp()
        {
            World = new World("Test");
            TestGroup = World.CreateSystem<TestSystemGroup>();
            TestGroup.AddSystemToUpdateList(World.CreateSystem<TestSystem2>());
        }

        [TearDown]
        public void TearDown()
        {
            World.Dispose();
        }


        public void CatchWithParamName<T>(TestDelegate action, string paramName)
            where T : ArgumentException
        {
            try
            {
                action.Invoke();
            }
            catch (T e)
            {
                if (e.ParamName == paramName)
                    Assert.Pass();
                else
                    Assert.Fail("Exception thrown but ParamName doesn't match");
                return;
            }

            Assert.Fail($"{typeof(T).Name} not thrown");
        }
    }

    public class TestSystemGroup : ComponentSystemGroup
    {
    }

    [MyTest]
    public class TestSystem : ComponentSystem
    {
        public bool Updated;

        protected override void OnUpdate()
        {
            Updated = true;
        }
    }

    [UpdateAfter(typeof(TestSystem))]
    public class TestSystem2 : ComponentSystem
    {
        public bool UpdatedAfterSystem1;

        protected override void OnUpdate()
        {
            UpdatedAfterSystem1 = World.GetExistingSystem<TestSystem>().Updated;
        }
    }

    [UpdateInGroup(typeof(TestSystemGroup))]
    public class TestSystem3 : ComponentSystem
    {
        public bool Updated;

        protected override void OnUpdate()
        {
            Updated = true;
        }
    }

    public class MyTestAttribute : Attribute
    {

    }

}