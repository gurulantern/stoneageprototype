import { Schema, type, MapSchema } from "@colyseus/schema";

export class NetworkedEntity extends Schema {
  @type("string") id: string;
  @type("string") ownerId: string;
  @type("string") creationId: string = "";
  @type("number") xPos: number = 0;
  @type("number") yPos: number = 0;
  @type("number") zPos: number = 0;
  @type("number") xRot: number = 0;
  @type("number") yRot: number = 0;
  @type("number") zRot: number = 0;
  @type("number") wRot: number = 0;
  @type("number") xScale: number = 1;
  @type("number") yScale: number = 1;
  @type("number") zScale: number = 1;
  @type("number") xVel: number = 0;
  @type("number") yVel: number = 0;
  @type("number") zVel: number = 0;
  @type("number") timestamp: number;
  @type("boolean") sleep: boolean = false;
  @type("boolean") tired: boolean = false;
  @type("boolean") wake: boolean = true;
  @type("boolean") observe: boolean = false;
  @type("boolean") gather: boolean = false;
  @type("number") food: number = 0;
  @type("number") wood: number = 0;
  @type("number") seeds: number = 0;
  @type("number") observePoints: number = 0;
  @type({map: "string"}) attributes = new MapSchema<string>();
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

