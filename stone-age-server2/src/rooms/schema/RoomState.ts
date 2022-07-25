import { Schema, type, MapSchema } from "@colyseus/schema";

export class NetworkedEntity extends Schema {
  @type("string") id: string;
  @type("string") ownerId: string;
  @type("string") creationId: string = "";
  @type("number") xPos: number = 0;
  @type("number") yPos: number = 0;
  @type("number") wRot: number = 0;
  @type("number") xScale: number = 1;
  @type("number") yScale: number = 1;
  @type("number") xVel: number = 0;
  @type("number") yVel: number = 0;
  @type("number") timestamp: number;
  @type("boolean") sleep: boolean = false;
  @type("boolean") tired: boolean = false;
  @type("boolean") wake: boolean = true;
  @type("boolean") observe: boolean = false;
  @type("boolean") gather: boolean = false;
  @type("boolean") scare: boolean = false;
  @type("boolean") afraid: boolean = false;
  @type("number") fruit: number = 0;
  @type("number") meat: number = 0;
  @type("number") wood: number = 0;
  @type("number") seeds: number = 0;
  @type("number") observePoints: number = 0;
  @type({map: "string"}) attributes = new MapSchema<string>();
}

export class GatherableState extends Schema {
  @type("string") id: string = "ID";
  @type("string") gatherableType: string = "";
  @type("boolean") destroyed: boolean = false;
  @type("number") xPos: number = 0;
  @type("number") yPos: number = 0;
  @type("number") availableTimestamp: number = 0.0;
  @type("number") harvestTrigger: number = 0;
}

export class ScorableState extends Schema {
  @type("string") id: string = "ID";
  @type("string") scorableType: string = "";
  @type("number") xPos: number = 0;
  @type("number") yPos: number = 0;
  @type("number") availableTimestamp: number = 0.0;
  @type("number") woodCost: number = 0;
  @type("number") seedsCost: number = 0;
} 

export class NetworkedUser extends Schema {
    @type("string") sessionId: string;
    @type("boolean") connected: boolean;
    @type("number") timestamp: number;
    @type({map: "string"}) attributes = new MapSchema<string>();
}

export class RoomState extends Schema {
    @type({ map: NetworkedEntity }) networkedEntities = new MapSchema<NetworkedEntity>();
    @type({ map: NetworkedUser }) networkedUsers = new MapSchema<NetworkedUser>();
    @type({ map: GatherableState }) gatherableObjects = new MapSchema<GatherableState>();
    @type({ map: ScorableState }) scorableObjects = new MapSchema<ScorableState>();
    @type({ map: "string" }) attributes = new MapSchema<string>();
    @type("number") roundTime: number = 180;
    @type("number") paintTime: number = 120;
    @type("number") voteTime: number = 60;
    @type("boolean") paintRound: boolean = true;
    @type("boolean") allianceToggle: boolean = false;
    @type("boolean") stealToggle: boolean = false;
    @type("boolean") tagsToggle: boolean = false;
    @type("number") foodScoreMultiplier: number = 2;
    @type("number") observeScoreMultiplier: number = 2;
    @type("number") createScoreMultiplier: number = 2;
    @type("number") tireRate: number = .5;
}

