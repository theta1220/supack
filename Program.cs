using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace supack
{
    public static class File
    {
        public static string Open(string file)
        {
            string text = "";
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch
            {
                Console.WriteLine("ファイルが見つからなかったなり:{0}", file);
            }
            return text;
        }
    }

    class Program
    {
        class Def
        {
            public Def(string name)
            {
                Name = name;
            }
            public string Name { get; private set; }
            public object Value { get; private set; }
        }
        static List<Def> Defs = new List<Def>();
        static void Main(string[] args)
        {
            var ext = "ss";
            if (args.Length >= 1) ext = args[0];
            string[] files = Directory.GetFiles(".", "*." + ext, SearchOption.AllDirectories);

            var outStr = "";
            foreach (string fileName in files)
            {
                var source = File.Open(fileName);
                var names = fileName.Split('/').Reverse().ToArray();
                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var parentName = (i + 1 < name.Length) ? names[i + 1] : "";
                    if (name == "." || name == "extension") continue;
                    var className = name.Replace("." + ext, "");
                    if (className == "entry")
                    {
                        source = "\n" + source + "\n";
                        break;
                    }
                    var type = (parentName == "extension") ? "extension" : "class";
                    source = CreateClassString(type, className, source);
                }
                if (source != "")
                {
                    outStr += source + "\n";
                }
            }
            outStr = PreProcess(outStr);
            Console.WriteLine(outStr);
        }

        static string CreateClassString(string type, string name, string source)
        {
            return type + " " + name + "\n{\n" + source + "\n}\n";
        }

        static string PreProcess(string source)
        {
            var res = "";
            var lines = source.Split('\n');
            bool read = true;
            foreach (var line in lines)
            {
                if (RemoveIndent(line).FirstOrDefault() == '#')
                {
                    var words = RemoveIndent(line).Split(' ');
                    var command = words[0];
                    if (command == "#def")
                    {
                        var def = new Def(words[1]);
                        Defs.Add(def);
                    }
                    else if (command == "#ifdef")
                    {
                        read = Defs.Find(o => o.Name == words[1]) != null;
                    }
                    else if (command == "#end")
                    {
                        read = true;
                    }
                    else
                    {
                        throw new Exception(string.Format("不明なプリプロセスコード:{0}", command));
                    }
                }
                else if (read)
                {
                    res += line + "\n";
                }
            }
            return res;
        }

        static string RemoveIndent(string source)
        {
            var res = "";
            var isIndent = true;
            foreach (var c in source)
            {
                if (c == ' ' && isIndent) continue;
                isIndent = false;
                res += c;
            }
            return res;
        }
    }
}
