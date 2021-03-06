using System.Collections.Generic;

// Message sent to update an entity's attributes
public class AttributeUpdateMessage
{
    // ID of the entity to update
    public string entityId;

    // Id of the networked user
    public string userId;

    // Map of the attributes to update
    public Dictionary<string, string> attributesToSet = new Dictionary<string, string>();
}
