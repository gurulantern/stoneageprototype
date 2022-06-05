using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkedEntity
{
	// Make sure to update Clone fi you add any attributes
	public NetworkedEntity Clone()
	{
		return new NetworkedEntity() { id = id, ownerId = ownerId, creationId = creationId, xPos = xPos, yPos = yPos, xScale = xScale, yScale = yScale, timestamp = timestamp, attributes = attributes };
	}
}
