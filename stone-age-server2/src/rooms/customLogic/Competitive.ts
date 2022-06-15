import { Client } from "colyseus";
import { RoomState, NetworkedEntity, NetworkedUser } from "../schema/RoomState";
import { MyRoom } from "../MyRoom";

const logger = require("../../helpers/logger.js");
const utilities = require('../../helpers/LSUtilities.js');

// string indentifiers for keys in the room attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const GameStart = "gameStart";
const GeneralMessage = "generalMessage";
const BeginRoundCountDown = "countDown";
const WinningTeamId = "winningTeamId";

const FoodScoreMultiplier = 10; //How many points 1 kill is worth

/** Enum for game state */
const StoneAgeServerGameState = {
    None: "None",
    Waiting: "Waiting",
    BeginRound: "BeginRound",
    SimulateRound: "SimulateRound",
    EndRound: "EndRound"
};
 
/** Enum for begin round count down */
const StoneAgeCountDownState = {
    Enter: "Enter",
    GetReady: "GetReady",
    CountDown: "CountDown"
};

/** Count down time before a round begins */
const StoneAgeCountDownTime: number = 5;

/**
 * The primary game loop on the server
 * @param roomRef Reference to the room
 * @param deltaTime The server delta time in seconds
 */
let gameLoop = function (roomRef: MyRoom, deltaTime: number){
// Update the game state
switch (getGameState(roomRef, CurrentState)) {
    case StoneAgeServerGameState.None:
        break;
    case StoneAgeServerGameState.Waiting:
        waitingLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.BeginRound:
        beginRoundLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.SimulateRound:
        simulateRoundLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.EndRound:
        endRoundLogic(roomRef, deltaTime);
        break;
    default:
        logger.error("Unknown Game State - " + getGameState(roomRef, CurrentState));
        break;
}
}

// Client Request Logic
// These functions get called by the client in the form of the "customMethod" message set up in the room.
//======================================
const customMethods: any = {};
/**
 * Called by the client when a player is killed (self-reported)
 * @param roomRef Reference to the room
 * @param client The reporting client
 * @param request In order, the Attacker's ID and the Victim's ID, for scoring purposes
 */
customMethods.foodCollected =  function (roomRef: MyRoom, client: Client, request: any) {
    
    //Don't count gathering until a round is going
    if(getGameState(roomRef, CurrentState) != StoneAgeServerGameState.SimulateRound) {
        logger.silly("Cannot gather until the game has begun!");
        return;
    }

    const param = request.param;

    // 0 = Gatherer ID | 1 = Food Source ID
    if(param == null || param.length < 2){
        throw "Missing food parameters";
        return;
    }

    const gathererID = param[0];
    const foodSourceID = param[1];
    let gatherer = roomRef.state.networkedEntities.get(gathererID);
    ///good for stealing from each other
    let foodSource = roomRef.state.networkedEntities.get(foodSourceID);
    
    if(gatherer != null){
        //Update the gathering player's stats and score
        let inventoryFood: number = getAttributeNumber(gatherer, "food", 0);
        inventoryFood += 1;
        let score: number = inventoryFood * FoodScoreMultiplier;
        roomRef.setAttribute(null, { entityId: gathererID, attributesToSet: { food: inventoryFood.toString(), score: score.toString() } });

        /// Update the team's score PLACE THIS IN THE CAVE SCRIPT
        updateTeamScore(roomRef, gathererID, 1);
    }
    else{
        logger.silly(`No attacking entity found with Id: ${gathererID}`)
    }

    if(foodSource != null){
        //Update death count of the dead player
        let foodTaken: number = getAttributeNumber(foodSource, "food", 0);
        foodTaken += 1;
        let score: number = (-foodTaken) * FoodScoreMultiplier;

        roomRef.setAttribute(null, {entityId: foodSourceID, attributesToSet: {food: foodTaken.toString(), score: score.toString() } });
        
        /// PLACE THIS IN CAVE SCRIPT
        updateTeamScore(roomRef, gathererID, -1);
    }
    else{
        logger.silly(`No dead entity found with Id: ${foodSourceID}`)
    }
}
//====================================== END Client Request Logic

