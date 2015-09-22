using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
            var clientEmail = Environment.GetEnvironmentVariable("GDataClientEmail");
            
            // Divided in two variables because Windows Environment variable max length is 2047.
            var privateKey = Convert.FromBase64String(
                Environment.GetEnvironmentVariable("GDataPrivateKeyBytesPart1") + 
                Environment.GetEnvironmentVariable("GDataPrivateKeyBytesPart2"));

            return new GDataRepositoryBase<SimpleDataStub>("GDataRepositoryBaseTest", clientEmail, privateKey);
        }
    }
}