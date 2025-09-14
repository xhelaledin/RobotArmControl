using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary
{
    public List<string> keys = new List<string>();
    public List<SaveListData> values = new List<SaveListData>();

    public SerializableDictionary() { }

    public SerializableDictionary(Dictionary<string, SaveListData> dict)
    {
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, SaveListData> ToDictionary()
    {
        var dict = new Dictionary<string, SaveListData>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}

public static class JsonUtilityWrapper
{
    public static string ToJson(SerializableDictionary dict)
    {
        return JsonUtility.ToJson(dict);
    }

    public static SerializableDictionary FromJson<SerializableDictionary>(string json)
    {
        return JsonUtility.FromJson<SerializableDictionary>(json);
    }
}