// GAME LOGIC
//======================================
/**
 * Retrieve an attribute number from an entity by name
 * @param entity The entity who has the attribute we want
 * @param attributeName The string name of the attribute
 * @param defaultValue If the attribute is not found or is not a number, we return this
 */
let getAttributeNumber = function(entity: NetworkedEntity, attributeName: string, defaultValue: number): number {
    let attribute = entity.attributes.get(attributeName);
    let attributeNumber: number = defaultValue;
    if(attribute){
        attributeNumber = Number(attribute);
        if(isNaN(attributeNumber)){
            logger.error(`*** Error parsing entity's attributeNumber: ${attributeNumber} ***`);
            attributeNumber = defaultValue;
        }
    }
    else {
        return defaultValue;
    }

    return attributeNumber;
}

/**
 * Checks if all the connected clients have a 'readyState' of "ready"
 * @param {*} users The collection of users from the room's state
 */
 let checkIfGameStarted = function(users: Map<string, NetworkedUser>) {
    let gameStarted = true;

    let userArr: NetworkedUser[] = Array.from<NetworkedUser>(users.values());

    if(userArr.length <= 0)
        gameStarted = false;

    for(let user of userArr) {
        
        let readyState = user.attributes.get(GameStart);
        
        if(readyState == null || readyState != "start"){
            gameStarted = false;
            break;
        }
    }

    return gameStarted;
}
/**
 * Get the score of a given team
 * @param roomRef Reference to the room
 * @param teamIndex The index of the team who's score we want
 */
let getTeamScore = function(roomRef: MyRoom, teamIndex: number): number {
    let score: number = Number(roomRef.state.attributes.get(`team_${teamIndex.toString()}_score`));

    if(isNaN(score)) {
        return 0;
    }

    return score;
}

/**
 * Returns the game state of the server
 * @param {*} gameState Key for which game state you want, either the Current game state for the Last game state
 */
let getGameState = function (roomRef: MyRoom, gameState: string) {
    return roomRef.state.attributes.get(gameState);
}

/** Resets data tracking collection and unlocks the room */
let resetForNewRound = function (roomRef: MyRoom) {
    
    setUsersAttribute(roomRef, GameStart, "waiting");

    unlockIfAble(roomRef);
}

let resetPlayerScores = function(roomRef: MyRoom) {
    // Reset all player scores
    setEntitiesAttribute(roomRef, "score", "0");
    setEntitiesAttribute(roomRef, "food", "0");
    setEntitiesAttribute(roomRef, "wood", "0");
    setEntitiesAttribute(roomRef, "seeds", "0");
    //Remove winning team
    if(roomRef.state.attributes.has(WinningTeamId))
    {
        roomRef.state.attributes.delete(WinningTeamId);
    }
}

/**
 * Reset the score for each team to zero
 * @param roomRef Reference to the room
 */
let resetTeamScores = function(roomRef: MyRoom) {

    // Set teams initial score
    roomRef.teams.forEach((teamMap, teamIdx) => {
        setRoomAttribute(roomRef, `team_${teamIdx.toString()}_score`, "0");
    });
    
}

let updateTeamScore = function(roomRef: MyRoom, teamMateId: string, amount: number) {

    let teamIdx: number = -1;
    let clientId: string = "";
    let teamScore: number = -1;

    // Get client Id from entity
    let entity: NetworkedEntity = roomRef.state.networkedEntities.get(teamMateId);

    if(entity) {
        clientId = entity.ownerId;
        
        // Update the score of the team the clientId belongs to
        roomRef.teams.forEach((teamMap, team) => {
            if(teamIdx == -1 && teamMap.has(clientId)) {
                teamIdx = team;
            }
        });

        if(teamIdx >= 0) {
            teamScore = getTeamScore(roomRef, teamIdx);

            teamScore += amount;
    
            setRoomAttribute(roomRef, `team_${teamIdx}_score`, teamScore.toString());
        }
        else {
            logger.error(`Update Team Score - Error - No team found for client Id: ${clientId}`);
        }
        
    }
    else {
        logger.error(`Update Team Score - Error - No entity found with Id: ${teamMateId}`);
    }

}

