using System.Collections.Generic;

// Message sent to update a room's options
public class OptionsMessage
{
    // Id of the networked user
    public string userId;

    // Map of the options to update
    public Dictionary<string, string> attributesToSet = new Dictionary<string, string>();
}
