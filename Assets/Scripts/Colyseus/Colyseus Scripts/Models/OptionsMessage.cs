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
    public Dictionary<string, string> optionsToSet;
}