/**
 * Sets attribute of all connected users.
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
let setUsersAttribute = function(roomRef: MyRoom, key: string, value: string) {
    
    for(let entry of Array.from<any>(roomRef.state.networkedUsers)) {
  
        let userKey = entry[0];
        let userValue = entry[1];
        let msg: any = {userId: userKey, attributesToSet: {}};
  
        msg.attributesToSet[key] = value;
  
        roomRef.setAttribute(null, msg);
    }
    
  }

  /**
 * Sets attribute of all connected entities.
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
  let setEntitiesAttribute = function(roomRef: MyRoom, key: string, value: string) {
    for(let entry of Array.from<any>(roomRef.state.networkedEntities)) {
  
        let entityKey = entry[0];
        let entityValue = entry[1];
        let msg: any = {entityId: entityKey, attributesToSet: {}};
  
        msg.attributesToSet[key] = value;
  
        roomRef.setAttribute(null, msg);
    }
  }
  
  /**
  * Sets attriubte of the room
  * @param {*} roomRef Reference to the room
  * @param {*} key The key for the attribute you want to set
  * @param {*} value The value of the attribute you want to set
  */
  let setRoomAttribute = function(roomRef: MyRoom, key: string, value: string) {
    roomRef.state.attributes.set(key, value);
  }

let unlockIfAble = function (roomRef: MyRoom) {
    if(roomRef.hasReachedMaxClients() === false) {
        roomRef.unlock();
    }
}

let checkIfEnoughPlayers = function(roomRef: MyRoom): boolean {

    let enough: boolean = true;

    if(roomRef.state.networkedUsers.size < 2) {
        // Number of players to play has dropped too low to continue, end the round
        enough = false;
    }
    
    // Check if either team does not have any players
    roomRef.teams.forEach((teamMap, teamIdx) => {
        
        if(teamMap.size == 0) {
            // This team no longer has any players 
            enough = false;
        }
    });

    return enough;
}
//====================================== END GAME LOGIC

// GAME STATE LOGIC
//======================================
/**
 * Move the server game state to the new state
 * @param {*} newState The new state to move to
 */
let moveToState = function (roomRef: MyRoom, newState: string) {

    //logger.silly(`*** Move State From = ${getGameState(CurrentState)} To = ${newState}`);

    // LastState = CurrentState
    setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));
            
    // CurrentState = newState
    setRoomAttribute(roomRef, CurrentState, newState);
}

