import { Schema, type, MapSchema } from "@colyseus/schema";

export class NetworkedEntity extends Schema {
    @type("string") id: string;
    @type("string") ownerId: string;
    @type("string") creationId: string = "";
    @type("number") xPos: number;
    @type("number") yPos: number;
    @type("number") timestamp: number;
    @type("number") xVel: number;
    @type("number") yVel: number;
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
}

