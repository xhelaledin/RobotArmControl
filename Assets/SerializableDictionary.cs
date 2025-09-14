using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveReference
{
    public string saveName;
    public List<int> values;
    public string dateString;
}

[Serializable]
public class SaveListData
{
    public List<SaveReference> saves = new List<SaveReference>();
    public string createdDate;
}

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

[Serializable]
public class GroupedSerializableDictionary
{
    public List<int> modelIndices = new List<int>();
    public List<SerializableDictionary> perModelData = new List<SerializableDictionary>();

    public GroupedSerializableDictionary() { }

    public GroupedSerializableDictionary(Dictionary<int, Dictionary<string, SaveListData>> grouped)
    {
        foreach (var kvp in grouped)
        {
            modelIndices.Add(kvp.Key);
            perModelData.Add(new SerializableDictionary(kvp.Value));
        }
    }

    public Dictionary<int, Dictionary<string, SaveListData>> ToGroupedDictionary()
    {
        var grouped = new Dictionary<int, Dictionary<string, SaveListData>>();
        for (int i = 0; i < modelIndices.Count; i++)
        {
            grouped[modelIndices[i]] = perModelData[i].ToDictionary();
        }
        return grouped;
    }
}

public static class JsonUtilityWrapper
{
    public static string ToJson(GroupedSerializableDictionary dict)
    {
        return JsonUtility.ToJson(dict);
    }

    public static GroupedSerializableDictionary FromJsonGrouped(string json)
    {
        return JsonUtility.FromJson<GroupedSerializableDictionary>(json);
    }
}
