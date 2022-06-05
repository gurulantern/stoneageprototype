// Potential targets of the RFC
public enum RFCTargets
{
    // Send this RFC to all connected clients
    ALL,

    // Send this RFC to all connected clients EXCEPT for the sender
    OTHERS
}

// Method call on clients in the same room
public class RFCMessage
{
    // The target of the RFC
    public RFCTargets target = RFCTargets.ALL;

    // The ID of the entity sending this RFC
    public string entityId;

    // The name of the function that will be called
    public string function;

    // Optional array of parameters to be sent to the clients
    public object[] param;
}
