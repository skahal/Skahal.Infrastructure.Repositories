using System;
using System.Linq;
using NUnit.Framework;
using Skahal.Infrastructure.Framework.Repositories;
using Skahal.Infrastructure.Repositories.GData;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
    [TestFixture]
    public class GDataRepositoryBaseTest
    {
        [Test]
        [Category("GData")]
        public void PersistNewItem_Item_PersistedOnGData()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateTarget();
            target.SetUnitOfWork(unitOfWork);

            var data = new SimpleDataStub();
            data.CreatedDate = DateTime.Now;
            data.Enabled = true;
            data.Name = "The name";
            data.Size = 100;
            target.Add(data);

            var originalCount = target.CountAll();
            unitOfWork.Commit();

            Assert.AreEqual(originalCount + 1, target.CountAll());
            var actual = target.FindBy(data.Id);
            Assert.AreEqual("The name", actual.Name);
        }

        [Test]
        [Category("GData")]
        public void PersistUpdatedItem_Item_PersistedOnGData()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateTarget();
            target.SetUnitOfWork(unitOfWork);

            var originalCount = target.CountAll();
            var all = target.FindAll();
            var toUpdate = all.First();
            toUpdate.Name = "updated";

            target[toUpdate.Id] = toUpdate;
            unitOfWork.Commit();

            Assert.AreEqual(originalCount, target.CountAll());

            var actual = target.FindBy(toUpdate.Id);
            Assert.AreEqual("updated", actual.Name);
        }

        [Test]
        [Category("GData")]
        public void PersistDeletedItem_Item_PersistedOnGData()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateTarget();
            target.SetUnitOfWork(unitOfWork);

            var originalCount = target.CountAll();
            var all = target.FindAll();

            var toDelete = all.FirstOrDefault();

            if (toDelete != null)
            {
                target.Remove(toDelete);
                unitOfWork.Commit();

                Assert.AreEqual(originalCount - 1, target.CountAll());
            }
        }

        private static GDataRepositoryBase<SimpleDataStub> CreateTarget()
        {
            var username = Environment.GetEnvironmentVariable("GDataUsername");
            var password = Environment.GetEnvironmentVariable("GDataPassword");

            return new GDataRepositoryBase<SimpleDataStub>("GDataRepositoryBaseTest", username, password);
        }
    }
}