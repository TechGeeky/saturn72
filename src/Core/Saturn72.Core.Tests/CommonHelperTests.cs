﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Saturn72.Core.Tests.TestObjects;
using Shouldly;

#endregion

namespace Saturn72.Core.Tests
{
    public class CommonHelperTests
    {
        [Test]
        public void IsValidEmail_AllTests()
        {
            var validEmails = new[] {"rrr@rmail.com", "123@essss.com"};
            var invalidEmails = new[] {"rrr@w.com", "@e.com"};

            foreach (var email in validEmails)
                CommonHelper.IsValidEmail(email).ShouldBeTrue();

            //foreach (var email in invalidEmails)
            //{
            //    CommonHelper.IsValidEmail(email).ShouldBeFalse();
            //}
        }

        [Test]
        public void GetCompatibleTypeName()
        {
            const string stringCompatibleName = "System.String, mscorlib";

            CommonHelper.GetCompatibleTypeName<string>().ShouldBe(stringCompatibleName);
            CommonHelper.GetCompatibleTypeName(typeof(string)).ShouldBe(stringCompatibleName);
        }

        [Test]
        public void TryGetTypeFromAppComain_ReturnsaType()
        {
            CommonHelper.TryGetTypeFromAppDomain("string").ShouldBeOfType<string>();
            CommonHelper.TryGetTypeFromAppDomain("System.String, mscorlib").ShouldBeOfType<string>();
        }

