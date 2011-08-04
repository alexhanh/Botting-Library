using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Data
{
    public abstract class PersistantStorage
    {
        protected Dictionary<string, Dictionary<string, object>> map;

        protected List<string> affected_ids;

        public abstract void Commit();

        public PersistantStorage()
        {
            AutoCommitOnChange = false;

            map = new Dictionary<string, Dictionary<string, object>>();

            affected_ids = new List<string>();
        }

		////
		public int GetTotalObjectCount()
		{
			int total = 0;
			foreach (KeyValuePair<string, Dictionary<string, object>> kvp in map)
			{
				total += kvp.Value.Count;
			}

			return total;
		}

        public bool AutoCommitOnChange { get; set; }

        public object this[string id, string key]
        {
            get
            {
                return map[id][key];
            }
            set
            {
                Add(id, key, value);
            }
        }

        public void Clear()
        {
            foreach (string id in map.Keys)
                affected_ids.Add(id);

            map.Clear();
           
            if (AutoCommitOnChange)
                Commit();
        }

        public int GetInt(string id, string key)
        {
            object value = map[id][key];

            if (value.GetType() == typeof(int))
            {
                return (int)value;
            }

            int i = Convert.ToInt32(map[id][key]);

            map[id][key] = i;

            return i;
        }

        public bool GetBool(string id, string key)
        {
            object value = map[id][key];

            if (value.GetType() == typeof(bool))
            {
                return (bool)value;
            }

            bool b = Convert.ToBoolean(map[id][key]);

            map[id][key] = b;

            return b;
        }

        public DateTime GetDateTime(string id, string key)
        {
            object value = map[id][key];

            if (value.GetType() == typeof(DateTime))
            {
                return (DateTime)value;
            }

            DateTime dt = DateTime.FromBinary(Convert.ToInt64(value));

            map[id][key] = dt;

            return dt;
        }

        public object GetObject(string id, string key)
        {
            return map[id][key];
        }

        protected string Serialize(object obj)
        {
            Type type = obj.GetType();

            if (type == typeof(DateTime))
                return ((DateTime)obj).ToBinary().ToString();
            else
                return obj.ToString();
        }

        public void Add(string id, string key, object obj)
        {
            affected_ids.Add(id);

            Dictionary<string, object> m;
            if (map.TryGetValue(id, out m))
                m[key] = obj;
            else
                map[id] = new Dictionary<string, object>() { { key, obj } };

            if (AutoCommitOnChange)
                Commit();
        }

        public bool Contains(string id)
        {
            return map.ContainsKey(id);
        }

        public bool Contains(string id, string key)
        {
            return map.ContainsKey(id) && map[id].ContainsKey(key);
        }

        public void Remove(string id)
        {
            map.Remove(id);
        }

        public void Remove(string id, string key)
        {
			if (map.ContainsKey(id))
				map[id].Remove(key);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in map)
            {
                sb.Append(kvp.Key + ";");

                foreach (KeyValuePair<string, object> kvp2 in kvp.Value)
                {
                    Type type = kvp2.Value.GetType();

                    sb.Append(" " + kvp2.Key + "|" + kvp2.Value + ";");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
