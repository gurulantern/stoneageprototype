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

	[Type(2, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> attributes = new MapSchema<string>();
}
