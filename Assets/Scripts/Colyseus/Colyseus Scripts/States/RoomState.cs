// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.22
// 

using Colyseus.Schema;

public partial class RoomState : Schema {
	[Type(0, "map", typeof(MapSchema<NetworkedEntity>))]
	public MapSchema<NetworkedEntity> networkedEntities = new MapSchema<NetworkedEntity>();

	[Type(1, "map", typeof(MapSchema<NetworkedUser>))]
	public MapSchema<NetworkedUser> networkedUsers = new MapSchema<NetworkedUser>();

	[Type(2, "map", typeof(MapSchema<GatherableState>))]
	public MapSchema<GatherableState> gatherableObjects = new MapSchema<GatherableState>();

	[Type(3, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> attributes = new MapSchema<string>();

	[Type(4, "number")]
	public float roundTime = default(float);

	[Type(5, "number")]
	public float paintTime = default(float);

	[Type(6, "number")]
	public float voteTime = default(float);

	[Type(7, "boolean")]
	public bool paintRound = default(bool);

	[Type(8, "boolean")]
	public bool allianceToggle = default(bool);

	[Type(9, "boolean")]
	public bool stealToggle = default(bool);

	[Type(10, "boolean")]
	public bool tagsToggle = default(bool);

	[Type(11, "number")]
	public float foodScoreMultiplier = default(float);

	[Type(12, "number")]
	public float observeScoreMultiplier = default(float);

	[Type(13, "number")]
	public float createScoreMultiplier = default(float);

	[Type(14, "number")]
	public float tireRate = default(float);
}
