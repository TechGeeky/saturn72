﻿
using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Saturn72.Core.Audit;
using Saturn72.UnitTesting.Framework;

namespace Saturn72.Core.Services.Impl.Tests
{
    public class AuditHelperTests
    {
        [Test]
        public void AuditHelper_PrepareForCreate_DoesNotThrwos_OnNull()
        {
            new AuditHelper(null).PrepareForCreateAudity(null);
        }

        [Test]
        public void AuditHelper_PrepareForCreate_Thrwos()
        {
            var aHelper = new AuditHelper(null);
            var audit = new DummyFullAudit
            {
                CreatedOnUtc = DateTime.Now.AddDays(10)
            };
            typeof(InvalidOperationException).ShouldBeThrownBy(() => aHelper.PrepareForCreateAudity(audit));

            audit = new DummyFullAudit
            {
                CreatedByUserId = 100
            };
            typeof(InvalidOperationException).ShouldBeThrownBy(() => aHelper.PrepareForCreateAudity(audit));
        }

        [Test]
        public void AuditHelper_PrepareForCreate_AddAudity()
        {
            var wc = new Mock<IWorkContext>();
            var userId = 100;
            wc.Setup(w => w.CurrentUserId)
                .Returns(userId);
            var aHelper = new AuditHelper(wc.Object);
            var audit = new DummyFullAudit();
            aHelper.PrepareForCreateAudity(audit)
                ;
            audit.CreatedByUserId.ShouldEqual(userId);
            audit.CreatedOnUtc.ShouldNotEqual(default(DateTime));
            audit.CreatedOnUtc.ShouldBeSmallerOrEqualTo(DateTime.Now);
        }

        [Test]
        public void AuditHelper_PrepareForUpdate_DoesNotThrwos_OnNull()
        {
            new AuditHelper(null).PrepareForUpdateAudity(null);
        }

        [Test]
        public void AuditHelper_PrepareForUpdate_AddAudity_WithoutCreate()
        {
            var wc = new Mock<IWorkContext>();
            var userId = 100;
            wc.Setup(w => w.CurrentUserId)
                .Returns(userId)
                .Callback(() => Thread.Sleep(50));

            var createdOn = DateTime.Now.AddDays(-10);
            var createdByUserId = 110;
            var aHelper = new AuditHelper(wc.Object);
            var audit = new DummyFullAudit
            {
                CreatedOnUtc = createdOn,
                CreatedByUserId = createdByUserId
            };
            aHelper.PrepareForUpdateAudity(audit);
            audit.CreatedByUserId.ShouldEqual(createdByUserId);
            audit.CreatedOnUtc.ShouldEqual(createdOn);

            audit.UpdatedByUserId.ShouldEqual(userId);
            audit.UpdatedOnUtc.ShouldBeGreaterThan(audit.CreatedOnUtc);
            audit.UpdatedOnUtc.ShouldBeSmallerOrEqualTo(DateTime.Now);

        }
        [Test]
        public void AuditHelper_PrepareForUpdate_AddAudity_WithCreate()
        {
            var wc = new Mock<IWorkContext>();
            var userId = 100;
            wc.Setup(w => w.CurrentUserId)
                .Returns(userId);

            var aHelper = new AuditHelper(wc.Object);
            var audit = new DummyFullAudit();
            aHelper.PrepareForUpdateAudity(audit);
            audit.CreatedByUserId.ShouldEqual(userId);
            audit.CreatedOnUtc.ShouldNotEqual(default(DateTime));
            audit.CreatedOnUtc.ShouldBeSmallerThan(DateTime.Now);

            audit.UpdatedByUserId.ShouldEqual(userId);
            audit.UpdatedOnUtc.ShouldBeGreaterOrEqualTo(audit.CreatedOnUtc);
            audit.UpdatedOnUtc.ShouldBeSmallerOrEqualTo(DateTime.Now);

        }
        [Test]
        public void AuditHelper_PrepareForelete_DoesNotThrwos_OnNull()
        {
            new AuditHelper(null).PrepareForDeleteAudity(null);
        }
        [Test]
        public void AuditHelper_PrepareForDelete_Thrwos()
        {
            var aHelper = new AuditHelper(null);
            var audit = new DummyFullAudit
            {
                DeletedOnUtc = DateTime.Now.AddDays(100)
            };
            typeof(InvalidOperationException).ShouldBeThrownBy(() => aHelper.PrepareForDeleteAudity(audit));

            audit = new DummyFullAudit
            {
                DeletedByUserId = 100
            };
            typeof(InvalidOperationException).ShouldBeThrownBy(() => aHelper.PrepareForDeleteAudity(audit));

            audit = new DummyFullAudit
            {
                Deleted = true
            };
            typeof(InvalidOperationException).ShouldBeThrownBy(() => aHelper.PrepareForDeleteAudity(audit));
        }

        [Test]
        public void AuditHelper_PrepareForDelete_AddAudity_DeletedByNotAssigned()
        {
            var wc = new Mock<IWorkContext>();
            var userId = 100;
            wc.Setup(w => w.CurrentUserId)
                .Returns(userId)
                .Callback(() => Thread.Sleep(50));

            var aHelper = new AuditHelper(wc.Object);
            var audit = new DummyFullAudit();
            aHelper.PrepareForDeleteAudity(audit);

            audit.DeletedByUserId.ShouldEqual(userId);
            audit.DeletedOnUtc.ShouldNotBeNull();
            audit.DeletedOnUtc.Value.ShouldBeSmallerThan(DateTime.Now);
            audit.Deleted.ShouldBeTrue();

        }

        [Test]
        public void AuditHelper_PrepareForDelete_AddAudity_DeletedByAssignedToIllegalUser()
        {
            var wc = new Mock<IWorkContext>();
            var userId = 100;
            wc.Setup(w => w.CurrentUserId)
                .Returns(userId)
                .Callback(() => Thread.Sleep(50));

            var aHelper = new AuditHelper(wc.Object);
            var audit1 = new DummyFullAudit();
            audit1.DeletedByUserId = 0;
            aHelper.PrepareForDeleteAudity(audit1);

            audit1.DeletedByUserId.ShouldEqual(userId);
            audit1.DeletedOnUtc.ShouldNotBeNull();
            audit1.DeletedOnUtc.Value.ShouldBeSmallerThan(DateTime.Now);
            audit1.Deleted.ShouldBeTrue();

            var audit2 = new DummyFullAudit();
            audit2.DeletedByUserId = -123;
            aHelper.PrepareForDeleteAudity(audit2);

            audit2.DeletedByUserId.ShouldEqual(userId);
            audit2.DeletedOnUtc.ShouldNotBeNull();
            audit2.DeletedOnUtc.Value.ShouldBeSmallerThan(DateTime.Now);
            audit2.Deleted.ShouldBeTrue();
        }

        internal class DummyFullAudit : ICrudAudit
        {
            public DateTime CreatedOnUtc { get; set; }
            public long CreatedByUserId { get; set; }
            public DateTime? UpdatedOnUtc { get; set; }
            public long? UpdatedByUserId { get; set; }
            public bool Deleted { get; set; }
            public DateTime? DeletedOnUtc { get; set; }
            public long? DeletedByUserId { get; set; }
        }
    }
}
