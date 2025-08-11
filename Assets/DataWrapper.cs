using System;
using System.Collections.Generic;

[Serializable]
public class DataWrapper
{
    public List<Entry> entries;

    public DataWrapper(List<Entry> entriesList)
    {
        entries = entriesList;
    }

    public DataWrapper() { }
}

[Serializable]
public class Entry
{
    public string key;
    public string value;
    public string type;     // "Int" or "String"
    public string category; // "SliderConfig", etc.

    public Entry(string key, string value, string type, string category)
    {
        this.key = key;
        this.value = value;
        this.type = type;
        this.category = category;
    }

    public Entry() { }
}

public enum PrefType
{
    Int,
    Float,
    String
}

public enum PrefCategory
{
    BluetoothCommandConstruct,
    Encryption,
    SliderConfig,
    Model3DVisual
}
