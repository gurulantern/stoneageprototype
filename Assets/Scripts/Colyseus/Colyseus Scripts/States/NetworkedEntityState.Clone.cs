using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Colyseus.Schema;
using UnityEngine;

public partial class NetworkedEntityState : Schema
{
	// Make sure to update Clone fi you add any attributes
	public NetworkedEntityState Clone()
	{
		return new NetworkedEntityState() { id = id, ownerId = ownerId, creationId = creationId, prefab = prefab, xPos = xPos, yPos = yPos, zPos = zPos, xRot = xRot, yRot = yRot, zRot = zRot, wRot =wRot, timestamp = timestamp, xVel = xVel, yVel = yVel, sessionId = sessionId, connected = connected, attributes = attributes };
	}
}
// Compare a property to determine changes of state
public class NetworkedEntityChanges
{
	// Name of property we're comparing
	public string Name { get; private set; }

	// The old value of the property
	public object OldValue { get; private set; }

	// The new value of the property
	public object NewValue { get; private set; }

	public NetworkedEntityChanges(string name, object oldValue, object newValue)
	{
		Name = name;
		OldValue = oldValue;
		NewValue = newValue;
	}

	public static List<NetworkedEntityChanges> Compare<T>(T oldObject, T newObject)
	{
		FieldInfo[] properties = typeof(T).GetFields();
		List<NetworkedEntityChanges> result = new List<NetworkedEntityChanges>();

		foreach (FieldInfo pi in properties)
		{

			object oldValue = pi.GetValue(oldObject), newValue = pi.GetValue(newObject);

			if (!object.Equals(oldValue, newValue))
			{
				result.Add(new NetworkedEntityChanges(pi.Name, oldValue, newValue));
			}
		}

		return result;
	}
}