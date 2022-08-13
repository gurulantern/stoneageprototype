"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyRoom = void 0;
const colyseus_1 = require("colyseus");
const RoomState_1 = require("./schema/RoomState");
const gatherableObjectFactory = __importStar(require("../helpers/gatherableObjectFactory"));
const scorableObjectFactory = __importStar(require("../helpers/scorableObjectFactory"));
const logger = require("../helpers/logger");
class MyRoom extends colyseus_1.Room {
    constructor() {
        super(...arguments);
        this.clientEntities = new Map();
        this.serverTime = 0;
        this.gatherTime = 180;
        this.paintTime = 120;
        this.voteTime = 60;
        this.foodScoreMultiplier = 2;
        this.observeScoreMultiplier = 1;
        this.createScoreMultiplier = 2;
        this.aurochs = 9;
        this.night = 60;
        this.deadAurochs = false;
        this.observeReq = 50;
        this.paintBonus = 20;
        this.customMethodController = null;
        this.observeObjects = ["Tree", "Fruit_Tree", "Aurochs", "Other_Player"];
        this.characterActions = ["steal", "scare", "create"];
        this.scoreTypes = ["gather", "create", "observe", "paint"];
    }
    /**
     * Getter function to retrieve the correct customLogic file. Will try .JS extension and then .TS
     * @param {*} fileName
     */
    getCustomLogic(fileName) {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                this.customMethodController = yield Promise.resolve().then(() => __importStar(require('./customLogic/' + fileName)));
            }
            catch (e) {
                logger.error(e);
            }
            return this.customMethodController;
        });
    }
    /**
     * Callback for the "customMethod" message from the client to run a custom function within the custom logic.
     * Function name is sent from a client.
     * @param {*} client
     * @param {*} request
     */
    onCustomMethod(client, request) {
        try {
            if (this.customMethodController != null) {
                this.customMethodController.ProcessMethod(this, client, request);
            }
            else {
                logger.debug("NO Custom Method Logic Set");
            }
        }
        catch (error) {
            logger.error("Error with custom Method logic: " + error);
        }
    }
    /**
     * Callback for the "entityUpdate" message from the client to update an entity
     * @param {*} clientID
     * @param {*} data
     */
    onEntityUpdate(clientID, data) {
        if (this.state.networkedEntities.has(`${data[0]}`) === false)
            return;
        let stateToUpdate = this.state.networkedEntities.get(data[0]);
        let startIndex = 1;
        if (data[1] === "attributes")
            startIndex = 2;
        for (let i = startIndex; i < data.length; i += 2) {
            const property = data[i];
            let updateValue = data[i + 1];
            if (updateValue === "inc") {
                updateValue = data[i + 2];
                updateValue = parseFloat(stateToUpdate.attributes.get(property)) + parseFloat(updateValue);
                i++; // inc i once more since we had a inc;
            }
            if (startIndex == 2) {
                stateToUpdate.attributes.set(property, updateValue.toString());
            }
            else {
                stateToUpdate[property] = updateValue;
            }
        }
        stateToUpdate.timestamp = parseFloat(this.serverTime.toString());
    }
    /**
     * Callback for when the room is created
     * @param {*} options The room options sent from the client when creating a room
     */
    onCreate(options) {
        return __awaiter(this, void 0, void 0, function* () {
            logger.info("*********************** STONE AGE ROOM CREATED ***********************");
            console.log(options);
            logger.info("***********************");
            this.maxClients = 32;
            this.setOptions(options, true);
            this.roomOptions = options;
            this.teams = new Map();
            if (options["roomId"] != null) {
                this.roomId = options["roomId"];
            }
            this.initializeMessageHandling();
            // Set the room state
            this.setState(new RoomState_1.RoomState());
            // Set the frequency of the patch rate
            this.setPatchRate(1000 / 20);
            // Retrieve the custom logic for the room (Competitive or COllaborative)
            yield this.getCustomLogic(options["logic"]);
            this.initializeGameTypeLogic(options);
            //this.gameSettings = {"logic" : "competitive"};
        });
    }
    // Callback when a client has joined the room
    onJoin(client, options) {
        logger.info(`Client joined!- ${client.sessionId} ***`);
        let newNetworkedUser = new RoomState_1.NetworkedUser().assign({
            sessionId: client.sessionId,
        });
        this.state.networkedUsers.set(client.sessionId, newNetworkedUser);
        logger.info(`${this.roomOptions["logic"]}`);
        client.send("onJoin", { newNetworkedUser: newNetworkedUser, customLogic: this.roomOptions["logic"], options: this.roomOptions });
        if (this.customMethodController != null) {
            if (this.customMethodController.ProcessUserJoined != null)
                this.customMethodController.ProcessUserJoined(this, client);
        }
    }
    /**
     * Set the attribute of an entity or a user
     * @param {*} client
     * @param {*} attributeUpdateMessage
     */
    setAttribute(client, attributeUpdateMessage) {
        if (attributeUpdateMessage == null
            || (attributeUpdateMessage.entityId == null && attributeUpdateMessage.userId == null)
            || attributeUpdateMessage.attributesToSet == null) {
            return; // Invalid Attribute Update Message
        }
        // Set entity attribute
        if (attributeUpdateMessage.entityId) {
            //Check if this client owns the object
            if (this.state.networkedEntities.has(`${attributeUpdateMessage.entityId}`) === false)
                return;
            this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).timestamp = parseFloat(this.serverTime.toString());
            let entityAttributes = this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).attributes;
            for (let index = 0; index < Object.keys(attributeUpdateMessage.attributesToSet).length; index++) {
                let key = Object.keys(attributeUpdateMessage.attributesToSet)[index];
                let value = attributeUpdateMessage.attributesToSet[key];
                entityAttributes.set(key, value);
            }
        }
        // Set user attribute
        else if (attributeUpdateMessage.userId) {
            //Check is this client ownes the object
            if (this.state.networkedUsers.has(`${attributeUpdateMessage.userId}`) === false) {
                logger.error(`Set Attribute - User Attribute - Room does not have networked user with Id - \"${attributeUpdateMessage.userId}\"`);
                return;
            }
            this.state.networkedUsers.get(`${attributeUpdateMessage.userId}`).timestamp = parseFloat(this.serverTime.toString());
            let userAttributes = this.state.networkedUsers.get(`${attributeUpdateMessage.userId}`).attributes;
            for (let index = 0; index < Object.keys(attributeUpdateMessage.attributesToSet).length; index++) {
                let key = Object.keys(attributeUpdateMessage.attributesToSet)[index];
                let value = attributeUpdateMessage.attributesToSet[key];
                userAttributes.set(key, value);
                logger.info(`User \"${attributeUpdateMessage.userId}\" has been updated with \"${key}\" of \"${value}\"`);
            }
        }
    }
    setOptions(options, creation) {
        /*
        if(optionsMessage == null
            || (optionsMessage.userId == null)
            || optionsMessage.optionsToSet == null) {
            return; // Invalid Option Update Message
        }
        */
        for (let index = 0; index < 14; index++) {
            let key = Object.keys(options)[index];
            let value = options[key];
            switch (key) {
                case "gatherTime":
                    this.gatherTime = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.gatherTime}`);
                    break;
                case "paintTime":
                    this.paintTime = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.paintTime}`);
                    break;
                case "voteTime":
                    this.voteTime = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.voteTime}`);
                    break;
                case "foodMulti":
                    this.foodScoreMultiplier = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.foodScoreMultiplier}`);
                    break;
                case "observeMulti":
                    this.observeScoreMultiplier = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.observeScoreMultiplier}`);
                    break;
                case "createMulti":
                    this.createScoreMultiplier = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.createScoreMultiplier}`);
                    break;
                case "aurochs":
                    this.aurochs = parseInt(value);
                    this.aurochsTotal = this.aurochs;
                    logger.info(`Set the ${key} setting to ${this.aurochsTotal}`);
                    break;
                case "night":
                    this.night = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.night}`);
                    break;
                case "deadAurochs":
                    this.deadAurochs = value === "True";
                    logger.info(`Set the ${key} setting to ${this.deadAurochs}`);
                    break;
                case "observeReq":
                    this.observeReq = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.observeReq}`);
                    break;
            }
        }
        if (creation == false) {
            Object.keys(options).forEach(key => {
                this.roomOptions[key] = options[key];
            });
            console.log(this.roomOptions);
            this.broadcast("newSettings", { optionsToSet: options });
            logger.info(`Sent client settings out!`);
        }
    }
    // Callback when a client has left the room
    onLeave(client, consented) {
        return __awaiter(this, void 0, void 0, function* () {
            let networkedUser = this.state.networkedUsers.get(client.sessionId);
            if (networkedUser) {
                networkedUser.connected = false;
            }
            logger.silly(`*** User Leave - ${client.sessionId} ***`);
            // this.clientEntities is keyed by client.sessionId
            // this.state.networkedUsers is keyed by client.sessionid
            try {
                if (consented) {
                    throw new Error("consented leave!");
                }
                logger.info("let's wait for reconnection for client: " + client.sessionId);
                const newClient = yield this.allowReconnection(client, 10);
                logger.info("reconnected! client: " + newClient.sessionId);
            }
            catch (e) {
                logger.info("disconnected! client: " + client.sessionId);
                logger.silly(`*** Removing Networked User and Entity ${client.sessionId} ***`);
                //remove user
                this.state.networkedUsers.delete(client.sessionId);
                //remove entites
                if (this.clientEntities.has(client.sessionId)) {
                    let allClientEntities = this.clientEntities.get(client.sessionId);
                    allClientEntities.forEach(element => {
                        this.state.networkedEntities.delete(element);
                    });
                    // remove the client from clientEntities
                    this.clientEntities.delete(client.sessionId);
                    if (this.customMethodController != null) {
                        if (this.customMethodController.ProcessUserLeft != null)
                            this.customMethodController.ProcessUserLeft(this, client);
                    }
                }
                else {
                    logger.error(`Can't remove entities for ${client.sessionId} - No entry in Client Entities!`);
                }
            }
        });
    }
    onDispose() {
    }
    initializeMessageHandling() {
        // Set the callback for the "ping" message for tracking server-client latency
        this.onMessage("ping", (client) => {
            client.send(0, { serverTime: this.serverTime });
        });
        // Set the callback for the "customMethod" message
        this.onMessage("customMethod", (client, request) => {
            this.onCustomMethod(client, request);
        });
        // Set the callback for the "entityUpdate" message
        this.onMessage("entityUpdate", (client, entityUpdateArray) => {
            if (this.state.networkedEntities.has(`${entityUpdateArray[0]}`) === false)
                return;
            this.onEntityUpdate(client.sessionId, entityUpdateArray);
        });
        this.onMessage("objectInit", (client, objectInfoArray) => {
            this.handleObjectInit(client, objectInfoArray);
        });
        this.onMessage("objectGathered", (client, objectInfoArray) => {
            this.handleGatherInteraction(client, objectInfoArray);
        });
        this.onMessage("animalInteraction", (client, objectInfoArray) => {
            this.handleAnimalInteraction(client, objectInfoArray);
        });
        this.onMessage("scoreChange", (client, objectInfoArray) => {
            this.handleScoreInteraction(client, objectInfoArray);
        });
        // Set the callback for the "removeFunctionCall" message
        this.onMessage("remoteFunctionCall", (client, RFCMessage) => {
            //Confirm Sending Client is Owner 
            if (this.state.networkedEntities.has(`${RFCMessage.entityId}`) === false)
                return;
            RFCMessage.clientId = client.sessionId;
            // Broadcast the "remoteFunctionCall" to all clients except the one the message originated from
            this.broadcast("onRFC", RFCMessage, RFCMessage.target == 0 ? {} : { except: client });
            logger.info(`*************** RFC CALL from ${RFCMessage.entityId} ***********`);
        });
        // Set the callback for the "setAttribute" message to set an entity or user attribute
        this.onMessage("setAttribute", (client, attributeUpdateMessage) => {
            this.setAttribute(client, attributeUpdateMessage);
        });
        // Set options for room
        this.onMessage("setOptions", (client, optionsMessage) => {
            logger.info(`^^^^^^^^^^^^ Setting new options ^^^^^^^`);
            this.setOptions(optionsMessage.optionsToSet, false);
            logger.info(`^^^^^^^^^^^^ Options set ^^^^^^^^^^^^^^^`);
        });
        // Set the callback for the "removeEntity" message
        this.onMessage("removeEntity", (client, removeId) => {
            if (this.state.networkedEntities.has(removeId)) {
                this.state.networkedEntities.delete(removeId);
            }
        });
        // Set the callback for the "createEntity" message
        this.onMessage("createEntity", (client, creationMessage) => {
            this.handleEntityCreation(client, creationMessage);
        });
    }
    initializeGameTypeLogic(options) {
        if (this.customMethodController == null)
            logger.debug("NO Custom Logic Set");
        try {
            if (this.customMethodController != null) {
                this.setMetadata({ isCoop: options["logic"] == "stoneAgeCoop" });
                this.customMethodController.InitializeLogic(this, options);
            }
        }
        catch (error) {
            logger.error("Error with custom room logic: " + error);
        }
        // Set the Simulation Interval callback
        this.setSimulationInterval(dt => {
            this.serverTime += dt;
            //Run Custom Logic for room if loaded
            try {
                if (this.customMethodController != null)
                    this.customMethodController.ProcessLogic(this, dt);
            }
            catch (error) {
                logger.error("Error with custom room logic: " + error);
            }
        });
    }
    handleEntityCreation(client, creationMessage) {
        // Generate new UID for the entity
        let entityViewID = colyseus_1.generateId();
        let newEntity = new RoomState_1.NetworkedEntity().assign({
            id: entityViewID,
            ownerId: client.sessionId,
            timestamp: this.serverTime
        });
        let userName = entityViewID;
        if (creationMessage.attributes["userName"] != null) {
            userName = creationMessage.attributes["userName"];
        }
        if (creationMessage.creationId != null)
            newEntity.creationId = creationMessage.creationId;
        newEntity.timestamp = parseFloat(this.serverTime.toString());
        for (let key in creationMessage.attributes) {
            if (key === "creationPos") {
                newEntity.xPos = parseFloat(creationMessage.attributes[key][0]);
                newEntity.yPos = parseFloat(creationMessage.attributes[key][1]);
                //newEntity.zPos = parseFloat(creationMessage.attributes[key][2]);
            }
            else if (key === "creationRot") {
                //newEntity.xRot = parseFloat(creationMessage.attributes[key][0]);
                //newEntity.yRot = parseFloat(creationMessage.attributes[key][1]);
                //newEntity.zRot = parseFloat(creationMessage.attributes[key][2]);
                newEntity.wRot = parseFloat(creationMessage.attributes[key][3]);
            }
            else {
                newEntity.attributes.set(key, creationMessage.attributes[key].toString());
            }
        }
        // Add the entity to the room state's networkedEntities map 
        this.state.networkedEntities.set(entityViewID, newEntity);
        logger.silly(`*** Added an entity - ViewID = ${entityViewID} ***`);
        // Add the entity to the client entities collection
        if (this.clientEntities.has(client.sessionId)) {
            this.clientEntities.get(client.sessionId).push(entityViewID);
        }
        else {
            this.clientEntities.set(client.sessionId, [entityViewID]);
        }
        logger.silly(`*** Send Player Joined Message  - User Name = ${userName}***`);
        this.broadcast("playerJoined", { userName: userName }, { except: client });
    }
    handleObjectInit(client, objectInfo) {
        return __awaiter(this, void 0, void 0, function* () {
            if (objectInfo.length === 2) {
                if (this.state.gatherableObjects.has(objectInfo[0]) === false) {
                    let gatherable = gatherableObjectFactory.getStateForType(objectInfo[1]);
                    gatherable.assign({
                        id: objectInfo[0],
                    });
                    this.state.gatherableObjects.set(objectInfo[0], gatherable);
                    logger.silly(`**** Initializing ${gatherable.id} ***`);
                    this.broadcast("gatherableInitialized", { objectID: gatherable.id });
                }
                else {
                    logger.info(`**** Gatherables already contains ${objectInfo[0]} ****`);
                }
            }
            else {
                if (this.state.scorableObjects.has(objectInfo[0]) === false) {
                    let scorable = scorableObjectFactory.getStateForType(objectInfo[1]);
                    scorable.assign({
                        id: objectInfo[0],
                        xPos: objectInfo[2],
                        yPos: objectInfo[3],
                        teamId: objectInfo[4]
                    });
                    this.state.scorableObjects.set(objectInfo[0], scorable);
                    logger.silly(`**** Initializing ${scorable.id} ***`);
                    this.broadcast("scorableInitialized", { objectID: scorable.id });
                }
                else {
                    logger.info(`**** Scorables already contains ${objectInfo[0]} ****`);
                }
            }
        });
    }
    handleGatherInteraction(client, objectInfo) {
        return __awaiter(this, void 0, void 0, function* () {
            //Get the interactable item
            let gatherableObject = this.state.gatherableObjects.get(objectInfo[0]);
            let gatheringState = this.state.networkedEntities.get(objectInfo[1]);
            if (gatherableObject.harvestTrigger > 0) {
                gatherableObject.harvestTrigger -= 1;
                logger.info(`**** ${gatherableObject} is at ${gatherableObject.harvestTrigger} **** `);
            }
            else {
                logger.info(`**** ${gatherableObject} is out of resources **** `);
                gatherableObject = null;
            }
            if (gatheringState != null && gatherableObject != null) {
                this.broadcast("objectGathered", { gatheredObjectID: gatherableObject.id, gatheringStateID: gatheringState.id,
                    gatheredObjectType: gatherableObject.gatherableType });
            }
        });
    }
    handleAnimalInteraction(client, animalInfo) {
        return __awaiter(this, void 0, void 0, function* () {
            let animal = this.state.gatherableObjects.get(animalInfo[0]);
            if (animal != null) {
                animal.xPos = animalInfo[1];
                animal.yPos = animalInfo[2];
            }
            //this.broadcast("animalInteracted", { animalID: animal.id, destinationX: animal.xPos, destinationY: animal.yPos });
        });
    }
    handleScoreInteraction(client, objectInfo) {
        return __awaiter(this, void 0, void 0, function* () {
            //const userRepo = DI.em.fork().getRepository(User);
            //If the server is not yet aware of this item, lets change that
            if (this.state.scorableObjects.has(objectInfo[0]) === false) {
                let scorable = scorableObjectFactory.getStateForType(objectInfo[1]);
                scorable.assign({
                    id: objectInfo[0],
                });
                this.state.scorableObjects.set(objectInfo[0], scorable);
            }
            //Get the interactable item
            let scorableObject = this.state.scorableObjects.get(objectInfo[0]);
            let scoringState = this.state.networkedUsers.get(client.id);
            if (scoringState != null && scorableObject != null) {
                this.broadcast("objectScored", { scoredObjectID: scorableObject.id, scoringStateID: scoringState.sessionId });
            }
        });
    }
}
exports.MyRoom = MyRoom;
