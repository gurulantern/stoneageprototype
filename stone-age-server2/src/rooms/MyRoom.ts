import { Room, Client, generateId } from "colyseus";
import { RoomState, NetworkedEntity, NetworkedUser, GatherableState, ScorableState } from "./schema/RoomState";
import * as gatherableObjectFactory from "../helpers/gatherableObjectFactory";
import * as scorableObjectFactory from "../helpers/scorableObjectFactory";
import { MapSchema } from "@colyseus/schema";
const logger = require("../helpers/logger");

export class MyRoom extends Room<RoomState> {

    clientEntities = new Map<string, string[]>();
    serverTime: number = 0;
    gatherTime: number = 180;
    paintTime : number = 120;
    voteTime : number = 60;
    foodScoreMultiplier: number = 2;
    observeScoreMultiplier: number = 1;
    createScoreMultiplier: number = 2;
    aurochs: number = 9;
    night: number = 60;
    deadAurochs: boolean = false;
    currentRoundTime: number;
    customMethodController: any = null;
    roomOptions: any;

    paintRound: boolean = true;
    allianceToggle: boolean = false;
    stealToggle: boolean = false;
    tagsToggle: boolean = false;
    tireRate: number = .5;
    aurochsTotal: number;

    CurrentCountDownState: string;
    currCountDown: number;
    currentTime: number;
    teams: Map<number, Map<string, Client>>;
    alliances: Map<number, number[]>;

    observeObjects: Array<string> = ["Tree", "Fruit_Tree", "Aurochs", "Other_Player"];
    scoreTypes: Array<string> = ["gather", "observe", "create"];
    /**
     * Getter function to retrieve the correct customLogic file. Will try .JS extension and then .TS
     * @param {*} fileName 
     */
    async getCustomLogic(fileName: string) {
        try {
            this.customMethodController = await import('./customLogic/' + fileName);

        } catch (e) {
            logger.error(e);
        }

        return this.customMethodController;
    }

    /**
     * Callback for the "customMethod" message from the client to run a custom function within the custom logic.
     * Function name is sent from a client.
     * @param {*} client 
     * @param {*} request 
     */
    onCustomMethod(client: Client, request: any) {
        try {
            if (this.customMethodController != null) {
                this.customMethodController.ProcessMethod(this, client, request);

            } else {
                logger.debug("NO Custom Method Logic Set");
            }

        } catch (error) {
            logger.error("Error with custom Method logic: " + error);
        }
    }

    /**
     * Callback for the "entityUpdate" message from the client to update an entity
     * @param {*} clientID 
     * @param {*} data 
     */
    onEntityUpdate(clientID: string, data: any) {
 
        if(this.state.networkedEntities.has(`${data[0]}`) === false) return;

        let stateToUpdate = this.state.networkedEntities.get(data[0]);
        
        let startIndex = 1;
        if(data[1] === "attributes") startIndex = 2;
        
        for (let i = startIndex; i < data.length; i+=2) {
            const property = data[i];
            let updateValue = data[i+1];
            if(updateValue === "inc") {
                updateValue = data[i+2];
                updateValue = parseFloat(stateToUpdate.attributes.get(property)) +  parseFloat(updateValue);
                i++; // inc i once more since we had a inc;
            }

            if(startIndex == 2) {
                stateToUpdate.attributes.set(property, updateValue.toString());
            } else {
                (stateToUpdate as any)[property] = updateValue;
            }
        }

        stateToUpdate.timestamp = parseFloat(this.serverTime.toString());
    }
        
    /**
     * Callback for when the room is created
     * @param {*} options The room options sent from the client when creating a room
     */
    async onCreate(options: any) {
        logger.info("*********************** STONE AGE ROOM CREATED ***********************");
        console.log(options);
        logger.info("***********************");

        this.maxClients = 32;
        this.roomOptions = options;

        this.teams = new Map<number, Map<string, Client>>();

        if(options["roomId"] != null) {
            this.roomId = options["roomId"];           
        }

        this.initializeMessageHandling();

        // Set the room state
        this.setState(new RoomState());

        // Set the frequency of the patch rate
        this.setPatchRate(1000 / 20);
    
        // Retrieve the custom logic for the room (Competitive or COllaborative)
        await this.getCustomLogic(options["logic"]);
        
        this.initializeGameTypeLogic(options);
    }

    // Callback when a client has joined the room
    onJoin(client: Client, options: any) {
        logger.info(`Client joined!- ${client.sessionId} ***`);
       
        let newNetworkedUser = new NetworkedUser().assign({
            sessionId: client.sessionId,
        });
        
        this.state.networkedUsers.set(client.sessionId, newNetworkedUser);

        client.send("onJoin", { newNetworkedUser: newNetworkedUser, customLogic: this.roomOptions["logic"]});

        if(this.customMethodController != null)
        {
            if(this.customMethodController.ProcessUserJoined != null)
                this.customMethodController.ProcessUserJoined(this, client);
        }
    }

