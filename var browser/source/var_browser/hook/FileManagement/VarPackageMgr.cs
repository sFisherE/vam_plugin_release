using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace var_browser
{
    [System.Serializable]
    public class AllSerializableVarPackage
    {
        public SerializableVarPackage[] Packages;
    }
    class VarPackageMgr
    {
        public static VarPackageMgr singleton=new VarPackageMgr();

        static string CachePath = "Cache/AllPackagesJSON/" + "AllPackages.bytes";
        public Dictionary<string, SerializableVarPackage> lookup = new Dictionary<string, SerializableVarPackage>();
        
        public SerializableVarPackage TryGetCache(string uid)
        {
            if (lookup.ContainsKey(uid))
            {
                return lookup[uid];
            }
            return null;
        }
        public bool existCache = false;
        public void Init()
        {
            existCache = false;
            if (File.Exists(CachePath))
            {
                existCache = true;
                using (FileStream stream = new FileStream(CachePath, FileMode.Open))
                {
                    if (stream != null)
                    {
                        BinaryFormatter formater = new BinaryFormatter();
                        List<KeyValuePair<string, SerializableVarPackage>> data = formater.Deserialize(stream) as List<KeyValuePair<string, SerializableVarPackage>>;
                        if (data != null)
                        {
                            for (int i = 0; i < data.Count; i++)
                            {
                                var item = data[i];
                                if(!lookup.ContainsKey(item.Key))
                                    lookup.Add(item.Key, item.Value);
                            }
                        }
                    }
                }
            }
        }
        public void Refresh()
        {
            bool dirty = false;
            foreach(var item in FileManager.PackagesByUid)
            {
                var uid = item.Key;
                if (!lookup.ContainsKey(uid))
                {
					string cacheJson = "Cache/AllPackagesJSON/" + uid + ".json";
                    if (File.Exists(cacheJson))
                    {
                        string text = File.ReadAllText(cacheJson);
                        SerializableVarPackage vp = Valve.Newtonsoft.Json.JsonConvert.DeserializeObject<SerializableVarPackage>(text);
                        lookup.Add(uid, vp);
                        dirty = true;
                    }
                }
            }
            if (dirty)
            {
                var packages = new List<KeyValuePair<string, SerializableVarPackage>>();
                foreach(var item in lookup)
                {
                    packages.Add(new KeyValuePair<string, SerializableVarPackage>(item.Key,item.Value));
                }
                using (FileStream stream = new FileStream(CachePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, packages);
                    stream.Flush();
                    stream.Close();
                }
            }
        }
    }
}
