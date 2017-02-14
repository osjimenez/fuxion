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

namespace Fuxion.Test
{
    public class SynchronizationTest : BaseTest
    {
        public SynchronizationTest(ITestOutputHelper output) : base(output) { }
        [Fact(DisplayName = "Synchronization - Demo")]
        public void Demo()
        {
            var fdRepo = new RepoFuxion();
            var saltoRepo = new RepoCRM();
            var presenceRepo = new RepoERP();
            // Create sync session
            var ses = new SynchronizationSession
            {
                Name = "Test",
                Works = new[]
                {
                    new SynchronizationWork
                    {
                        Name = "Users",
                        #region Sides
                        Sides = new ISynchronizationSide[] {
                            new SynchronizationSide<RepoFuxion, UserFuxion, int>
                            {
                                IsMaster = true,
                                Name = "FUXION",
                                Source = fdRepo,
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",

                                OnLoad = s => s.Get().ToList(),
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Add(i),
                                OnDelete = (s, i) => s.Delete(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<UserFuxion, SkillFuxion, int> {
                                Name = "FUXION-SKILLS",
                                Source = null,
                                OnLoad = s => s.Skills,
                                PluralItemTypeName = "Skills",
                                SingularItemTypeName = "Skill",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Skills.Add(i),
                                OnDelete = (s, i) => s.Skills.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<SkillFuxion, SkillPropertyFuxion, int> {
                                Name = "FUXION-PROPERTIES",
                                Source = null,
                                OnLoad = s => s.Properties,
                                PluralItemTypeName = "Properties",
                                SingularItemTypeName = "Property",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Properties.Add(i),
                                OnDelete = (s, i) => s.Properties.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<RepoCRM, UserCRM, int>
                            {
                                //IsMaster = true,
                                Name = "CRM",
                                Source = saltoRepo,
                                OnLoad = s => s.Get().ToList(),
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Add(i),
                                OnDelete = (s, i) => s.Delete(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<UserCRM, SkillCRM, int> {
                                Name = "CRM-SKILLS",
                                Source = null,
                                OnLoad = s => s.Skills,
                                PluralItemTypeName = "Skills",
                                SingularItemTypeName = "Skill",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => {
                                    if (s.Skills == null)s.Skills=new List<SkillCRM>();
                                    s.Skills.Add(i);
                                },
                                OnDelete = (s, i) => s.Skills.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<SkillCRM, SkillPropertyCRM, int> {
                                Name = "CRM-PROPERTIES",
                                Source = null,
                                OnLoad = s => s.Properties,
                                PluralItemTypeName = "Properties",
                                SingularItemTypeName = "Property",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => {
                                    if (s.Properties == null) s.Properties = new  List<SkillPropertyCRM>();
                                    s.Properties.Add(i);
                                },
                                OnDelete = (s, i) => s.Properties.Remove(i),
                                OnUpdate = (s, i) => { }
                            },
                            new SynchronizationSide<RepoERP, UserERP, int>
                            {
                                //IsMaster = true,
                                Name = "ERP",
                                Source = presenceRepo,
                                OnLoad = s => s.Get().ToList(),
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",
                                OnNaming = i => i.Name,
                                OnInsert = (s, i) => s.Add(i),
                                OnDelete = (s, i) => s.Delete(i),
                                OnUpdate = (s, i) => { }
                            },
                        },
                        #endregion
                        #region Comparators
                        Comparators = new ISynchronizationComparator[] {
                            new SynchronizationComparator<UserFuxion, UserCRM, int>
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
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                    if (a.Age != b.Age)
                                        p.AddProperty(nameof(a.Age), a.Age, b.Age);
                                }
                            },
                            new SynchronizationComparator<UserERP, UserFuxion, int>
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
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                    if (a.Age != b.Age)
                                        p.AddProperty(nameof(a.Age), a.Age, b.Age);
                                }
                            },
                            new SynchronizationComparator<SkillERP, SkillFuxion, int>
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
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                            new SynchronizationComparator<UserCRM, UserERP, int>
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
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                    if (a.Age != b.Age)
                                        p.AddProperty(nameof(a.Age), a.Age, b.Age);
                                }
                            },
                            new SynchronizationComparator<SkillCRM, SkillFuxion, int>
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
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                            new SynchronizationComparator<SkillPropertyCRM, SkillPropertyFuxion, int>
                            {
                                OnSelectKeyA = u => u.Id,
                                OnSelectKeyB = u => u.Id,
                                OnMapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new SkillPropertyFuxion { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                OnMapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new SkillPropertyCRM { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                OnCompare = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                        },
                        #endregion
                        #region Subworks
                        //SubWorks = new []
                        //{
                        //    new SynchronizationWork
                        //    {
                        //        Name = "Skills relations",
                        //        Sides = new ISynchronizationSide[]
                        //        {
                        //            new SynchronizationSide<UserFuxion,SkillFuxion,int>
                        //            {
                        //                IsMaster = true,
                        //                Name = "FUXION",
                        //                Source = null,
                        //                PluralItemTypeName = "Skills",
                        //                SingularItemTypeName = "Skill",
                        //                OnNaming = user => user.Name,
                        //                OnLoad = user => user.Skills,
                        //                OnDelete = (user, skill) => user.Skills.Remove(skill),
                        //                OnInsert = (user, skill) => user.Skills.Add(skill),
                        //                OnUpdate = (user, skill) => { }                                        
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                    }//.AddSubWork<UserFuxion,SkillFuxion,int>(null)
                }
            };

            // Preview synchronization
            var res = ses.PreviewAsync().Result;

            //res.Print();

            // Serialize
            DataContractSerializer ser = new DataContractSerializer(typeof(SynchronizationSessionPreview));
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
            var res2 = (SynchronizationSessionPreview)ser.ReadObject(str);

            // Run Sync
            ses.RunAsync(res2).Wait();

            // Check results
            //Assert.Equal(6, res.First().Count());
            //Assert.Equal(3, res.First().First().Count());
            //Assert.Equal(2, res.First().Skip(1).First().Count());

            Printer.WriteLine("TEST FINISHED");
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
    #region Side FD
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
        public ICollection<SkillPropertyFuxion> Properties { get; set; }
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SkillPropertyFuxion
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
                            Name = "Skill 1",
                            Properties = new[]
                            {
                                new SkillPropertyFuxion
                                {
                                    Id = 1,
                                    Name = "Property 1"
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
                            Name = "Skill Bob",
                            Properties = new[]
                            {
                                new SkillPropertyFuxion
                                {
                                    Id = 1,
                                    Name = "Property Bob"
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
    #region Side Salto
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
        public ICollection<SkillPropertyCRM> Properties { get; set; }
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SkillPropertyCRM
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
                    Name = "Tom (CRM modified)",
                    Age = 30,
                    Skills = new[]
                    {
                        new SkillCRM
                        {
                            Id = 1,
                            Name = "Skill 1 (CRM modified)",
                            Properties = new[]
                            {
                                new SkillPropertyCRM
                                {
                                    Id = 1,
                                    Name = "Property 1 (CRM modified)"
                                }
                            }
                        }
                    }
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
                            Name = "Skill 5"
                        }
                    }
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
                    Name = "Clark (ERP modified)",
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