/**
 * The logic run when the server is in the Waiting state
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef: MyRoom, deltaTime: Number) {
    
    let gameStarted: boolean = false;
    let enoughPlayers: boolean = false;

    // Switch on LastState since the waiting logic gets used in multiple places
    switch(getGameState(roomRef, LastState)){
        case StoneAgeServerGameState.None:
        case StoneAgeServerGameState.EndRound:
            
            // Check if there are enough players
            gameStarted = checkIfGameStarted(roomRef.state.networkedUsers);
            enoughPlayers = checkIfEnoughPlayers(roomRef);

            // Return out if game has not started yet
            if(gameStarted == false || enoughPlayers == false) { 
                
                setRoomAttribute(roomRef, GeneralMessage, `${(gameStarted == false ? "Waiting for game to start." : "")}${(enoughPlayers == false ? " There aren't enough players to begin." : "")}`);

                return;
            }

            setRoomAttribute(roomRef, GeneralMessage, "");

            // Lock the room
            roomRef.lock();

            resetPlayerScores(roomRef);
            resetTeamScores(roomRef);

            // Begin a new round
            moveToState(roomRef, StoneAgeServerGameState.BeginRound);
            break;
    }
}

/**
 * The logic run when the server is in the BeginRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let beginRoundLogic = function (roomRef: MyRoom, deltaTime: number) {
    
    switch(roomRef.CurrentCountDownState) {
        // Beginning a new round
        case StoneAgeCountDownState.Enter:
            
            // Reset the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "");

            // Broadcast to the clients that a round has begun
            roomRef.broadcast("beginRoundCountDown", {});

            // Reset count down helper value
            roomRef.currCountDown = 0;

            // Move to the GetReady state of the count down
            roomRef.CurrentCountDownState = StoneAgeCountDownState.GetReady;
            break;
        case StoneAgeCountDownState.GetReady:

            // Begin with "Get Ready!"
            // Set the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "START!");
            
            // Show the "Get Ready!" message for 3 seconds
            if(roomRef.currCountDown < 3){
                roomRef.currCountDown += deltaTime;
                return;
            }

            // Move to the CountDown state of the count down
            roomRef.CurrentCountDownState = StoneAgeCountDownState.CountDown;

            // Set count down helper to the Count Down Time
            roomRef.currCountDown = StoneAgeCountDownTime;

            break;
        case StoneAgeCountDownState.CountDown:
            
            // Update count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, `Starting in ${Math.ceil(roomRef.currCountDown).toString()}s`);

            // Update Count Down value
            if(roomRef.currCountDown >= 0){
                roomRef.currCountDown -= deltaTime;
                return;
            }
            
            // TODO: beginRound is expecting a boss health
            let fakeHealth = -1;
            roomRef.broadcast("beginRound", { fakeHealth });

            // Move to the Simulation state
            moveToState(roomRef, StoneAgeServerGameState.SimulateRound);

            // Reset Current Count Down state for next round
            roomRef.CurrentCountDownState = StoneAgeCountDownState.Enter;
            break;
    }

    
}

/**
 * The logic run when the server is in the SimulateRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let simulateRoundLogic = function (roomRef: MyRoom, deltaTime: Number) {
    
    // Check if there are enough players to continue
    if(checkIfEnoughPlayers(roomRef) == false) {

        // End round since there are not enough players on a team to finish the round
        moveToState(roomRef, StoneAgeServerGameState.EndRound);

        return;
    }
/*
    roomRef.teams.forEach((teamMap, teamIdx) => {
        let teamScore: number = getTeamScore(roomRef, teamIdx);

        // Check if the team score is enough to win
        if(getGameState(roomRef, CurrentState) == StoneAgeServerGameState.SimulateRound && teamScore >= roomRef.tdmScoreToWin) {
            
            setRoomAttribute(roomRef, WinningTeamId, teamIdx.toString());

            moveToState(roomRef, StoneAgeServerGameState.EndRound);
        }
    });
*/
}

/**
 * The logic run when the server is in the EndRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let endRoundLogic = function (roomRef: MyRoom, deltaTime: Number) {

    // Let all clients know that the round has ended
    roomRef.broadcast("onRoundEnd", { });

    // Reset the server state for a new round
    resetForNewRound(roomRef);

    // Move to Waiting state, waiting for someone to start the game for another round of play
    moveToState(roomRef, StoneAgeServerGameState.Waiting);
}

let alertClientsOfTeamChange = function (roomRef: MyRoom, clientID: string, teamIndex: Number, added: boolean){
    roomRef.broadcast("onTeamUpdate", { teamIndex: teamIndex, clientID: clientID, added: added.toString()});
}
//====================================== END GAME STATE LOGIC

// VME Room accessed functions
//======================================
/**
 * Initialize the Star Boss Co-op logic
 * @param {*} roomRef Reference to the room
 * @param {*} options Options of the room from the client when it was created
 */
