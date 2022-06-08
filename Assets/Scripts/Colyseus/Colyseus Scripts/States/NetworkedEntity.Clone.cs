using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkedEntity
{
	// Make sure to update Clone fi you add any attributes
	public NetworkedEntity Clone()
	{
		return new NetworkedEntity() { id = id, ownerId = ownerId, creationId = creationId, prefab = prefab, xPos = xPos, yPos = yPos, timestamp = timestamp, xVel = xVel, yVel = yVel, attributes = attributes };
	}
}
