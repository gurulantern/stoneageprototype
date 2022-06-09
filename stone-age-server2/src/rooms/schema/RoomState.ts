import { Schema, type, MapSchema } from "@colyseus/schema";
import { Vector3 } from "../../helpers/Vectors";

export class NetworkedEntityState extends Schema {
    @type("string") id: string;
    @type("string") ownerId: string;
    @type("string") creationId: string = "";
    @type("number") xPos: number;
    @type("number") yPos: number;
    @type("number") zPos: number;
    @type("number") xRot: number;
    @type("number") yRot: number;
    @type("number") zRot: number;
    @type("number") wRot: number;
    @type("number") timestamp: number;
    @type("number") xVel: number;
    @type("number") yVel: number;
    @type("string") sessionId: string;
    @type("boolean") connected: boolean;
    @type({map: "string"}) attributes = new MapSchema<string>();
}
export class RoomState extends Schema {
    @type({ map: NetworkedEntityState }) networkedUsers = new MapSchema<NetworkedEntityState>();
    @type({ map: "string" }) attributes = new MapSchema<string>();

    @type("number") serverTime: number = 0.0;

  getUserPosition(sessionId: string): Vector3 {

    if (this.networkedUsers.has(sessionId)) {

      const user: NetworkedEntityState = this.networkedUsers.get(sessionId);

      return {
        x: user.xPos,
        y: user.yPos,
        z: user.zPos
      };
    }

    return null;
  }

  setUserPosition(sessionId: string, position: Vector3) {
    if (this.networkedUsers.has(sessionId)) {

      const user: NetworkedEntityState = this.networkedUsers.get(sessionId);

      user.xPos = position.x;
      user.yPos = position.y;
      user.zPos = position.z;
    }
  }

  getUserRotation(sessionId: string): Vector3 {

    if (this.networkedUsers.has(sessionId)) {

      const user: NetworkedEntityState = this.networkedUsers.get(sessionId);

      return {
        x: user.xRot,
        y: user.yRot,
        z: user.zRot
      };
    }

    return null;
  }

}

