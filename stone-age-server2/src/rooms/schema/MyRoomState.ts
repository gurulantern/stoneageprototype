import { Schema, type, MapSchema } from "@colyseus/schema";

export class ExampleNetworkedEntity extends Schema {
    @type("string") id: string;
    @type("string") ownerId: string;
    @type("string") creationId: string = "";
    @type("number") xPos: number = 0;
    @type("number") yPos: number = 0;
    @type("number") timestamp: number;
    @type("number") tireRate: number;
    @type("number") stamina: number;
    @type("number") restoreRate: number;
    @type("boolean") gather: false;
    @type("boolean") observe: false;
    @type("boolean") sleep: false;
    @type("number") playerFood: number;
    @type("number") teamFood: number; 
    @type({map: "string"}) attributes = new MapSchema<string>();
}

export class ExampleNetworkedUser extends Schema {
    @type("string") sessionId: string;
    @type("boolean") connected: boolean;
    @type("number") timestamp: number;
    @type({map: "string"}) attributes = new MapSchema<string>();
}

export class ExampleRoomState extends Schema {
    @type({ map: ExampleNetworkedEntity }) networkedEntities = new MapSchema<ExampleNetworkedEntity>();
    @type({ map: ExampleNetworkedUser }) networkedUsers = new MapSchema<ExampleNetworkedUser>();
    @type({ map: "string" }) attributes = new MapSchema<string>();
}

