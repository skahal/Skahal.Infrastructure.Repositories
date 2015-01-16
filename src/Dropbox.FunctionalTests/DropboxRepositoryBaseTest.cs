using System;
using System.Linq;
using NUnit.Framework;
using Skahal.Infrastructure.Framework.Repositories;

namespace Skahal.Infrastructure.Repositories.Dropbox.FunctionalTests
{
    [TestFixture]
    public class DropboxRepositoryBaseTest
    {
        [Test]
        [Category("Dropbox")]
        public void PersistNewItem_Item_PersistedOnDropbox()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateClient();
            target.ClearAll();
            target.SetUnitOfWork(unitOfWork);

            var data = new SimpleDataStub();
            data.Name = "Test1";
            target.Add(data);

            var originalCount = target.CountAll();
            unitOfWork.Commit();

            Assert.AreEqual(originalCount + 1, target.CountAll());
        }

        [Test]
        [Category("Dropbox")]
        public void CountAll_Filter_FiltedResults()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateClient();
            target.ClearAll();
            target.SetUnitOfWork(unitOfWork);

            var data = new SimpleDataStub();
            data.Name = "TEST_1";
            target.Add(data);

            data = new SimpleDataStub();
            data.Name = "TEST_2";
            target.Add(data);

            unitOfWork.Commit();

            Assert.AreEqual(2, target.CountAll());
            Assert.AreEqual(2, target.CountAll(c => c.Name.StartsWith("TEST")));
            Assert.AreEqual(2, target.CountAll(c => c.Name.Contains("_")));
            Assert.AreEqual(1, target.CountAll(c => c.Name.EndsWith("_1")));
            Assert.AreEqual(1, target.CountAll(c => c.Name.EndsWith("_2")));
        }

        [Test]
        [Category("Dropbox")]
        public void FindAll_Filter_FiltedResults()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateClient();
            target.ClearAll();
            target.SetUnitOfWork(unitOfWork);

            var data = new SimpleDataStub();
            data.Name = "TEST_1";
            target.Add(data);

            data = new SimpleDataStub();
            data.Name = "TEST_2";
            target.Add(data);

            unitOfWork.Commit();

            Assert.AreEqual(2, target.FindAll().Count());
            Assert.AreEqual(2, target.FindAll(0, 2).Count());
            Assert.AreEqual(1, target.FindAll(0, 1).Count());
            Assert.AreEqual(1, target.FindAll(1, 2).Count());
            Assert.AreEqual(2, target.FindAll(c => c.Name.StartsWith("TEST")).Count());
            Assert.AreEqual(2, target.FindAll(c => c.Name.Contains("_")).Count());
            Assert.AreEqual(1, target.FindAll(c => c.Name.EndsWith("_1")).Count());
            Assert.AreEqual(1, target.FindAll(c => c.Name.EndsWith("_2")).Count());
        }

        [Test]
        [Category("Dropbox")]
        public void Pesists_Item_Persisted()
        {
            var unitOfWork = new MemoryUnitOfWork();
            var target = CreateClient();
            target.ClearAll();
            target.SetUnitOfWork(unitOfWork);

            var data = new SimpleDataStub();
            data.Name = "TEST_1";
            target.Add(data);

            data = new SimpleDataStub();
            data.Name = "TEST_2";
            target.Add(data);

            unitOfWork.Commit();

            var actual = target.FindAll().ToList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual("TEST_1", actual[0].Name);
            Assert.AreEqual("TEST_2", actual[1].Name);

            var actualRecord = actual[1];
            actualRecord.Name += " (UPDATED)";
            target[actualRecord.Id] = actualRecord;
            target.Remove(actual[0]);
            unitOfWork.Commit();

            actual = target.FindAll().ToList();
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("TEST_2 (UPDATED)", actual[0].Name);
        }

        private static DropboxRepositoryBase<SimpleDataStub> CreateClient()
        {
            var apiToken = Environment.GetEnvironmentVariable("DropboxApiToken");
            var target = new DropboxRepositoryBase<SimpleDataStub>(apiToken, "DropboxRepositoryBaseTest");
            return target;
        }
    }
}