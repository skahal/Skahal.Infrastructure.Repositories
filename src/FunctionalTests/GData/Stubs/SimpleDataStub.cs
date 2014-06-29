using System;
using Skahal.Infrastructure.Framework.Domain;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
    public class SimpleDataStub : EntityWithIdBase<string>, IAggregateRoot
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Size { get; set; }
        public bool Enabled { get; set; }
    }
}
 