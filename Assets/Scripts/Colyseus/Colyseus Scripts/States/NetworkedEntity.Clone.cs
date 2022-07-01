using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkedEntity
{
	// Make sure to update Clone fi you add any attributes
	public NetworkedEntity Clone()
	{
		return new NetworkedEntity() { id = id, ownerId = ownerId, creationId = creationId, xPos = xPos, yPos = yPos, zPos = zPos, xRot = xRot, yRot = yRot, zRot = zRot, wRot = wRot, xScale = xScale, yScale = yScale, zScale = zScale, timestamp = timestamp, sleep = sleep, tired = tired, wake = wake, observe = observe, gather = gather, attributes = attributes };
	}
}