    /**
     * Set the attribute of an entity or a user
     * @param {*} client 
     * @param {*} attributeUpdateMessage 
     */
    setAttribute (client: Client, attributeUpdateMessage: any) {
        if(attributeUpdateMessage == null 
            || (attributeUpdateMessage.entityId == null && attributeUpdateMessage.userId == null)
            || attributeUpdateMessage.attributesToSet == null) {
            return; // Invalid Attribute Update Message
        }

        // Set entity attribute
        if(attributeUpdateMessage.entityId){
            //Check if this client owns the object
            if(this.state.networkedEntities.has(`${attributeUpdateMessage.entityId}`) === false) return;
            
            this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).timestamp = parseFloat(this.serverTime.toString());
            let entityAttributes = this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).attributes;
            for (let index = 0; index < Object.keys(attributeUpdateMessage.attributesToSet).length; index++) {
                let key = Object.keys(attributeUpdateMessage.attributesToSet)[index];
                let value = attributeUpdateMessage.attributesToSet[key];
                entityAttributes.set(key, value);
            }
        }
        // Set user attribute
        else if(attributeUpdateMessage.userId) {
            
            //Check is this client ownes the object
            if(this.state.networkedUsers.has(`${attributeUpdateMessage.userId}`) === false) {
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

    setOptions (client: Client, optionsMessage: any) {
        if(optionsMessage == null 
            || (optionsMessage.userId == null)
            || optionsMessage.optionsToSet == null) {
            return; // Invalid Option Update Message
        }
        
        let newSettingsKeys: string[]; 
        let newSettingsValues: string[]; 
        
        for (let index = 0; index < 9; index++) {
            let key = Object.keys(optionsMessage.optionsToSet)[index];
            let value = optionsMessage.optionsToSet[key];

            switch(key) {
                case "gatherTime":
                    this.gatherTime = parseInt(value);
                    logger.info(`Set the ${key} setting to ${this.gatherTime}`);
                    break;
                case "paintTme":
                    if (parseInt(value) !== 0) {
                        this.paintTime = parseInt(value);
                        this.paintRound = true;
                        logger.info(`Set the ${key} setting to ${this.paintTime}`);
                        break;
                    } else {
                        this.paintRound = false;
                        break;
                    }
                case "voteTime":
                    if (this.paintRound = true) {
                        this.voteTime = parseInt(value);
                        logger.info(`Set the ${key} setting to ${this.voteTime}`);
                        break;
                    }
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
            }
        }

        this.roomOptions = optionsMessage.optionToSet; 
        this.broadcast("newSettings", { optionsToSet : optionsMessage.optionsToSet })
        logger.info(`Sent client settings out!`);
    }

    // Callback when a client has left the room
    async onLeave(client: Client, consented: boolean) {
        let networkedUser = this.state.networkedUsers.get(client.sessionId);
        
        if(networkedUser){
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
            const newClient = await this.allowReconnection(client, 10);
            logger.info("reconnected! client: " + newClient.sessionId);

        } catch (e) {
            logger.info("disconnected! client: " + client.sessionId);
            logger.silly(`*** Removing Networked User and Entity ${client.sessionId} ***`);
            
            //remove user
            this.state.networkedUsers.delete(client.sessionId);

            //remove entites
            if(this.clientEntities.has(client.sessionId)) {
                let allClientEntities = this.clientEntities.get(client.sessionId);
                allClientEntities.forEach(element => {

                    this.state.networkedEntities.delete(element);
                });

                // remove the client from clientEntities
                this.clientEntities.delete(client.sessionId);

                if(this.customMethodController != null)
                {
                    if(this.customMethodController.ProcessUserLeft != null)
                        this.customMethodController.ProcessUserLeft(this, client);
                }
            } 
            else{
                logger.error(`Can't remove entities for ${client.sessionId} - No entry in Client Entities!`);
            }
        }
    }

    onDispose() {
    }

    initializeMessageHandling(){
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
            if(this.state.networkedEntities.has(`${entityUpdateArray[0]}`) === false) return;

            this.onEntityUpdate(client.sessionId, entityUpdateArray);
        });

        this.onMessage("objectInit", (client, objectInfoArray) => {
            this.handleObjectInit(client, objectInfoArray);
        })

        
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
            if(this.state.networkedEntities.has(`${RFCMessage.entityId}`) === false) return;

            RFCMessage.clientId = client.sessionId;

            // Broadcast the "remoteFunctionCall" to all clients except the one the message originated from
            this.broadcast("onRFC", RFCMessage, RFCMessage.target == 0 ? {} : {except : client});
            logger.info(`*************** RFC CALL from ${RFCMessage.entityId} ***********`);
        });

        // Set the callback for the "setAttribute" message to set an entity or user attribute
        this.onMessage("setAttribute", (client, attributeUpdateMessage) => {
            this.setAttribute(client, attributeUpdateMessage); 
        });

        // Set options for room
        this.onMessage("setOptions", (client, optionsMessage) => {
            logger.info(`^^^^^^^^^^^^ Setting new options ^^^^^^^`);
            this.setOptions(client, optionsMessage);
            logger.info(`^^^^^^^^^^^^ Options set ^^^^^^^^^^^^^^^`);
        })


        // Set the callback for the "removeEntity" message
        this.onMessage("removeEntity", (client, removeId) => {
            if(this.state.networkedEntities.has(removeId)) {
                this.state.networkedEntities.delete(removeId);
            }
        });

        // Set the callback for the "createEntity" message
        this.onMessage("createEntity", (client, creationMessage) => {
            this.handleEntityCreation(client, creationMessage);
        });
    }

    initializeGameTypeLogic(options: any){
        if(this.customMethodController == null)  logger.debug("NO Custom Logic Set");

        try{
            if(this.customMethodController != null) {
                this.setMetadata({isCoop: options["logic"] == "stoneAgeCoop" });
                this.customMethodController.InitializeLogic(this, options);
            }
        }
        catch(error){
            logger.error("Error with custom room logic: " + error);
        }

        // Set the Simulation Interval callback
        this.setSimulationInterval(dt => {
            this.serverTime += dt;
            //Run Custom Logic for room if loaded
            try {
                if(this.customMethodController != null) 
                    this.customMethodController.ProcessLogic(this, dt);

            } catch (error) {
                logger.error("Error with custom room logic: " + error);
            }
            
        } );
    }

    handleEntityCreation(client : Client, creationMessage: any){
        // Generate new UID for the entity
        let entityViewID = generateId();
        let newEntity = new NetworkedEntity().assign({
            id: entityViewID,
            ownerId: client.sessionId,
            timestamp: this.serverTime
        });

        let userName = entityViewID;

        if(creationMessage.attributes["userName"] != null) {
            userName = creationMessage.attributes["userName"];
        }

        if(creationMessage.creationId != null) newEntity.creationId = creationMessage.creationId;

        newEntity.timestamp = parseFloat(this.serverTime.toString());

        for (let key in creationMessage.attributes) {
            if(key === "creationPos")
            {
                newEntity.xPos = parseFloat(creationMessage.attributes[key][0]);
                newEntity.yPos = parseFloat(creationMessage.attributes[key][1]);
                //newEntity.zPos = parseFloat(creationMessage.attributes[key][2]);
            }
            else if(key === "creationRot")
            {
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
        if(this.clientEntities.has(client.sessionId)) {
            this.clientEntities.get(client.sessionId).push(entityViewID);
        } else {
            this.clientEntities.set(client.sessionId, [entityViewID]);
        }

        logger.silly(`*** Send Player Joined Message  - User Name = ${userName}***`);

        this.broadcast("playerJoined", {userName: userName}, {except : client});
    }

    async handleObjectInit(client: Client, objectInfo: any) {
        if (objectInfo.length === 2) {
            if (this.state.gatherableObjects.has(objectInfo[0]) === false) {
                let gatherable = gatherableObjectFactory.getStateForType(objectInfo[1]);
                gatherable.assign({
                id: objectInfo[0],
                });
                this.state.gatherableObjects.set(objectInfo[0], gatherable);
                logger.silly(`**** Initializing ${gatherable.id} ***`);
                this.broadcast("gatherableInitialized", { objectID : gatherable.id });
            } else {
                logger.info(`**** Gatherables already contains ${objectInfo[0]} ****`);
            }
        } else {
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
                this.broadcast("scorableInitialized", { objectID : scorable.id });
            } else {
                logger.info(`**** Scorables already contains ${objectInfo[0]} ****`);
            }
        }
    }

    async handleGatherInteraction(client: Client, objectInfo: any) {
        //Get the interactable item
        let gatherableObject = this.state.gatherableObjects.get(objectInfo[0]);

        let gatheringState = this.state.networkedEntities.get(objectInfo[1]);
        if (gatherableObject.harvestTrigger > 0) {
            gatherableObject.harvestTrigger -= 1;
            logger.info(`**** ${gatherableObject} is at ${gatherableObject.harvestTrigger} **** `)
        } else {
            logger.info(`**** ${gatherableObject} is out of resources **** `)
            gatherableObject = null;
        }
        if (gatheringState != null && gatherableObject != null) {
            this.broadcast("objectGathered", { gatheredObjectID: gatherableObject.id, gatheringStateID: gatheringState.id, 
                gatheredObjectType : gatherableObject.gatherableType});
        }
    }

    async handleAnimalInteraction(client: Client, animalInfo: any) {
        let animal = this.state.gatherableObjects.get(animalInfo[0]);

        if (animal != null) {
            animal.xPos = animalInfo[1];
            animal.yPos = animalInfo[2];
        }

        //this.broadcast("animalInteracted", { animalID: animal.id, destinationX: animal.xPos, destinationY: animal.yPos });
    }

    async handleScoreInteraction(client: Client, objectInfo: any) {

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
    }
    
}
    

