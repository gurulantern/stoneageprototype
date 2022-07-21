using System.Collections.Generic;

// Message sent to update a room's options
public class OptionsMessage
{
    // Id of the networked user
    public string userId;

    // Map of the options to update
    public Dictionary<string, string> optionsToSet = new Dictionary<string, string>();
}

public class SettingsMessage
{
    /*
    public string[] keys;
    public string[] values;
    */
    public Dictionary<string, string> optionsToSet;
    /*
    public Dictionary<string, string> makeDictionary()
    {
        optionsToSet = new Dictionary<string, string>();
        for (int i = 0; i < keys.Length; i++)
        {
            optionsToSet.Add(keys[i], values[i]);
        }
        return optionsToSet;
    }
    */
}
