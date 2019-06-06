//using Fuxion.Identity.Test;
//using Fuxion.Identity.Test.Dao;
//using Fuxion.Identity.Test.Mocks;
//using Fuxion.Repositories;
//using System;
//using System.Linq;

//namespace Fuxion.Identity.DatabaseEFTest
//{
//	public static class Scenario
//	{
//		public const string DATABASE = nameof(DATABASE);
//		public const string MEMORY = nameof(MEMORY);
//		public static void Load(string key)
//		{
//			//TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
//			//    .Where(a => a.FullName.StartsWith("Fuxion"))
//			//    .SelectMany(a => a.DefinedTypes).ToArray();
//			Factory.RemoveAllInjectors();
//			if (key == MEMORY)
//			{
//				if (memoryFactory == null)
//				{
//					var con = new Container();
//					con.RegisterInstance<ICurrentUserNameProvider>(new AlwaysRootCurrentUserNameProvider());
//					con.RegisterInstance<IPasswordProvider>(new PasswordProviderMock());
//					var rep = new IdentityMemoryTestRepository();
//					con.RegisterInstance<IKeyValueRepository<string, IIdentity>>(rep);
//					con.RegisterInstance<IIdentityTestRepository>(rep);
//					con.RegisterSingleton<IdentityManager>();

//					var fac = new TypeDiscriminatorFactory();
//					fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
//					con.RegisterInstance(fac);

//					memoryFactory = new SimpleInjectorFactoryInjector(con);
//				}
//				Factory.AddInjector(memoryFactory);
//			}
//			else if (key == DATABASE)
//			{
//				if (databaseFactory == null)
//				{
//					var con = new Container();
//					con.RegisterInstance<ICurrentUserNameProvider>(new AlwaysRootCurrentUserNameProvider());
//					con.RegisterInstance<IPasswordProvider>(new PasswordProviderMock());
//					var rep = new IdentityDatabaseEFTestRepository();
//					rep.Initialize();
//					con.RegisterInstance<IKeyValueRepository<string, IIdentity>>(new MemoryCachedKeyValueRepository<string, IIdentity>(rep));
//					con.RegisterInstance<IIdentityTestRepository>(rep);
//					con.RegisterSingleton<IdentityManager>();

//					var fac = new TypeDiscriminatorFactory();
//					fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
//					con.RegisterInstance(fac);

//					databaseFactory = new SimpleInjectorFactoryInjector(con);
//				}
//				Factory.AddInjector(databaseFactory);
//			}
//			else throw new NotImplementedException($"El escenario '{key}' no esta soportado");
//		}

//		private static SimpleInjectorFactoryInjector memoryFactory;
//		private static SimpleInjectorFactoryInjector databaseFactory;
//	}
//}
