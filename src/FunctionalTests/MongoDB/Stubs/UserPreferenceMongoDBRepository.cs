using System;
using Skahal.Infrastructure.Framework.People;
using MongoDB.Bson.Serialization;
using Skahal.Infrastructure.Framework.Domain;
using Skahal.Infrastructure.Repositories.MongoDB;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
	public class UserPreferenceMongoDBRepository : MongoDBRepositoryBase<User>
	{
		public UserPreferenceMongoDBRepository () : base("mongodb://localhost/?safe=true", "UserPreference")
		{

		}
	}
}