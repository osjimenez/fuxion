using Fuxion.Synchronization;
using Fuxion.Test.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Synchronization
{
	public class SynchronizationTest : BaseTest
	{
		public SynchronizationTest(ITestOutputHelper output) : base(output) => this.output = output;

		private readonly ITestOutputHelper output;
		[Fact(DisplayName = "Synchronization - PostPreviewAction")]
		public async Task PostPreviewAction()
		{
			var fuxionRepo = new RepoFuxion();
			var crmRepo = new RepoCRM();
			var postPreviewActionExecuted = false;

			ISide GetFuxionSide(bool isMaster) => new Side<RepoFuxion, UserFuxion>
			{
				IsMaster = isMaster,
				Name = "FUXION",
				Source = fuxionRepo,
				PluralItemTypeName = Strings.Users,
				SingularItemTypeName = Strings.User,
				ItemTypeIsMale = true,

				OnLoad = s => s.Get().ToList(),
				OnNaming = i => i.Name,
				OnTagging = i => i.Id.ToString(),
				OnInsert = (s, i) => s.Add(i),
				OnDelete = (s, i) => s.Delete(i),
				OnUpdate = (s, i) => { }
			};
			ISide GetCRMSide(bool isMaster) => new Side<RepoCRM, UserCRM>
			{
				IsMaster = isMaster,
				Name = "CRM",
				Source = crmRepo,
				OnLoad = s => s.Get().ToList(),
				PluralItemTypeName = Strings.Users,
				SingularItemTypeName = Strings.User,
				ItemTypeIsMale = true,
				OnNaming = i => i.Name,
				OnTagging = i => i.Id.ToString(),
				OnInsert = (s, i) => s.Add(i),
				OnDelete = (s, i) => s.Delete(i),
				OnUpdate = (s, i) => { }
			};
			IComparator GetFuxionCRMComparator() => new Comparator<UserFuxion, UserCRM, int>
			(
				onSelectKeyA: u => u.Id,
				onSelectKeyB: u => u.Id)
			{
				OnMapAToB = (a, b) =>
				{
					if (b == null)
						return new UserCRM(a.Id, a.Name, a.Age);
					b.Name = a.Name;
					b.Age = a.Age;
					return b;
				},
				OnMapBToA = (b, a) =>
				{
					if (a == null)
						return new UserFuxion(b.Id, b.Name, b.Age);
					a.Name = b.Name;
					a.Age = b.Age;
					return a;
				},
				PropertiesComparator = PropertiesComparator<UserFuxion, UserCRM>
					.WithoutAutoDiscoverProperties()
					.With(a => a.Name, b => b.Name)
					.With(a => a.Age, b => b.Age, v => v.aValue == v.bValue)
			};

			var work1 = new Work
			(
				name: "Users",
				sides: new[] { GetFuxionSide(true), GetCRMSide(false) },
				comparators: new[] { GetFuxionCRMComparator() }
			);
			var work2 = new Work
			(
				name: "Users",
				sides: new[] { GetFuxionSide(false), GetCRMSide(true) },
				comparators: new[] { GetFuxionCRMComparator() }
			);
			work2.PostPreviewAction = p =>
			{
				var w1 = p.Works.First(w => w.Id == work1.Id);
				var w2 = p.Works.First(w => w.Id == work2.Id);
				var itemsToDeleteInCRM = w1.Items.Where(i => i.Sides.First().Action == SynchronizationAction.Delete).ToList();
				var itemsToInsertInFuxion = w2.Items.Where(i => i.Sides.First().Action == SynchronizationAction.Insert).ToList();
				foreach (var item in itemsToDeleteInCRM)
					if (itemsToInsertInFuxion.Any(i => i.MasterItemTag == item.Sides.First().SideItemTag))
					{
						w1.Items.Remove(item);
						output.WriteLine("POST PREVIEW ACTION EXECUTION");
					}
				postPreviewActionExecuted = true;
			};

			var ses = new Session("Test")
			{
				Works = new[] { work1, work2 }
			};

			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);

			Assert.True(postPreviewActionExecuted, "Post preview action wasn't executed");
		}
		[Fact(DisplayName = "Synchronization - PostRunAction")]
		public async Task PostRunAction()
		{
			var fuxionRepo = new RepoFuxion();
			var crmRepo = new RepoCRM();
			var postRunActionExecuted = false;

			ISide GetFuxionSide(bool isMaster) => new Side<RepoFuxion, UserFuxion>
			{
				IsMaster = isMaster,
				Name = "FUXION",
				Source = fuxionRepo,
				PluralItemTypeName = Strings.Users,
				SingularItemTypeName = Strings.User,
				ItemTypeIsMale = true,

				OnLoad = s => s.Get().ToList(),
				OnNaming = i => i.Name,
				OnTagging = i => i.Id.ToString(),
				OnInsert = (s, i) => s.Add(i),
				OnDelete = (s, i) => s.Delete(i),
				OnUpdate = (s, i) => { }
			};
			ISide GetCRMSide(bool isMaster) => new Side<RepoCRM, UserCRM>
			{
				IsMaster = isMaster,
				Name = "CRM",
				Source = crmRepo,
				OnLoad = s => s.Get().ToList(),
				PluralItemTypeName = Strings.Users,
				SingularItemTypeName = Strings.User,
				ItemTypeIsMale = true,
				OnNaming = i => i.Name,
				OnTagging = i => i.Id.ToString(),
				OnInsert = (s, i) => s.Add(i),
				OnDelete = (s, i) => s.Delete(i),
				OnUpdate = (s, i) => { }
			};
			IComparator GetFuxionCRMComparator() => new Comparator<UserFuxion, UserCRM, int>
			(
				onSelectKeyA: u => u.Id,
				onSelectKeyB: u => u.Id)
			{
				OnMapAToB = (a, b) =>
				{
					if (b == null)
						return new UserCRM(a.Id, a.Name, a.Age);
					b.Name = a.Name;
					b.Age = a.Age;
					return b;
				},
				OnMapBToA = (b, a) =>
				{
					if (a == null)
						return new UserFuxion(b.Id, b.Name, b.Age);
					a.Name = b.Name;
					a.Age = b.Age;
					return a;
				},
				PropertiesComparator = PropertiesComparator<UserFuxion, UserCRM>
					.WithoutAutoDiscoverProperties()
					.With(a => a.Name, b => b.Name)
					.With(a => a.Age, b => b.Age, v => v.aValue == v.bValue)
			};

			var work1 = new Work
			(
				name: "Users",
				sides: new[] { GetFuxionSide(true), GetCRMSide(false) },
				comparators: new[] { GetFuxionCRMComparator() }
			);
			var work2 = new Work
			(
				name: "Users",
				sides: new[] { GetFuxionSide(false), GetCRMSide(true) },
				comparators: new[] { GetFuxionCRMComparator() }
			)
			{
				PostRunAction = p =>
				{
					output.WriteLine("POST RUN ACTION EXECUTION");
					postRunActionExecuted = true;
				}
			};

			var ses = new Session("Test")
			{
				Works = new[] { work1, work2 }
			};

			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);

			Assert.True(postRunActionExecuted, "Post run action wasn't executed");
		}
		[Fact(DisplayName = "Synchronization - Demo")]
		public void Demo()
		{
			var es = new CultureInfo("ES-es");
			Thread.CurrentThread.CurrentCulture = es;
			Thread.CurrentThread.CurrentUICulture = es;

			var fuxionRepo = new RepoFuxion();
			var crmRepo = new RepoCRM();
			var erpRepo = new RepoERP();
			// Create sync session
			var ses = new Session("Test")
			{
				Works = new[]
				{
					new Work
					(
						name: "Users",
						#region Sides
						sides: new ISide[] {
							new Side<RepoFuxion, UserFuxion>
							{
								IsMaster = true,
								Name = "FUXION",
								Source = fuxionRepo,
								PluralItemTypeName = Strings.Users,
								SingularItemTypeName = Strings.User,
								ItemTypeIsMale = true,

								OnLoad = s => s.Get().ToList(),
								OnNaming = i => i.Name,
								OnInsert = (s, i) => s.Add(i),
								OnDelete = (s, i) => s.Delete(i),
								OnUpdate = (s, i) => { }
							},
							new Side<UserFuxion, SkillFuxion> {
								Name = "%sourceName%'s skills",
								Source = null!,
								OnLoad = s => s.Skills,
								PluralItemTypeName = Strings.Skills,
								SingularItemTypeName = Strings.Skill,
								ItemTypeIsMale = false,

								OnNaming = i => i.Name,
								OnInsert = (s, i) => s.Skills.Add(i),
								OnDelete = (s, i) => s.Skills.Remove(i),
								OnUpdate = (s, i) => { }
							},
							new Side<SkillFuxion, CharacteristicFuxion> {
								Name = "%sourceName%'s characteristics",
								Source = null!,
								OnLoad = s => s.Properties,
								PluralItemTypeName = Strings.Characteristic,
								SingularItemTypeName = Strings.Characteristics,
								ItemTypeIsMale = false,
								OnNaming = i => i.Name,
								OnInsert = (s, i) => s.Properties.Add(i),
								OnDelete = (s, i) => s.Properties.Remove(i),
								OnUpdate = (s, i) => { }
							},
							new Side<RepoCRM, UserCRM>
							{
                                //IsMaster = true,
                                Name = "CRM",
								Source = crmRepo,
								OnLoad = s => s.Get().ToList(),
								PluralItemTypeName = Strings.Users,
								SingularItemTypeName = Strings.User,
								ItemTypeIsMale = true,
								OnNaming = i => i.Name,
								OnInsert = (s, i) => s.Add(i),
								OnDelete = (s, i) => s.Delete(i),
								OnUpdate = (s, i) => { }
							},
							new Side<UserCRM, SkillCRM> {
								Name = "%sourceName%'s skills",
								Source = null!,
								OnLoad = s => s.Skills,
								PluralItemTypeName = Strings.Skills,
								SingularItemTypeName = Strings.Skill,
								ItemTypeIsMale = true,
								OnNaming = i => i.Name,
								OnInsert = (s, i) => {
									if (s.Skills == null)s.Skills=new List<SkillCRM>();
									s.Skills.Add(i);
								},
								OnDelete = (s, i) => s.Skills.Remove(i),
								OnUpdate = (s, i) => { }
							},
							new Side<SkillCRM, CharacteristicCRM> {
								Name = "%sourceName%'s properties",
								Source = null!,
								OnLoad = s => s.Properties,
								PluralItemTypeName = Strings.Characteristics,
								SingularItemTypeName = Strings.Characteristic,
								ItemTypeIsMale = false,
								OnNaming = i => i.Name,
								OnInsert = (s, i) => {
									if (s.Properties == null) s.Properties = new  List<CharacteristicCRM>();
									s.Properties.Add(i);
								},
								OnDelete = (s, i) => s.Properties.Remove(i),
								OnUpdate = (s, i) => { }
							},
							new Side<RepoERP, UserERP>
							{
                                //IsMaster = true,
                                Name = "ERP",
								Source = erpRepo,
								OnLoad = s => s.Get().ToList(),
								PluralItemTypeName = Strings.Users,
								SingularItemTypeName = Strings.User,
								ItemTypeIsMale = true,
								OnNaming = i => i.Name,
								OnInsert = (s, i) => s.Add(i),
								OnDelete = (s, i) => s.Delete(i),
								OnUpdate = (s, i) => { }
							},
						},
                        #endregion
                        #region Comparators
                        comparators: new IComparator[] {
							new Comparator<UserFuxion, UserCRM, int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id)
							{
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new UserCRM(a.Id, a.Name, a.Age);
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new UserFuxion(b.Id, b.Name, b.Age);
									a.Name = b.Name;
									a.Age = b.Age;
									return a;
								},
								PropertiesComparator = PropertiesComparator<UserFuxion, UserCRM>
									.WithoutAutoDiscoverProperties()
									.With(a => a.Name, b => b.Name)
									.With(a => a.Age, b => b.Age, v => v.aValue == v.bValue)
							},
							new Comparator<UserERP, UserFuxion, int>
							(
								onSelectKeyA: u=>u.Id,
								onSelectKeyB: u=>u.Id){
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new UserFuxion(a.Id, a.Name, a.Age);
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new UserERP(b.Id, b.Name, b.Age);
									a.Name = b.Name;
									a.Age = b.Age;
									return a;
								},
							},
							new Comparator<SkillERP, SkillFuxion, int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new SkillFuxion(a.Id, a.Name);
									b.Name = a.Name;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new SkillERP(b.Id, b.Name);
									a.Name = b.Name;
									return a;
								},
							},
							new Comparator<UserCRM, UserERP, int>
							(
								onSelectKeyA: u=>u.Id,
								onSelectKeyB: u=>u.Id){
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new UserERP(a.Id, a.Name, a.Age);
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new UserCRM (b.Id, b.Name, b.Age);
									a.Name = b.Name;
									a.Age = b.Age;
									return a;
								},
							},
							new Comparator<SkillCRM, SkillFuxion, int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new SkillFuxion(a.Id, a.Name);
									b.Name = a.Name;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new SkillCRM(b.Id, b.Name);
									a.Name = b.Name;
									return a;
								},

							},
							new Comparator<CharacteristicCRM, CharacteristicFuxion, int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a,b) =>
								{
									if(b == null)
										return new CharacteristicFuxion(a.Id, a.Name);
									b.Name = a.Name;
									return b;
								},
								OnMapBToA = (b,a) =>
								{
									if(a == null)
										return new CharacteristicCRM(b.Id, b.Name);
									a.Name = b.Name;
									return a;
								},
							},
						}
                        #endregion
                    )
				}
			};

			var man = new SynchronizationManager();

			// Preview synchronization
			var res = man.PreviewAsync(ses).Result;

			// Serialize
			var ser = new DataContractSerializer(typeof(SessionPreview));
			var str = new MemoryStream();
			ser.WriteObject(str, res);
			str.Position = 0;

			// Print as JSON and XML
			//Printer.WriteLine("JSON:\r\n" + res.ToJson());

			var doc = new XmlDocument();
			doc.Load(str);
			var str2 = new MemoryStream();
			var writer = new XmlTextWriter(str2, Encoding.Default)
			{
				Formatting = Formatting.Indented
			};
			doc.WriteContentTo(writer);
			str2.Position = 0;
			var serStr = new StreamReader(str2).ReadToEnd();
			//Printer.WriteLine("XML:\r\n" + serStr);
			str.Position = 0;

			// Deserialize
			var res2 = (SessionPreview?)ser.ReadObject(str);
			if (res2 is null) throw new InvalidOperationException($"'{nameof(SessionPreview)}' is null");
			Printer.WriteLineAction = m => output.WriteLine(m);
			res2.ResourceManager = Strings.ResourceManager;
			//res2.Print();
			foreach (var work in res2.Works)
			{
				Printer.Foreach($"Work '{work.Name}' ({work.ChangesMessage})", work.Items, item =>
				{
					Printer.Foreach($"Item '{item.MasterItemName}' ({item.ChangesMessage})", item.Sides, side =>
					{
						//Printer.WriteLine($"Side '{side.SideName}' ({side.ChangesMessage}) = {side.StatusMessage}");

						var act = new Action<ItemRelationPreview>(relation =>
						{
							using (Printer.Indent2())
							{
								foreach (var pro in relation.Properties)
									Printer.WriteLine($"Property '{pro.PropertyName}' = {pro.StatusMessage}");
								foreach (var rel in relation.Relations)
									Printer.WriteLine($"Relation '{rel.MasterItemName}' ({rel.ChangesMessage}) = {rel.StatusMessage}");
							}
						});

						using (Printer.Indent2($"Side '{side.SideName}' ({side.ChangesMessage}) = {side.StatusMessage}"))
						{
							foreach (var pro in side.Properties)
								Printer.WriteLine($"Property '{pro.PropertyName}' = {pro.StatusMessage}");
							foreach (var rel in side.Relations)
							{
								Printer.WriteLine($"Relation '{rel.MasterItemName}' ({rel.ChangesMessage}) = {rel.StatusMessage}");
								act(rel);
							}
						}
					});
				});
			}
			Printer.WriteLineAction = m => { };

			// Run Sync
			man.RunAsync(res2).Wait();

			Assert.True(crmRepo.Get().Single(u => u.Id == 1).Name == "Tom", "User 'Tom' isn't updated in CRM side");
			Assert.True(erpRepo.Get().Single(u => u.Id == 2).Name == "Clark", "User 'Clark' isn't updated in ERP side");

			// Check results
			//Assert.Equal(6, res.First().Count());
			//Assert.Equal(3, res.First().First().Count());
			//Assert.Equal(2, res.First().Skip(1).First().Count());

			Printer.WriteLine("TEST FINISHED");
		}
		[Fact(DisplayName = "Synchronization - Insert at level 0")]
		public async Task InsertAtLevel0()
		{
			UserCRM? inserted = null;
			var ses = new Session("TEST - Insert at Level 0")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = Enumerable.Empty<UserCRM>().ToArray(),
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
								OnInsert = (uu, u) => inserted = u,
							}
						},
						comparators: new[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							}
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(inserted);
			Assert.Equal(1, inserted?.Id);
			Assert.Equal("Oscar", inserted?.Name);
			Assert.Equal(18, inserted?.Age);
		}
		[Fact(DisplayName = "Synchronization - Update at level 0")]
		public async Task UpdateAtLevel0()
		{
			UserCRM? updated = null;
			var ses = new Session("TEST - Update at Level 0")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new []
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar (old)",
										age: 17
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
								OnUpdate = (uu, u) => updated = u,
							}
						},
						comparators: new[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							}
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(updated);
			Assert.Equal(1, updated?.Id);
			Assert.Equal("Oscar", updated?.Name);
			Assert.Equal(18, updated?.Age);
		}
		[Fact(DisplayName = "Synchronization - Delete at level 0")]
		public async Task DeleteAtLevel0()
		{
			UserCRM? deleted = null;
			var ses = new Session("TEST - Delete at Level 0")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = Enumerable.Empty<UserFuxion>().ToArray(),
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new []
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
								OnDelete = (uu, u) => deleted = u,
							}
						},
						comparators: new[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							}
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(deleted);
			Assert.Equal(1, deleted?.Id);
			Assert.Equal("Oscar", deleted?.Name);
			Assert.Equal(18, deleted?.Age);
		}

		[Fact(DisplayName = "Synchronization - Insert at level 1")]
		public async Task InsertAtLevel1()
		{
			SkillCRM? inserted = null;
			var ses = new Session("TEST - Insert at Level 1")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillFuxion
											(
												id: 1,
												name: "Driver"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
								OnInsert = (ss, s) => inserted = s
							},
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							},
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id)
							{
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(inserted);
			Assert.Equal(1, inserted?.Id);
			Assert.Equal("Driver", inserted?.Name);
		}
		[Fact(DisplayName = "Synchronization - Update at level 1")]
		public async Task UpdateAtLevel1()
		{
			SkillCRM? updated = null;
			var ses = new Session("TEST - Update at Level 1")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillFuxion
											(
												id: 1,
												name: "Driver"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver (old)"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
								OnUpdate = (ss, s) => updated = s
							},
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							},
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(updated);
			Assert.Equal(1, updated?.Id);
			Assert.Equal("Driver", updated?.Name);
		}
		[Fact(DisplayName = "Synchronization - Delete at level 1")]
		public async Task DeleteAtLevel1()
		{
			SkillCRM? deleted = null;
			var ses = new Session("TEST - Delete at Level 1")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
								OnDelete = (ss, s) => deleted = s
							},
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
                            },
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(deleted);
			Assert.Equal(1, deleted?.Id);
			Assert.Equal("Driver", deleted?.Name);
		}

		[Fact(DisplayName = "Synchronization - Insert at level 2")]
		public async Task InsertAtLevel2()
		{
			CharacteristicCRM? inserted = null;
			var ses = new Session("TEST - Insert at Level 2")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillFuxion
											(
												id: 1,
												name: "Driver",
												properties: new[]
												{
													new CharacteristicFuxion
													(
														id: 1,
														name: "Experience"
													)
												}
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
							},
							new Side<SkillFuxion,CharacteristicFuxion>
							{
								Name = "FUXION-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties
							},
							new Side<SkillCRM,CharacteristicCRM>
							{
								Name = "CRM-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties,
								OnInsert = (pp, p) => inserted = p
							},
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
                            },
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id)
                            ,
							new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new CharacteristicCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(inserted);
			Assert.Equal(1, inserted?.Id);
			Assert.Equal("Experience", inserted?.Name);
		}
		[Fact(DisplayName = "Synchronization - Update at level 2")]
		public async Task UpdateAtLevel2()
		{
			CharacteristicCRM? updated = null;
			var ses = new Session("TEST - Update at Level 2")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillFuxion
											(
												id: 1,
												name: "Driver",
												properties: new[]
												{
													new CharacteristicFuxion
													(
														id: 1,
														name: "Experience"
													)
												}
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver",
												properties: new[]
												{
													new CharacteristicCRM
													(
														id: 1,
														name: "Experience (old)"
													)
												}
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<SkillFuxion,CharacteristicFuxion>
							{
								Name = "FUXION-SKILL-PROPERTY",
								OnNaming = s => s.Name,
								OnLoad = u => u.Properties
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
							},
							new Side<SkillCRM, CharacteristicCRM>
							{
								Name = "CRM-SKILL-PROPERTY",
								OnNaming = s => s.Name,
								OnLoad = u => u.Properties,
								OnUpdate = (ss, s) => updated = s
							},
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
                            },
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
                            },
							new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new CharacteristicCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
                            },
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(updated);
			Assert.Equal(1, updated?.Id);
			Assert.Equal("Experience", updated?.Name);
		}
		[Fact(DisplayName = "Synchronization - Delete at level 2")]
		public async Task DeleteAtLevel2()
		{
			CharacteristicCRM? deleted = null;
			var ses = new Session("TEST - Delete at Level 2")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillFuxion
											(
												id: 1,
												name: "Driver"
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver",
												properties: new[]
												{
													new CharacteristicCRM
													(
														id: 1,
														name: "Experience"
													)
												}
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
							},
							new Side<SkillFuxion, CharacteristicFuxion>
							{
								Name = "FUXION-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties,

							},
							new Side<SkillCRM, CharacteristicCRM>
							{
								Name = "CRM-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties,
								OnDelete = (pp, p) => deleted = p
							}
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
                            },
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
                            },
							new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new CharacteristicCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);
			Assert.NotNull(deleted);
			Assert.Equal(1, deleted?.Id);
			Assert.Equal("Experience", deleted?.Name);
		}
		[Fact(DisplayName = "Synchronization - Delete at levels 1 & 2")]
		public async Task DeleteAtLevels1and2()
		{
			SkillCRM? deleted1 = null;
			CharacteristicCRM? deleted2 = null;
			var ses = new Session("TEST - Delete at Level 2")
			{
				Works = new[]
				{
					new Work
					(
						name: "TestWork",
						sides: new ISide[]
						{
							new Side<UserFuxion[],UserFuxion>
							{
								Name = "FUXION",
								IsMaster = true,
								Source = new[] {
									new UserFuxion
									(
										id: 1,
										name: "Oscar",
										age: 18
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserCRM[],UserCRM>
							{
								Name = "CRM",
								Source = new[]
								{
									new UserCRM
									(
										id: 1,
										name: "Oscar",
										age: 18,
										skills: new[]
										{
											new SkillCRM
											(
												id: 1,
												name: "Driver",
												properties: new[]
												{
													new CharacteristicCRM
													(
														id: 1,
														name: "Experience"
													)
												}
											)
										}
									)
								},
								OnNaming = u => u.Name,
								OnLoad = uu => uu,
							},
							new Side<UserFuxion,SkillFuxion>
							{
								Name = "FUXION-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills
							},
							new Side<UserCRM,SkillCRM>
							{
								Name = "CRM-SKILL",
								OnNaming = s => s.Name,
								OnLoad = u => u.Skills,
								OnDelete = (pp, p) =>
								{
                                    //if(deleted2 != null) throw new InvalidStateException("Level ");
                                    deleted1 = p;
								}
							},
							new Side<SkillFuxion, CharacteristicFuxion>
							{
								Name = "FUXION-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties,

							},
							new Side<SkillCRM, CharacteristicCRM>
							{
								Name = "CRM-SKILL-PROPERTY",
								OnNaming = p => p.Name,
								OnLoad = s => s.Properties,
								OnDelete = (pp, p) =>
								{
									if(deleted1 != null) throw new InvalidStateException("Level 1 was deleted before level 2");
									deleted2 = p;
								}
							}
						},
						comparators: new IComparator[]
						{
							new Comparator<UserFuxion,UserCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new UserCRM(a.Id, a.Name, a.Age);
									b.Id = a.Id;
									b.Name = a.Name;
									b.Age = a.Age;
									return b;
								},
							},
							new Comparator<SkillFuxion,SkillCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new SkillCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
							new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
							(
								onSelectKeyA: u => u.Id,
								onSelectKeyB: u => u.Id){
								OnMapAToB = (a, b) =>
								{
									if(b == null)
										b = new CharacteristicCRM(a.Id, a.Name);
									b.Id = a.Id;
									b.Name = a.Name;
									return b;
								},
							},
						}
					)
				}
			};
			var man = new SynchronizationManager();
			var pre = await man.PreviewAsync(ses);
			pre.Print();
			await man.RunAsync(pre);

			Assert.NotNull(deleted1);
			Assert.Equal(1, deleted1?.Id);
			Assert.Equal("Driver", deleted1?.Name);

			Assert.NotNull(deleted2);
			Assert.Equal(1, deleted2?.Id);
			Assert.Equal("Experience", deleted2?.Name);
		}
	}
	public class Repo<T>
	{
		public Repo(IEnumerable<T> items) => list = new List<T>(items);

		private readonly List<T> list;
		public IEnumerable<T> Get() => list;
		public void Add(T item) => list.Add(item);
		public void Delete(T item) => list.Remove(item);
		public void Update(T item)
		{
			//var index = list.IndexOf(item);
			//if (index != -1) list[index] = item;
			//return index != -1;
		}
	}
	#region Side FUXION
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class UserFuxion
	{
		public UserFuxion(int id, string name, int age, ICollection<SkillFuxion>? skills = null)
		{
			Id = id;
			Name = name;
			Age = age;
			Skills = skills ?? new List<SkillFuxion>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public ICollection<SkillFuxion> Skills { get; set; }
		public override string ToString() => Name;
	}
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class SkillFuxion
	{
		public SkillFuxion(int id, string name, ICollection<CharacteristicFuxion>? properties = null)
		{
			Id = id;
			Name = name;
			Properties = properties ?? new List<CharacteristicFuxion>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public ICollection<CharacteristicFuxion> Properties { get; set; }
	}
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class CharacteristicFuxion
	{
		public CharacteristicFuxion(int id, string name)
		{
			Id = id;
			Name = name;
		}
		public int Id { get; set; }
		public string Name { get; set; }
	}
	public class RepoFuxion : Repo<UserFuxion>
	{
		public RepoFuxion() : base(new[] {
				new UserFuxion
				(
					id: 1,
					name: "Tom",
					age: 30,
					skills: new[]
					{
						new SkillFuxion
						(
							id: 1,
							name: "Tom Skill",
							properties: new[]
							{
								new CharacteristicFuxion
								(
									id: 1,
									name: "Tom Property"
								)
							}
						)
					}
				),
				new UserFuxion
				(
					id: 2,
					name: "Clark",
					age: 24
				),
				new UserFuxion
				(
					id: 3,
					name: "Jerry",
					age: 23
				),
				new UserFuxion
				(
					id: 4,
					name: "Bob",
					age: 43,
					skills: new[]
					{
						new SkillFuxion
						(
							id: 1,
							name: "Bob Skill",
							properties: new[]
							{
								new CharacteristicFuxion
								(
									id: 1,
									name: "Bob Property"
								)
							}
						)
					}
				),
				new UserFuxion
				(
					id: 7,
					name: "Jimmy",
					age: 43
				)
			})
		{ }
	}
	#endregion
	#region Side CRM
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class UserCRM
	{
		public UserCRM(int id, string name, int age, ICollection<SkillCRM>? skills = null)
		{
			Id = id;
			Name = name;
			Age = age;
			Skills = skills ?? new List<SkillCRM>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public ICollection<SkillCRM> Skills { get; set; }
		public override string ToString() => Name;
	}
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class SkillCRM
	{
		public SkillCRM(int id, string name, ICollection<CharacteristicCRM>? properties = null)
		{
			Id = id;
			Name = name;
			Properties = properties ?? new List<CharacteristicCRM>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public ICollection<CharacteristicCRM> Properties { get; set; }
	}
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class CharacteristicCRM
	{
		public CharacteristicCRM(int id, string name)
		{
			Id = id;
			Name = name;
		}
		public int Id { get; set; }
		public string Name { get; set; }
	}
	public class RepoCRM : Repo<UserCRM>
	{
		public RepoCRM() : base(new[] {
				new UserCRM
				(
					id: 1,
					name: "Tom (CRM)",
					age: 30,
					skills: new[]
					{
						new SkillCRM
						(
							id: 1,
							name: "Tom Skill (CRM)",
							properties: new[]
							{
								new CharacteristicCRM
								(
									id: 1,
									name: "Tom Property (CRM)"
								)
							}
						)
					}.ToList()
				),
				new UserCRM
				(
					id: 2,
					name: "Clark",
					age: 24
				),
				new UserCRM
				(
					id: 3,
					name: "Jerry",
					age: 23
				),
				new UserCRM
				(
					id: 5,
					name: "Adam",
					age: 46,
					skills: new[]
					{
						new SkillCRM
						(
							id: 1,
							name: "Adam Skill"
						)
					}.ToList()
				)
			})
		{ }
	}
	#endregion
	#region Side Presence
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class UserERP
	{
		public UserERP(int id, string name, int age, ICollection<SkillERP>? skills = null)
		{
			Id = id;
			Name = name;
			Age = age;
			Skills = skills ?? new List<SkillERP>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public ICollection<SkillERP> Skills { get; set; }
		public override string ToString() => Name;
	}
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public class SkillERP
	{
		public SkillERP(int id, string name)
		{
			Id = id;
			Name = name;
		}
		public int Id { get; set; }
		public string Name { get; set; }
	}
	public class RepoERP : Repo<UserERP>
	{
		public RepoERP() : base(new[] {
				new UserERP
				(
					id: 1,
					name: "Tom",
					age: 29
				),
				new UserERP
				(
					id: 2,
					name: "Clark (ERP)",
					age: 24
				),
				new UserERP
				(
					id: 3,
					name: "Jerry",
					age: 23
				),
				new UserERP
				(
					id: 6,
					name: "Scott",
					age: 87
				)
			})
		{ }
	}
	#endregion
}
