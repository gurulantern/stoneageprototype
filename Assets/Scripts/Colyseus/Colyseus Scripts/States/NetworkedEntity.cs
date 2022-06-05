// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.22
// 

using Colyseus.Schema;

public partial class NetworkedEntity : Schema {
	[Type(0, "string")]
	public string id = default(string);

	[Type(1, "string")]
	public string ownerId = default(string);

	[Type(2, "string")]
	public string creationId = default(string);

	[Type(3, "number")]
	public float xPos = default(float);

	[Type(4, "number")]
	public float yPos = default(float);

	[Type(5, "number")]
	public float xRot = default(float);

	[Type(6, "number")]
	public float yRot = default(float);

	[Type(7, "number")]
	public float xScale = default(float);

	[Type(8, "number")]
	public float yScale = default(float);

	[Type(9, "number")]
	public float timestamp = default(float);

	[Type(10, "number")]
	public float xVel = default(float);

	[Type(11, "number")]
	public float yVel = default(float);

	[Type(12, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> attributes = new MapSchema<string>();
}
