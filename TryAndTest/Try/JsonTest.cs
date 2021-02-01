using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Try {
    static public class JsonTest {
        enum Enum {
            Zero, One, Two, Three
        }
        class Foo1 {
            public DateTime bar1;
            public TimeSpan bar2;
            public Foo1 bar3;
            [JsonIgnore]
            public bool IsNull {
                get => bar3 == null;
            }
            public bool IsBar3Null() {
                return bar3 == null;
            }
        }
        class Foo2 {
            public int bar1;
        }
        public static void TestPartialSerialize() {
            Foo1 foo = new Foo1 {
                bar1 = DateTime.Now,
                bar2 = new TimeSpan(1, 2, 3),
                bar3 = new Foo1 {
                    bar1 = new DateTime(1926, 8, 17),
                    bar2 = new TimeSpan(-1, -2, -3),
                    bar3 = null
                }
            };
            string json = JsonConvert.SerializeObject(new Foo1[] { foo, foo }, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            Console.WriteLine(json);
            Foo1[] foos = JsonConvert.DeserializeObject<Foo1[]>(json);
            Console.WriteLine($"{foos.Length}");
            //ShowSelectedInExplorer.FilesOrFolders(@"D:\Code", @"D:\Design", @"D:\test.txt");
            Console.ReadKey();
        }
        public static void TestMissingElementInArray() {
            string path = @"D:\Code\Visual Studio\Solutions\Shell Extensions\TryAndTest\Try\MissingElementInArray.json";
            Foo2[] foo2s = JsonConvert.DeserializeObject<Foo2[]>(new StreamReader(path).ReadToEnd());
            Console.WriteLine(foo2s[0].bar1);
        }
    }
}
