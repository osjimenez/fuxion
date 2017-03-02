using Fuxion.Synchronization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Xml;
using System.Xml.Linq;
using Fuxion.Test.Resources;
using System.Threading;
using System.Globalization;
using System.Linq.Expressions;
using Fuxion.Collections.Generic;

namespace Fuxion.Test
{
    public class SynchronizationTest : BaseTest
    {
        public SynchronizationTest(ITestOutputHelper output) : base(output) {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact(DisplayName = "Synchronization - Demo")]
        public void Demo()
        {
            var es = new CultureInfo("ES-es");
            Thread.CurrentThread.CurrentCulture = es;
            Thread.CurrentThread.CurrentUICulture = es;

            //Printer.WriteLineAction = m => { };
            var fuxionRepo = new RepoFuxion();
            var crmRepo = new RepoCRM();
            var erpRepo = new RepoERP();
            // Create sync session
            var ses = new Session
            {
                Name = "Test",
                Works = new[]
                {
                    new Work
                    {
                        Name = "Users",
                        #region Sides
                        Sides = new ISide[] {
                            new Side<RepoFuxion, UserFuxion, int>
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
                            new Side<UserFuxion, SkillFuxion, int> {
                                Name = "%sourceName%'s skills",
                                Source = null,
                                OnLoad = s => s.Skills,
                                PluralItemTypeName = Strings.Skills,
                                SingularItemTypeName = Strings.Skill,
                                ItemTypeIsMale = false,

                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Skills.Add(i),
                                OnDelete = (s, i) => s.Skills.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new Side<SkillFuxion, CharacteristicFuxion, int> {
                                Name = "%sourceName%'s characteristics",
                                Source = null,
                                OnLoad = s => s.Properties,
                                PluralItemTypeName = Strings.Characteristic,
                                SingularItemTypeName = Strings.Characteristics,
                                ItemTypeIsMale = false,
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Properties.Add(i),
                                OnDelete = (s, i) => s.Properties.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new Side<RepoCRM, UserCRM, int>
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
                            new Side<UserCRM, SkillCRM, int> {
                                Name = "%sourceName%'s skills",
                                Source = null,
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
                            new Side<SkillCRM, CharacteristicCRM, int> {
                                Name = "%sourceName%'s properties",
                                Source = null,
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
                            new Side<RepoERP, UserERP, int>
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
                        Comparators = new IComparator[] {
                            new Comparator<UserFuxion, UserCRM, int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserCRM { Id = a.Id, Name = a.Name, Age = a.Age };
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserFuxion { Id = b.Id, Name = b.Name, Age = b.Age };
                                    a.Name = b.Name;
                                    a.Age = b.Age;
                                    return a;
                                },
                                PropertiesComparator = new PropertiesComparator<UserFuxion, UserCRM>(false)
                                {
                                    {a => a.Name, b => b.Name },
                                    {a => a.Age, b => b.Age, (a,aVal,b,bVal)=> aVal != bVal },
                                    //{a => a.Age, b => b.Age, new GenericEqualityComparer<int>(null,null) },
                                    //{a => a.Name, b => b.Age, (name, age) => name == age.ToString() },
                                }
                            },
                            new Comparator<UserERP, UserFuxion, int>
                            {
                                OnSelectKeyA = u=>u.Id,
                                OnSelectKeyB = u=>u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserFuxion { Id = a.Id, Name = a.Name, Age = a.Age };
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserERP { Id = b.Id, Name = b.Name, Age = b.Age };
                                    a.Name = b.Name;
                                    a.Age = b.Age;
                                    return a;
                                },
                                //PropertiesComparator = new PropertiesComparator<UserERP, UserFuxion>
                                //{
                                //    {a => a.Name, b => b.Name },
                                //    {a => a.Age, b => b.Age },
                                //}
                            },
                            new Comparator<SkillERP, SkillFuxion, int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new SkillFuxion { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new SkillERP { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                //PropertiesComparator = new PropertiesComparator<SkillERP, SkillFuxion>
                                //{
                                //    {a => a.Name, b => b.Name },
                                //}
                            },
                            new Comparator<UserCRM, UserERP, int>
                            {
                                OnSelectKeyA = u=>u.Id,
                                OnSelectKeyB = u=>u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserERP { Id = a.Id, Name = a.Name, Age = a.Age };
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserCRM { Id = b.Id, Name = b.Name, Age = b.Age };
                                    a.Name = b.Name;
                                    a.Age = b.Age;
                                    return a;
                                },
                                //PropertiesComparator = new PropertiesComparator<UserCRM, UserERP>
                                //{
                                //    {a => a.Name, b => b.Name },
                                //    {a => a.Age, b => b.Age },
                                //}
                            },
                            new Comparator<SkillCRM, SkillFuxion, int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new SkillFuxion { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new SkillCRM { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                //PropertiesComparator = new PropertiesComparator<SkillCRM, SkillFuxion>
                                //{
                                //    {a => a.Name, b => b.Name },
                                //}

                            },
                            new Comparator<CharacteristicCRM, CharacteristicFuxion, int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new CharacteristicFuxion { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new CharacteristicCRM { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                //PropertiesComparator = new PropertiesComparator<CharacteristicCRM, CharacteristicFuxion>
                                //{
                                //    {a => a.Name, b => b.Name },
                                //}
                            },
                        },
                        #endregion
                    }
                }
            };

            var man = new SynchronizationManager();
            // Preview synchronization
            var res = man.PreviewAsync(ses).Result;

            //res.Print();

            // Serialize
            DataContractSerializer ser = new DataContractSerializer(typeof(SessionPreview));
            var str = new MemoryStream();
            ser.WriteObject(str, res);
            str.Position = 0;

            // Print as JSON and XML
            //Printer.WriteLine("JSON:\r\n" + res.ToJson());

            XmlDocument doc = new XmlDocument();
            doc.Load(str);
            var str2 = new MemoryStream();
            var writer = new XmlTextWriter(str2, Encoding.Default);
            writer.Formatting = Formatting.Indented;
            doc.WriteContentTo(writer);
            str2.Position = 0;
            var serStr = new StreamReader(str2).ReadToEnd();
            //Printer.WriteLine("XML:\r\n" + serStr);
            str.Position = 0;

            // Deserialize
            var res2 = (SessionPreview)ser.ReadObject(str);
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
                            Printer.Indent(() =>
                            {
                                foreach (var pro in relation.Properties)
                                    Printer.WriteLine($"Property '{pro.PropertyName}' = {pro.StatusMessage}");
                                foreach (var rel in relation.Relations)
                                    Printer.WriteLine($"Relation '{rel.MasterItemName}' ({rel.ChangesMessage}) = {rel.StatusMessage}");
                            });
                        });

                        Printer.Indent($"Side '{side.SideName}' ({side.ChangesMessage}) = {side.StatusMessage}",() =>
                        {
                            foreach (var pro in side.Properties)
                                Printer.WriteLine($"Property '{pro.PropertyName}' = {pro.StatusMessage}");
                            foreach (var rel in side.Relations)
                            {
                                Printer.WriteLine($"Relation '{rel.MasterItemName}' ({rel.ChangesMessage}) = {rel.StatusMessage}");
                                act(rel);
                            }
                        });
                        //Printer.Indent($"Side {side.SideName} ({side.ChangesMessage}) = {side.StatusMessage}", () => {
                        //    Printer.Foreach($"Property {pro.PropertyName}: {pro.SideValue} => {pro.MasterValue}", side.Properties, pro =>
                        //    {
                        //        Printer.WriteLine($"Property {pro.PropertyName}: {pro.SideValue} => {pro.MasterValue}");
                        //    });
                        //});
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
            UserCRM inserted = null;
            var ses = new Session
            {
                Name = "TEST - Insert at Level 0",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = Enumerable.Empty<UserCRM>().ToArray(),
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                                OnInsert = (uu, u) => inserted = u,
                            }
                        },
                        Comparators = new[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                            }
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(inserted);
            Assert.Equal(1, inserted.Id);
            Assert.Equal("Oscar", inserted.Name);
            Assert.Equal(18, inserted.Age);
        }
        [Fact(DisplayName = "Synchronization - Update at level 0")]
        public async Task UpdateAtLevel0()
        {
            UserCRM updated = null;
            var ses = new Session
            {
                Name = "TEST - Update at Level 0",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new []
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar (old)",
                                        Age = 17
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                                OnUpdate = (uu, u) => updated = u,
                            }
                        },
                        Comparators = new[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, r) =>
                                //{
                                //    if(a.Name != b.Name)
                                //        r.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)
                                //        r.AddProperty(() => a.Age,() => b.Age);
                                //}
                            }
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(updated);
            Assert.Equal(1, updated.Id);
            Assert.Equal("Oscar", updated.Name);
            Assert.Equal(18, updated.Age);
        }
        [Fact(DisplayName = "Synchronization - Delete at level 0")]
        public async Task DeleteAtLevel0()
        {
            UserCRM deleted = null;
            var ses = new Session
            {
                Name = "TEST - Delete at Level 0",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = Enumerable.Empty<UserFuxion>().ToArray(),
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new []
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                                OnDelete = (uu, u) => deleted = u,
                            }
                        },
                        Comparators = new[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, r) =>
                                //{
                                //    if(a.Name != b.Name)
                                //        r.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)
                                //        r.AddProperty(() => a.Age,() => b.Age);
                                //}
                            }
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(deleted);
            Assert.Equal(1, deleted.Id);
            Assert.Equal("Oscar", deleted.Name);
            Assert.Equal(18, deleted.Age);
        }

        [Fact(DisplayName = "Synchronization - Insert at level 1")]
        public async Task InsertAtLevel1()
        {
            SkillCRM inserted = null;
            var ses = new Session
            {
                Name = "TEST - Insert at Level 1",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillFuxion
                                            {
                                                Id = 1,
                                                Name = "Driver"
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                                OnInsert = (ss, s) => inserted = s
                            },
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new SkillCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(inserted);
            Assert.Equal(1, inserted.Id);
            Assert.Equal("Driver", inserted.Name);
        }
        [Fact(DisplayName = "Synchronization - Update at level 1")]
        public async Task UpdateAtLevel1()
        {
            SkillCRM updated = null;
            var ses = new Session
            {
                Name = "TEST - Update at Level 1",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillFuxion
                                            {
                                                Id = 1,
                                                Name = "Driver"
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillCRM
                                            {
                                                Id = 1,
                                                Name = "Driver (old)"
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                                OnUpdate = (ss, s) => updated = s
                            },
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new SkillCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                                //OnCompare = (a, b, p) =>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //}
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(updated);
            Assert.Equal(1, updated.Id);
            Assert.Equal("Driver", updated.Name);
        }
        [Fact(DisplayName = "Synchronization - Delete at level 1")]
        public async Task DeleteAtLevel1()
        {
            SkillCRM deleted = null;
            var ses = new Session
            {
                Name = "TEST - Delete at Level 1",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillCRM
                                            {
                                                Id = 1,
                                                Name = "Driver"
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                                OnDelete = (ss, s) => deleted = s
                            },
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new SkillCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(deleted);
            Assert.Equal(1, deleted.Id);
            Assert.Equal("Driver", deleted.Name);
        }

        [Fact(DisplayName = "Synchronization - Insert at level 2")]
        public async Task InsertAtLevel2()
        {
            CharacteristicCRM inserted = null;
            var ses = new Session
            {
                Name = "TEST - Insert at Level 2",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillFuxion
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                                Properties = new[]
                                                {
                                                    new CharacteristicFuxion
                                                    {
                                                        Id = 1,
                                                        Name = "Experience"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillCRM
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                            },
                            new Side<SkillFuxion,CharacteristicFuxion,int>
                            {
                                Name = "FUXION-SKILL-PROPERTY",
                                OnNaming = p => p.Name,
                                OnLoad = s => s.Properties
                            },
                            new Side<SkillCRM,CharacteristicCRM,int>
                            {
                                Name = "CRM-SKILL-PROPERTY",
                                OnNaming = p => p.Name,
                                OnLoad = s => s.Properties,
                                OnInsert = (pp, p) => inserted = p
                            },
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                //OnCompare = (a, b, p) =>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //}
                            },
                            new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new CharacteristicCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(inserted);
            Assert.Equal(1, inserted.Id);
            Assert.Equal("Experience", inserted.Name);
        }
        [Fact(DisplayName = "Synchronization - Update at level 2")]
        public async Task UpdateAtLevel2()
        {
            CharacteristicCRM updated = null;
            var ses = new Session
            {
                Name = "TEST - Update at Level 2",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillFuxion
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                                Properties = new[]
                                                {
                                                    new CharacteristicFuxion
                                                    {
                                                        Id = 1,
                                                        Name = "Experience"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillCRM
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                                Properties = new[]
                                                {
                                                    new CharacteristicCRM
                                                    {
                                                        Id = 1,
                                                        Name = "Experience (old)"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<SkillFuxion,CharacteristicFuxion,int>
                            {
                                Name = "FUXION-SKILL-PROPERTY",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Properties
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                            },
                            new Side<SkillCRM, CharacteristicCRM,int>
                            {
                                Name = "CRM-SKILL-PROPERTY",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Properties,
                                OnUpdate = (ss, s) => updated = s
                            },
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new SkillCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                                //OnCompare = (a, b, p) =>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //}
                            },
                            new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new CharacteristicCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                                //OnCompare = (a, b, p) =>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //}
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(updated);
            Assert.Equal(1, updated.Id);
            Assert.Equal("Experience", updated.Name);
        }
        [Fact(DisplayName = "Synchronization - Delete at level 2")]
        public async Task DeleteAtLevel2()
        {
            CharacteristicCRM deleted = null;
            var ses = new Session
            {
                Name = "TEST - Delete at Level 2",
                Works = new[]
                {
                    new Work
                    {
                        Name = "TestWork",
                        Sides = new ISide[]
                        {
                            new Side<UserFuxion[],UserFuxion,int>
                            {
                                Name = "FUXION",
                                IsMaster = true,
                                Source = new[] {
                                    new UserFuxion
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillFuxion
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserCRM[],UserCRM,int>
                            {
                                Name = "CRM",
                                Source = new[]
                                {
                                    new UserCRM
                                    {
                                        Id = 1,
                                        Name = "Oscar",
                                        Age = 18,
                                        Skills = new[]
                                        {
                                            new SkillCRM
                                            {
                                                Id = 1,
                                                Name = "Driver",
                                                Properties = new[]
                                                {
                                                    new CharacteristicCRM
                                                    {
                                                        Id = 1,
                                                        Name = "Experience"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                OnNaming = u => u.Name,
                                OnLoad = uu => uu,
                            },
                            new Side<UserFuxion,SkillFuxion,int>
                            {
                                Name = "FUXION-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills
                            },
                            new Side<UserCRM,SkillCRM,int>
                            {
                                Name = "CRM-SKILL",
                                OnNaming = s => s.Name,
                                OnLoad = u => u.Skills,
                            },
                            new Side<SkillFuxion, CharacteristicFuxion,int>
                            {
                                Name = "FUXION-SKILL-PROPERTY",
                                OnNaming = p => p.Name,
                                OnLoad = s => s.Properties,
                                
                            },
                            new Side<SkillCRM, CharacteristicCRM,int>
                            {
                                Name = "CRM-SKILL-PROPERTY",
                                OnNaming = p => p.Name,
                                OnLoad = s => s.Properties,
                                OnDelete = (pp, p) => deleted = p
                            }
                        },
                        Comparators = new IComparator[]
                        {
                            new Comparator<UserFuxion,UserCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new UserCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    b.Age = a.Age;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //    if(a.Age != b.Age)p.AddProperty(() => a.Age,() => b.Age);
                                //}
                            },
                            new Comparator<SkillFuxion,SkillCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new SkillCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                                //OnCompare = (a, b, p)=>
                                //{
                                //    if(a.Name != b.Name) p.AddProperty(() => a.Name,() => b.Name);
                                //}
                            },
                            new Comparator<CharacteristicFuxion,CharacteristicCRM,int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a, b) =>
                                {
                                    if(b == null)
                                        b = new CharacteristicCRM();
                                    b.Id = a.Id;
                                    b.Name = a.Name;
                                    return b;
                                },
                            },
                        }
                    }
                }
            };
            SynchronizationManager man = new SynchronizationManager();
            var pre = await man.PreviewAsync(ses);
            pre.Print();
            await man.RunAsync(pre);
            Assert.NotNull(deleted);
            Assert.Equal(1, deleted.Id);
            Assert.Equal("Experience", deleted.Name);
        }
    }
    public class Repo<T>
    {
        public Repo(IEnumerable<T> items)
        {
            list = new List<T>(items);
        }
        List<T> list;
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
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public ICollection<SkillFuxion> Skills { get; set; }
        public override string ToString() => Name;
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SkillFuxion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CharacteristicFuxion> Properties { get; set; }
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class CharacteristicFuxion
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoFuxion : Repo<UserFuxion>
    {
        public RepoFuxion() : base(new[] {
                new UserFuxion
                {
                    Id = 1,
                    Name = "Tom",
                    Age = 30,
                    Skills = new[]
                    {
                        new SkillFuxion
                        {
                            Id = 1,
                            Name = "Tom Skill",
                            Properties = new[]
                            {
                                new CharacteristicFuxion
                                {
                                    Id = 1,
                                    Name = "Tom Property"
                                }
                            }
                        }
                    }
                },
                new UserFuxion
                {
                    Id = 2,
                    Name = "Clark",
                    Age = 24,
                },
                new UserFuxion
                {
                    Id = 3,
                    Name = "Jerry",
                    Age = 23,
                },
                new UserFuxion
                {
                    Id = 4,
                    Name = "Bob",
                    Age = 43,
                    Skills = new[]
                    {
                        new SkillFuxion
                        {
                            Id = 1,
                            Name = "Bob Skill",
                            Properties = new[]
                            {
                                new CharacteristicFuxion
                                {
                                    Id = 1,
                                    Name = "Bob Property"
                                }
                            }
                        }
                    }
                },
                new UserFuxion
                {
                    Id = 7,
                    Name = "Jimmy",
                    Age = 43
                }
            })
        { }
    }
    #endregion
    #region Side CRM
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class UserCRM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public ICollection<SkillCRM> Skills { get; set; }
        public override string ToString() => Name;
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SkillCRM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CharacteristicCRM> Properties { get; set; }
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class CharacteristicCRM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoCRM : Repo<UserCRM>
    {
        public RepoCRM() : base(new[] {
                new UserCRM
                {
                    Id = 1,
                    Name = "Tom (CRM)",
                    Age = 30,
                    Skills = new[]
                    {
                        new SkillCRM
                        {
                            Id = 1,
                            Name = "Tom Skill (CRM)",
                            Properties = new[]
                            {
                                new CharacteristicCRM
                                {
                                    Id = 1,
                                    Name = "Tom Property (CRM)"
                                }
                            }
                        }
                    }.ToList()
                },
                new UserCRM
                {
                    Id = 2,
                    Name = "Clark",
                    Age = 24,
                },
                new UserCRM
                {
                    Id = 3,
                    Name = "Jerry",
                    Age = 23,
                },
                new UserCRM
                {
                    Id = 5,
                    Name = "Adam",
                    Age = 46,
                    Skills = new[]
                    {
                        new SkillCRM
                        {
                            Id = 1,
                            Name = "Adam Skill"
                        }
                    }.ToList()
                }
            })
        { }
    }
    #endregion
    #region Side Presence
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class UserERP
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public ICollection<SkillERP> Skills { get; set; }
        public override string ToString() => Name;
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SkillERP
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoERP : Repo<UserERP>
    {
        public RepoERP() : base(new[] {
                new UserERP
                {
                    Id = 1,
                    Name = "Tom",
                    Age = 29,
                },
                new UserERP
                {
                    Id = 2,
                    Name = "Clark (ERP)",
                    Age = 24
                },
                new UserERP
                {
                    Id = 3,
                    Name = "Jerry",
                    Age = 23,
                },
                new UserERP
                {
                    Id = 6,
                    Name = "Scott",
                    Age = 87
                }
            })
        { }
    }
    #endregion
}
