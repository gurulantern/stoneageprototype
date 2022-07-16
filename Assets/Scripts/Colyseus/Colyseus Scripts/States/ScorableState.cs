// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class ScorableState : Schema {
	[Type(0, "string")]
	public string id = default(string);

	[Type(1, "string")]
	public string scorableType = default(string);

    [Type(2, "number")]
	public float xPos = default(float);

    [Type(3, "number")]
	public float yPos = default(float);

	[Type(4, "number")]
	public float availableTimestamp = default(float);

	[Type(5, "number")]
	public float woodCost = default(float);

    [Type(6, "number")]
	public float seedsCost = default(float);
}
