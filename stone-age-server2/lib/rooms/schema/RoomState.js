"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.RoomState = exports.NetworkedUser = exports.ScorableState = exports.GatherableState = exports.NetworkedEntity = void 0;
const schema_1 = require("@colyseus/schema");
class NetworkedEntity extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.creationId = "";
        this.xPos = 0;
        this.yPos = 0;
        this.wRot = 0;
        this.xScale = 1;
        this.yScale = 1;
        this.xVel = 0;
        this.yVel = 0;
        this.sleep = false;
        this.tired = false;
        this.wake = true;
        this.observe = false;
        this.gather = false;
        this.scare = false;
        this.afraid = false;
        this.fruit = 0;
        this.meat = 0;
        this.wood = 0;
        this.seeds = 0;
        this.attributes = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type("string")
], NetworkedEntity.prototype, "id", void 0);
__decorate([
    schema_1.type("string")
], NetworkedEntity.prototype, "ownerId", void 0);
__decorate([
    schema_1.type("string")
], NetworkedEntity.prototype, "creationId", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "xPos", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "yPos", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "wRot", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "xScale", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "yScale", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "xVel", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "yVel", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "timestamp", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "sleep", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "tired", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "wake", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "observe", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "gather", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "scare", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedEntity.prototype, "afraid", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "fruit", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "meat", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "wood", void 0);
__decorate([
    schema_1.type("number")
], NetworkedEntity.prototype, "seeds", void 0);
__decorate([
    schema_1.type({ map: "string" })
], NetworkedEntity.prototype, "attributes", void 0);
exports.NetworkedEntity = NetworkedEntity;
class GatherableState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.id = "ID";
        this.gatherableType = "";
        this.destroyed = false;
        this.xPos = 0;
        this.yPos = 0;
        this.availableTimestamp = 0.0;
        this.harvestTrigger = 0;
    }
}
__decorate([
    schema_1.type("string")
], GatherableState.prototype, "id", void 0);
__decorate([
    schema_1.type("string")
], GatherableState.prototype, "gatherableType", void 0);
__decorate([
    schema_1.type("boolean")
], GatherableState.prototype, "destroyed", void 0);
__decorate([
    schema_1.type("number")
], GatherableState.prototype, "xPos", void 0);
__decorate([
    schema_1.type("number")
], GatherableState.prototype, "yPos", void 0);
__decorate([
    schema_1.type("number")
], GatherableState.prototype, "availableTimestamp", void 0);
__decorate([
    schema_1.type("number")
], GatherableState.prototype, "harvestTrigger", void 0);
exports.GatherableState = GatherableState;
class ScorableState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.id = "ID";
        this.scorableType = "";
        this.xPos = 0;
        this.yPos = 0;
        this.availableTimestamp = 0.0;
        this.fruitPaid = 0;
        this.meatPaid = 0;
        this.woodPaid = 0;
        this.seedsPaid = 0;
        this.ownerId = "";
        this.teamId = "";
        this.finished = false;
    }
}
__decorate([
    schema_1.type("string")
], ScorableState.prototype, "id", void 0);
__decorate([
    schema_1.type("string")
], ScorableState.prototype, "scorableType", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "xPos", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "yPos", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "availableTimestamp", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "fruitPaid", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "meatPaid", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "woodPaid", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "seedsPaid", void 0);
__decorate([
    schema_1.type("string")
], ScorableState.prototype, "ownerId", void 0);
__decorate([
    schema_1.type("number")
], ScorableState.prototype, "teamId", void 0);
__decorate([
    schema_1.type("boolean")
], ScorableState.prototype, "finished", void 0);
exports.ScorableState = ScorableState;
class NetworkedUser extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.attributes = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type("string")
], NetworkedUser.prototype, "sessionId", void 0);
__decorate([
    schema_1.type("boolean")
], NetworkedUser.prototype, "connected", void 0);
__decorate([
    schema_1.type("number")
], NetworkedUser.prototype, "timestamp", void 0);
__decorate([
    schema_1.type({ map: "string" })
], NetworkedUser.prototype, "attributes", void 0);
exports.NetworkedUser = NetworkedUser;
class RoomState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.networkedEntities = new schema_1.MapSchema();
        this.networkedUsers = new schema_1.MapSchema();
        this.gatherableObjects = new schema_1.MapSchema();
        this.scorableObjects = new schema_1.MapSchema();
        this.attributes = new schema_1.MapSchema();
        this.roundTime = 180;
        this.paintTime = 120;
        this.voteTime = 60;
        this.paintRound = true;
        this.allianceToggle = false;
        this.stealToggle = false;
        this.tagsToggle = false;
        this.foodScoreMultiplier = 2;
        this.observeScoreMultiplier = 2;
        this.createScoreMultiplier = 2;
        this.tireRate = .5;
    }
}
__decorate([
    schema_1.type({ map: NetworkedEntity })
], RoomState.prototype, "networkedEntities", void 0);
__decorate([
    schema_1.type({ map: NetworkedUser })
], RoomState.prototype, "networkedUsers", void 0);
__decorate([
    schema_1.type({ map: GatherableState })
], RoomState.prototype, "gatherableObjects", void 0);
__decorate([
    schema_1.type({ map: ScorableState })
], RoomState.prototype, "scorableObjects", void 0);
__decorate([
    schema_1.type({ map: "string" })
], RoomState.prototype, "attributes", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "roundTime", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "paintTime", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "voteTime", void 0);
__decorate([
    schema_1.type("boolean")
], RoomState.prototype, "paintRound", void 0);
__decorate([
    schema_1.type("boolean")
], RoomState.prototype, "allianceToggle", void 0);
__decorate([
    schema_1.type("boolean")
], RoomState.prototype, "stealToggle", void 0);
__decorate([
    schema_1.type("boolean")
], RoomState.prototype, "tagsToggle", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "foodScoreMultiplier", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "observeScoreMultiplier", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "createScoreMultiplier", void 0);
__decorate([
    schema_1.type("number")
], RoomState.prototype, "tireRate", void 0);
exports.RoomState = RoomState;
