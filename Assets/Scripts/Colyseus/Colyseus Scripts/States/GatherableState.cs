// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class GatherableState : Schema {
	[Type(0, "string")]
	public string id = default(string);

	[Type(1, "string")]
	public string gatherableType = default(string);

    [Type(2, "boolean")]
	public bool destroyed = default(bool);

    [Type(3, "number")]
	public float xPos = default(float);

    [Type(4, "number")]
	public float yPos = default(float);

	[Type(5, "number")]
	public float availableTimestamp = default(float);

	[Type(6, "number")]
	public float foodTotal = default(float);

    [Type(7, "number")]
	public float woodTotal = default(float);

	[Type(8, "number")]
	public float seedsTotal = default(float);

    [Type(9, "number")]
	public float harvestTrigger = default(float);

    [Type(10, "number")]
	public float resourceTaken = default(float);

	[Type(11, "number")]
	public float seedsTaken = default(float);
}
