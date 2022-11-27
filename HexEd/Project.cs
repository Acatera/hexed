using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HexEd
{
    public class Project
    {
        public string FilePath { get; set; } = default!;
        public List<Selection> Selections { get; set; } = new();
        public int Position { get; set; }

        public static Project Load(string file)
        {
            var contents=  File.ReadAllText(file);
            var project = JsonSerializer.Deserialize<Project>(contents);
            return project;
        }

        public void Save(string file)
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(file, json);
        }
    }
}
