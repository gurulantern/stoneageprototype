using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkedEntity
{
	// Make sure to update Clone fi you add any attributes
	public NetworkedEntity Clone()
	{
		return new NetworkedEntity() { id = id, ownerId = ownerId, creationId = creationId, xPos = xPos, yPos = yPos, wRot = wRot, xScale = xScale, yScale = yScale, xVel = xVel, yVel = yVel, timestamp = timestamp, sleep = sleep, tired = tired, wake = wake, observe = observe, gather = gather, fruit = fruit, meat = meat, wood = wood, seeds = seeds, observePoints = observePoints, attributes = attributes };
	}
}
