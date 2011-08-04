using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GR.Data
{
    public class PersistantFileStorage : PersistantStorage
    {
        protected string filename;

        public PersistantFileStorage(string filename)
            : base()
        {
            this.filename = filename;

            if (!File.Exists(filename))
                return;

            StreamReader reader = new StreamReader(filename);

            string line;

            while ((line = reader.ReadLine()) != null && line != "")
            {
                string[] s = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                
                string id = s[0];

                map[id] = new Dictionary<string, object>();

                for (int i = 1; i < s.Length; i++)
                {
                    string[] key_value = s[i].Split(new char[] { '|' });
                    map[id][key_value[0]] = key_value[1];
                }
            }

            reader.Close();
            reader.Dispose();
        }

        public override void Commit()
        {
            StreamWriter writer = new StreamWriter(filename, false);

            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in map)
            {
                writer.Write(kvp.Key + ";");

                foreach (KeyValuePair<string, object> kvp2 in kvp.Value)
                {
                    writer.Write(kvp2.Key + "|" + Serialize(kvp2.Value) + ";");
                }

                writer.WriteLine();
            }

            writer.Flush();
            writer.Close();
            writer.Dispose();

            affected_ids.Clear();
        }
    }
}
