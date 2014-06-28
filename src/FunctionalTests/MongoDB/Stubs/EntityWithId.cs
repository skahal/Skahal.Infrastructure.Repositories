using System;
using Skahal.Infrastructure.Framework.Domain;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
	public class EntityWithId : EntityWithIdBase<string>, IAggregateRoot
	{
		public EntityWithId (string id): base(id)
		{
		}

		public string Name { get; set; }
		public string RemoteKey { get; set; }
	}
}

