using UnityEngine;
using System.Collections;

public class CustomMethodMessage
{
    // The name of the method we want to run on the server
    public string method;

    // Optional array of parameters to be sent to the clients
    public object[] param;
}