        [Test]
        public void TryGetTypeFromAppComain_ReturnsaNull()
        {
            CommonHelper.TryGetTypeFromAppDomain("dddd").ShouldBeNull();
            CommonHelper.TryGetTypeFromAppDomain("notExsistTypeName, NotExistAssembly").ShouldBeNull();
            CommonHelper.TryGetTypeFromAppDomain("notExsistTypeName, NotExistAssembly, whatever").ShouldBeNull();
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeName_ThrowsOnBadTypeName()
        {
            var typeName = "BlaBla, BlaBla";
            Should.Throw<ArgumentException>(() => CommonHelper.GetTypeFromAppDomain(typeName));

            typeName = "RRR";
            Should.Throw<ArgumentException>(() => CommonHelper.GetTypeFromAppDomain(typeName));
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeName_ReturnsNull()
        {
            var typeName = "Saturn72.Core.Tests.BlaBla, Saturn72.Core.Tests";
            CommonHelper.GetTypeFromAppDomain(typeName).ShouldBeNull();
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeName_GetsType()
        {
            var typeName = "Saturn72.Core.Tests.TestObjects.TestObject, Saturn72.Core.Tests";
            var t = CommonHelper.GetTypeFromAppDomain(typeName);
            t.ShouldBeOfType<TestObject>();
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeAndAssemblyNames_ThrowsOnBadTypeName()
        {
            var typeName = "RRR";
            Should.Throw<ArgumentException>(() => CommonHelper.GetTypeFromAppDomain("", typeName));
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeAndAssemblyNames_ReturnsNull()
        {
            var typeName = "Saturn72.Core.Tests.BlaBla, Saturn72.Core.Tests";
            CommonHelper.GetTypeFromAppDomain(typeName).ShouldBeNull();
        }

        [Test]
        public void GetTypeFromAppDomain_FromTypeAndAssemblyNames_GetsType()
        {
            CommonHelper.GetTypeFromAppDomain("Saturn72.Core.Tests.TestObjects.TestObject", "Saturn72.Core.Tests")
                .ShouldBeOfType<TestObject>();
        }

        [Test]
        public void CreateInstnce_CreatesInstanceFromTypeName()
        {
            var type = typeof(List);
            var typeName = type.FullName + ", " + type.Assembly.GetName().Name;
            var instance = CommonHelper.CreateInstance<List>(typeName);

            Assert.IsInstanceOf<List>(instance);
        }

        [Test]
        public void CreateInstnce_CreatesInstanceFromTypeNameWithParams()
        {
            var type = typeof(TestObject);
            var typeName = type.FullName + ", " + type.Assembly.GetName().Name;
            var instance = CommonHelper.CreateInstance<TestObject>(typeName, "objName", new List<string> {"1", "2", "3"});

            Assert.IsInstanceOf<TestObject>(instance);
            instance.Name.ShouldBe("objName");
            instance.List.ToList().Count.ShouldBe(3);
        }


        [Test]
        public void CreateInstnce_CreatesInstanceFromType()
        {
            var type = typeof(List);
            var instance = CommonHelper.CreateInstance<List>(type);

            Assert.IsInstanceOf<List>(instance);
        }

        [Test]
        public void CreateInstnce_CreatesInstanceFromTypeWithParams()
        {
            var type = typeof(TestObject);
            var instance = CommonHelper.CreateInstance<TestObject>(type, "objName", new List<string> {"1", "2", "3"});

            Assert.IsInstanceOf<TestObject>(instance);
            instance.Name.ShouldBe("objName");
            instance.List.ToList().Count.ShouldBe(3);
        }

        [Test]
        public void CreateInstnce_ThrowsOnBadTypeName()
        {
            Assert.Throws<ArgumentException>(() => CommonHelper.CreateInstance<List>("TTT"));

            Assert.Throws<ArgumentException>(() => CommonHelper.CreateInstance<List>("TTT, TTT"));
            Assert.Throws<ArgumentException>(() => CommonHelper.CreateInstance<List>("TTT,System"));
        }

        [Test]
        public void CreateInstnce_NullOnBadServiceRequested()
        {
            var instance = CommonHelper.CreateInstance<string>(typeof(List));

            Assert.IsNull(instance);
        }

        [Test]
        public void ToInt_returnsInteger()
        {
            var res = CommonHelper.To<int>("100");
            res.ShouldBe(100);
        }

        [Test]
        public void ToInt_ReturnsZeroResult()
        {
            var res = CommonHelper.ToInt("");
            res.ShouldBe(0);

            res = CommonHelper.ToInt("dwwd");
            res.ShouldBe(0);

            res = CommonHelper.ToInt(null);
            res.ShouldBe(0);
        }

        [Test]
        public void CommonHelper_Copy_Throws()
        {
            //different type
            Should.Throw<InvalidOperationException>(
                () => CommonHelper.Copy(new DummyClass(), new DummyClassChild()));
        }

        [Test]
        public void CommonHelper_Copy_CreateNew()
        {
            var source = new DummyClass();
            var dest = CommonHelper.Copy(source);

            dest.InternalString.ShouldBe(source.InternalString);
            dest.StringWithSetter.ShouldBe(source.StringWithSetter);
            dest.StringWithoutSetter.ShouldBe(source.StringWithoutSetter);
        }

        [Test]
        public void CommonHelper_Copy_ToInstance()
        {
            var source = new DummyClass
            {
                StringWithSetter = "string with setter",
                InternalString = "internal string"
            };

            var dest = new DummyClass();

            CommonHelper.Copy(source, dest);

            dest.InternalString.ShouldBe(source.InternalString);
            dest.StringWithSetter.ShouldBe(source.StringWithSetter);
            dest.StringWithoutSetter.ShouldBe(source.StringWithoutSetter);
        }

        [Test]
        public void CommonHelper_Copy_ProtectedProperties()
        {
            var source = new DummyClassChild
            {
                StringWithSetter = "string with setter",
                InternalString = "internal string"
            };
            source.SetProtectedString("value");

            var dest = new DummyClassChild();

            CommonHelper.Copy(source, dest);

            dest.InternalString.ShouldBe(source.InternalString);
            dest.StringWithSetter.ShouldBe(source.StringWithSetter);
            dest.StringWithoutSetter.ShouldBe(source.StringWithoutSetter);
        }


        internal class DummyClass
        {
            public string StringWithSetter { get; set; }

            public string StringWithoutSetter { get; } = "TTT";

            internal string InternalString { get; set; }
            protected string ProtectedString { get; set; }
        }

        internal class DummyClassChild : DummyClass
        {
            public string GetProtectedString
            {
                get { return ProtectedString; }
            }

            public void SetProtectedString(string value)
            {
                ProtectedString = value;
            }
        }
    }
}