exports.InitializeLogic = function (roomRef: MyRoom, options: any) {

    logger.silly(`*** Stone Age Competitive Logic Initialize ***`);
    /** The current state of the count down logic */
    roomRef.CurrentCountDownState = StoneAgeCountDownState.Enter;

    /** Used to help run the count down at the beginning of a new round. */
    roomRef.currCountDown = 0;

    roomRef.currentTime = 0;

    roomRef.tdmScoreToWin = options["scoreToWin"] ? Number(options["scoreToWin"]) : 10;

    logger.silly(`*** TDM - Score to win = ${roomRef.tdmScoreToWin} ***`);

    //If we ever want more than 2 teams, this will need to be updated
    roomRef.teams = new Map();
    roomRef.teams.set(0, new Map());
    roomRef.teams.set(1, new Map());

    // Set initial game state to waiting for all clients to be ready
    setRoomAttribute(roomRef, CurrentState, StoneAgeServerGameState.Waiting)
    setRoomAttribute(roomRef, LastState, StoneAgeServerGameState.None);

    resetForNewRound(roomRef);
}

/**
 * Run Game Loop Logic
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in milliseconds
 */
exports.ProcessLogic = function (roomRef: MyRoom, deltaTime: number) {
    
    gameLoop(roomRef, deltaTime / 1000); // convert milliseconds to seconds
}

/**
 * Processes requests from a client to run custom methods
 * @param {*} roomRef Reference to the room
 * @param {*} client Reference to the client the request came from
 * @param {*} request Request object holding any data from the client
 */ 
exports.ProcessMethod = function (roomRef: MyRoom, client: Client, request: any) {
    
    // Check for and run the method if it exists
    if (request.method in customMethods && typeof customMethods[request.method] === "function") {
        customMethods[request.method](roomRef, client, request);
    } else {
        throw "No Method: " + request.method + " found";
        return; 
    }
}

/**
 * Process report of a user leaving. If we were previously locked due to a game starting and didn't
 * unlock at the end because the room was full, we'll need to unlock now
 */ 
 exports.ProcessUserLeft = function (roomRef: MyRoom, client: Client) {
    if(roomRef.locked)
    {
        switch(getGameState(roomRef, CurrentState)){
        case StoneAgeServerGameState.Waiting:
            unlockIfAble(roomRef);
            break;
        case StoneAgeServerGameState.BeginRound:
        case StoneAgeServerGameState.SimulateRound:
        case StoneAgeServerGameState.EndRound:
            logger.silly(`Will not unlock the room, Game State - ${getGameState(roomRef, CurrentState)}`);
            break;
        }
    }

    //Remove player from their team
    roomRef.teams.forEach((playerMap, teamIdx) =>{
        if(playerMap.has(client.id))
        {
            playerMap.delete(client.id);
            alertClientsOfTeamChange(roomRef, client.id, teamIdx, false);
        }
    });
 }

 /**
 * Process report of a user leaving. If we were previously locked due to a game starting and didn't
 * unlock at the end because the room was full, we'll need to unlock now
 */ 
 exports.ProcessUserJoined = function (roomRef: MyRoom, client: Client) {
    let desiredTeam = -1;
    let currMin = 99999;
    let map = new Map();
    roomRef.teams.forEach((playerMap, teamIdx) =>{
        //Alert the incoming client of the current teams
        client.send("onReceiveTeam", { teamIndex: teamIdx, clients: Array.from(playerMap.keys()) });
        if(playerMap.size < currMin){
            currMin = playerMap.size;
            desiredTeam = teamIdx;
            map = playerMap;
        }
    });

    map.set(client.id, client);
    roomRef.teams.set(desiredTeam, map);
    //Alert the clients of a new player
    alertClientsOfTeamChange(roomRef, client.id, desiredTeam, true);
 }
//====================================== END Room accessed functions