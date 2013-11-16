using NUnit.Framework;
using System;
using Skahal.Infrastructure.Framework.Repositories;
using Skahal.Infrastructure.Framework.People;
using System.Linq;
using TestSharp;
using System.IO;
using HelperSharp;
using System.Collections.Generic;
using System.Threading;

namespace Skahal.Infrastructure.Repositories.FunctionalTests
{
	[TestFixture ()]
	public class MongoDBRepositoryBaseTest
	{
		#region Fields
		private UserMongoDBRepository m_target;
		private IUnitOfWork m_unitOfWork;
		#endregion

		#region Tests
		[TestFixtureSetUp]
		public void InitializeFixture()
		{
			ProcessHelper.KillAll ("mongod");
			var rootDir = VSProjectHelper.GetProjectFolderPath ("FunctionalTests");
			var dbPath = Path.Combine (rootDir, "db");
			var logPath = Path.Combine (dbPath, "db.log");
			DirectoryHelper.DeleteIfNotExists (dbPath);
			DirectoryHelper.CreateIfNotExists (dbPath);

			var mongodPath = "/Applications/mongodb/bin/mongod";

			if (!File.Exists(mongodPath)) {
				mongodPath = "/etc/rc.d/init.d/mongod";

				if (!File.Exists(mongodPath)) {
					mongodPath = "mongod";
				}
			}



			ProcessHelper.Run (mongodPath, "--dbpath {0} --logpath {1}".With (dbPath, logPath), false);
		
			FileHelper.WaitForFileContentContains (logPath, "waiting for connections");
		}

		[SetUp]
		public void InitializeTest()
		{
			IEnumerable<User> users;

			while (true) {
				try
				{
					m_target = new UserMongoDBRepository ();
					m_unitOfWork = new MemoryUnitOfWork ();
					m_target.SetUnitOfWork (m_unitOfWork);

					users = m_target.FindAll (f => true);
					break;
				}
				catch {
					Thread.Sleep (1000);
					continue;
				}
			}

			foreach (var u in users) {
				m_target.Remove (u);
			}

			for (long i = 0; i < 10; i++) {
				var user = new User (i.ToString()) { Name = "Test name " + i, RemoteKey = "Remote Key" + (10-(i + 1)) };
				m_target.Add (user);
			}

			m_unitOfWork.Commit ();
		}

		[TestFixtureTearDown]
		public void CleanupFixture()
		{
			ProcessHelper.KillAll ("mongod");
		}

		[Test ()]
		public void Add_NotCommitAndCommit_PersistsAfterCommit ()
		{
			var originalCount = m_target.CountAll ();
			var user = new User (Guid.NewGuid ().ToString ()) { Name = "Test name " + Guid.NewGuid().ToString() };
			m_target.Add (user);
			Assert.AreEqual (originalCount, m_target.CountAll());

			m_unitOfWork.Commit ();

			Assert.AreEqual (originalCount + 1, m_target.CountAll ());
			var searchUser = m_target.FindBy (user.Key);
			Assert.AreEqual (user.Name, searchUser.Name);
		}

		[Test ()]
		public void Remove_NotCommitAndCommit_PersistsAfterCommit ()
		{
			var originalCount = m_target.CountAll ();
			m_target.Remove (m_target.FindBy("2"));
			Assert.AreEqual (originalCount, m_target.CountAll());

			m_unitOfWork.Commit ();

			Assert.AreEqual (originalCount - 1, m_target.CountAll ());
			var searchUser = m_target.FindBy ("2");
			Assert.IsNull (searchUser);
		}

		[Test ()]
		public void CountAll_Args_Count ()
		{
			Assert.IsTrue (m_target.CountAll () == 10);
			Assert.AreEqual(0, m_target.CountAll(u => u.Name.Equals("NOT EXISTS")));
			Assert.IsTrue(m_target.CountAll(u => u.Name.StartsWith("Test name ")) >= 10);
			Assert.AreEqual(1, m_target.CountAll(u => u.Name.Equals("Test name 5")));
		}

		[Test ()]
		public void FindAll_Args_Entities()
		{
			var actual = m_target.FindAll (f => true).ToList ();
			Assert.AreEqual (10, actual.Count);
			Assert.AreEqual ("Test name 5", actual [5].Name);

			actual = m_target.FindAll (f => f.Name.Equals ("Test name 6")).ToList();
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual ("Test name 6", actual [0].Name);

			actual = m_target.FindAll (2, 3).ToList();
			Assert.AreEqual(3, actual.Count);
			Assert.AreEqual ("Test name 2", actual [0].Name);
			Assert.AreEqual ("Test name 3", actual [1].Name);
			Assert.AreEqual ("Test name 4", actual [2].Name);

			actual = m_target.FindAll (0, 3, f => f.Name.Equals ("Test name 3")).ToList();
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual ("Test name 3", actual [0].Name);
		}

		[Test ()]
		public void FindAllAscending_Args_Entities()
		{
			var actual = m_target.FindAllAscending (f => f.RemoteKey).ToList ();
			Assert.AreEqual (10, actual.Count);
			Assert.AreEqual ("Test name 9", actual [0].Name);
			Assert.AreEqual ("Test name 0", actual [9].Name);

			actual = m_target.FindAllAscending (f => f.Name.Equals ("Test name 6"), f => f.RemoteKey).ToList();
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual ("Test name 6", actual [0].Name);

			actual = m_target.FindAllAscending (2, 3, f => f.RemoteKey).ToList();
			Assert.AreEqual(3, actual.Count);
			Assert.AreEqual ("Test name 7", actual [0].Name);
			Assert.AreEqual ("Test name 6", actual [1].Name);
			Assert.AreEqual ("Test name 5", actual [2].Name);
		}

		[Test ()]
		public void FindAllDescending_Args_Entities()
		{
			var actual = m_target.FindAllDescending (f => f.RemoteKey).ToList ();
			Assert.AreEqual (10, actual.Count);
			Assert.AreEqual ("Test name 0", actual [0].Name);
			Assert.AreEqual ("Test name 9", actual [9].Name);

			actual = m_target.FindAllDescending (f => f.Name.Equals ("Test name 6"), f => f.RemoteKey).ToList();
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual ("Test name 6", actual [0].Name);

			actual = m_target.FindAllDescending (2, 3, f => f.RemoteKey).ToList();
			Assert.AreEqual(3, actual.Count);
			Assert.AreEqual ("Test name 2", actual [0].Name);
			Assert.AreEqual ("Test name 3", actual [1].Name);
			Assert.AreEqual ("Test name 4", actual [2].Name);
		}

		[Test ()]
		public void FindBy_Key_Entity()
		{
			var actual = m_target.FindBy (100);
			Assert.IsNull (actual);

			actual = m_target.FindBy ("2");
			Assert.AreEqual ("2", actual.Key);
			Assert.AreEqual ("Test name 2", actual.Name);

			actual = m_target.FindBy ("9");
			Assert.AreEqual ("9", actual.Key);
			Assert.AreEqual ("Test name 9", actual.Name);
		}
		#endregion
	}
}

