using System;
using Skahal.Infrastructure.Framework.People;
using MongoDB.Bson.Serialization;
using Skahal.Infrastructure.Framework.Domain;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
	public class EntityWithIdMongoDBRepository : MongoDBRepositoryBase<EntityWithId>
	{
		public EntityWithIdMongoDBRepository () : base("mongodb://localhost/?safe=true", "User")
		{

		}
	}
}