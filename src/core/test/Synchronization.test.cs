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

namespace Fuxion.Test
{
    public class SynchronizationTest
    {
        public SynchronizationTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void SyncDemo()
        {
            var fdRepo = new RepoFD();
            var saltoRepo = new RepoSalto();
            var presenceRepo = new RepoPresence();
            var ses = new SyncSession
            {
                Works = new[]
                {
                    new SyncWork
                    {
                        Sides = new ISyncSide[] {
                            new SyncSide<RepoFD, UserFD, int>
                            {
                                IsMaster = true,
                                Name = "FOOD-DEFENSE",
                                Source = fdRepo,
                                Loader = s => s.Get().ToList(),
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",
                                Nominator = i => i.Name,
                                Adder =(s,i) => s.Add(i),
                                Remover=(s,i)=> s.Delete(i),
                                Updater=(s,i)=> { }
                            },
                            new SyncSide<RepoSalto, UserSalto, int>
                            {
                                //IsMaster = true,
                                Name = "SALTO",
                                Source = saltoRepo,
                                Loader = s => s.Get().ToList(),
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",
                                Nominator = i => i.Name,
                                Adder =(s,i) => s.Add(i),
                                Remover=(s,i)=> s.Delete(i),
                                Updater=(s,i)=> { }
                            },
                            new SyncSide<RepoPresence, UserPresence, int>
                            {
                                //IsMaster = true,
                                Name = "PRESENCE",
                                Source = presenceRepo,
                                Loader = s => s.Get().ToList(),
                                PluralItemTypeName = "Users",
                                SingularItemTypeName = "User",
                                Nominator = i => i.Name,
                                Adder =(s,i) => s.Add(i),
                                Remover=(s,i)=> s.Delete(i),
                                Updater=(s,i)=> { }
                            },
                        },
                        Comparators = new ISyncComparator[] {
                            new SyncComparator<UserFD, UserSalto, int>
                            {
                                KeyASelector = u => u.Id,
                                KeyBSelector = u => u.Id,
                                MapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserSalto { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                MapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserFD { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                Function = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                            new SyncComparator<UserPresence, UserFD, int>
                            {
                                KeyASelector = u=>u.Id,
                                KeyBSelector = u=>u.Id,
                                MapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserFD { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                MapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserPresence { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                Function = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                            new SyncComparator<UserSalto, UserPresence, int>
                            {
                                KeyASelector = u=>u.Id,
                                KeyBSelector = u=>u.Id,
                                MapAToB = (a,b) =>
                                {
                                    if(b == null)
                                        return new UserPresence { Id = a.Id, Name = a.Name };
                                    b.Name = a.Name;
                                    return b;
                                },
                                MapBToA = (b,a) =>
                                {
                                    if(a == null)
                                        return new UserSalto { Id = b.Id, Name = b.Name };
                                    a.Name = b.Name;
                                    return a;
                                },
                                Function = (a, b, p) =>
                                {
                                    // Compruebo cada propiedad para ver si es igual, si no lo es agrego la propiedad al resultado de la preview
                                    if (a.Name != b.Name)
                                        p.AddProperty(nameof(a.Name), a.Name, b.Name);
                                }
                            },
                        },
                        //SubWorks = new []
                        //{
                        //    new SyncWork<UserA,UserB,SkillA,SkillB,int>
                        //    {
                        //        Comparer = new Func<SkillA, SkillB, ISyncPreview>((a,b)=>
                        //        {
                        //            var p = new SyncPreview<SkillA, SkillB>
                        //            {
                        //                ItemA = a,
                        //                ItemB = b,
                        //            };
                        //            if (a.Name != b.Name)
                        //                p.AddProperty(nameof(a.Name), a.Name, b.Name);
                        //            return p.Properties.Count() > 0 ? p : null;
                        //        })
                        //    }
                        //}
                    }
                }
            };
            var res = ses.PreviewAsync().Result;

            Printer.PrintAction = m => output.WriteLine(m);

            foreach (var work in res.Works)
            {
                Printer.Print("Work:");
                Printer.Ident(() =>
                {
                    foreach (var item in work.Items)
                    {
                        Printer.Print($"Item '{(item.MasterItemExist ? item.MasterItemName : "null")}' has '{item.Sides.Count()}' sides");
                        Printer.Ident(() =>
                        {
                            foreach (var side in item.Sides)
                            {
                                if (side.SideItemExist)
                                {
                                    Printer.Print($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side is named '{side.SideItemName}' with key '{side.Key}' and has '{side.Properties.Count()}' change(s)");
                                    Printer.Ident(() =>
                                    {
                                        foreach (var pro in side.Properties)
                                        {
                                            Printer.Print($"Property '{pro.PropertyName}' will be changed from '{pro.SideValue}' to '{pro.MasterValue}'");
                                        }
                                    });
                                }
                                else Printer.Print($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side does not exist");
                            }
                        });
                    }
                });
            }

            DataContractSerializer ser = new DataContractSerializer(typeof(SyncSessionPreview));
            var str = new MemoryStream();
            ser.WriteObject(str, res);
            str.Seek(0, SeekOrigin.Begin);
            var res2 = (SyncSessionPreview)ser.ReadObject(str);

            ses.RunAsync(res2).Wait();

            //Assert.Equal(6, res.First().Count());
            //Assert.Equal(3, res.First().First().Count());
            //Assert.Equal(2, res.First().Skip(1).First().Count());

            Debug.WriteLine("");
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
    public class UserFD
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<SkillFD> Skills { get; set; }
        public override string ToString() => Name;
    }
    public class SkillFD
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoFD : Repo<UserFD>
    {
        public RepoFD() : base(new[] {
                new UserFD
                {
                    Id = 1,
                    Name = "Modificado Salto-Presence",
                },
                new UserFD
                {
                    Id = 2,
                    Name = "Modificado Salto",
                },
                new UserFD
                {
                    Id = 3,
                    Name = "Modificado Presence",
                },
                new UserFD
                {
                    Id = 4,
                    Name = "Solo master"
                }
            })
        { }
    }
    #endregion
    #region Side Salto
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class UserSalto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<SkillSalto> Skills { get; set; }
        public override string ToString() => Name;
    }
    public class SkillSalto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoSalto : Repo<UserSalto>
    {
        public RepoSalto() : base(new[] {
                new UserSalto
                {
                    Id = 1,
                    Name = "Modificado Salto-Presence (S)",
                },
                new UserSalto
                {
                    Id = 2,
                    Name = "Modificado Salto (S)",
                },
                new UserSalto
                {
                    Id = 3,
                    Name = "Modificado Presence",
                },
                new UserSalto
                {
                    Id = 5,
                    Name = "Solo salto"
                }
            })
        { }
    }
    #endregion
    #region Side Presence
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class UserPresence
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<SkillPresence> Skills { get; set; }
        public override string ToString() => Name;
    }
    public class SkillPresence
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RepoPresence : Repo<UserPresence>
    {
        public RepoPresence() : base(new[] {
                new UserPresence
                {
                    Id = 1,
                    Name = "Modificado Salto-Presence (P)",
                },
                new UserPresence
                {
                    Id = 2,
                    Name = "Modificado Salto",
                },
                new UserPresence
                {
                    Id = 3,
                    Name = "Modificado Presence (P)",
                },
                new UserPresence
                {
                    Id = 6,
                    Name = "Solo presence"
                }
            })
        { }
    }
    #endregion
}
