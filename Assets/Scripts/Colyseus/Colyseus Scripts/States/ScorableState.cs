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
	public float fruitPaid = default(float);

    [Type(6, "number")]
	public float meatPaid = default(float);

	[Type(7, "number")]
	public float woodPaid = default(float);

    [Type(8, "number")]
	public float seedsPaid = default(float);

    [Type(9, "string")]
	public string ownerId = default(string);

    [Type(10, "number")]
	public float teamId = default(float);
	
	[Type(11, "boolean")]
	public bool finished = default(bool);
}
