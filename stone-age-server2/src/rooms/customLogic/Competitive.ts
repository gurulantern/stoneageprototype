import { Client, Clock } from "colyseus";
import { RoomState, NetworkedEntity, NetworkedUser, ScorableState } from "../schema/RoomState";
import { MyRoom } from "../MyRoom";

const logger = require("../../helpers/logger.js");
const utilities = require('../../helpers/LSUtilities.js');

let score: number;
let spawnInterval: number;
let spawnTime: number;
let sState: ScorableState;
let spender: NetworkedEntity;
let gatherer: NetworkedEntity;

//Clock initializer
let clock = new Clock(true);

// string indentifiers for keys in the room attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const ClientReadyState = "readyState";
const GeneralMessage = "generalMessage";
const BeginRoundCountDown = "countDown";
const WinningTeamId = "winningTeamId";
const ElapsedTime = "elapsedTime";
const scoreTypes: Array<string> = ["gather", "observe", "create"];


/** Enum for game state */
const StoneAgeServerGameState = {
    None: "None",
    Waiting: "Waiting",
    BeginRound: "BeginRound",
    SimulateRound: "SimulateRound",
    BeginPaintRound: "BeginPaintRound",
    PaintRound: "PaintRound",
    BeginVoteRound: "BeginVoteRound",
    VoteRound: "VoteRound",
    EndRound: "EndRound"
};
 
/** Enum for begin round count down */
const StoneAgeCountDownState = {
    Enter: "Enter",
    GetReady: "GetReady",
    CountDown: "CountDown"
};

/** Count down time before a round begins */
const StoneAgeCountDownTime: number = 10;

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
    case StoneAgeServerGameState.BeginPaintRound:
        beginPaintRoundLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.PaintRound:
        paintRoundLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.BeginVoteRound:
        beginVoteRoundLogic(roomRef, deltaTime);
        break;
    case StoneAgeServerGameState.VoteRound:
        voteRoundLogic(roomRef, deltaTime);
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
 * Called by the client when a player gathers (self-reported)
 * @param roomRef Reference to the room
 * @param client The reporting client
 * @param request In order, the Gatherer's ID and the Source's ID, for scoring purposes
 */
customMethods.gather =  function (roomRef: MyRoom, client: Client, request: any) {
    
    //Don't count gathering until a round is going
    /*
    if(getGameState(roomRef, CurrentState) != StoneAgeServerGameState.SimulateRound) {
        logger.silly("Cannot score gathered food until the game has begun or in gather round!");
        return;
    }
    */

    const param = request.param;

    // 0 = Gatherer ID | 1 = fruit gathered | 2 = meat gathered | 3 = teamIndex
    if(param == null || param.length < 2){
        throw "Missing food parameters";
        return;
    }

    const gathererID = param[0];
    const gatherableType = param[1];
    const amount = Number(param[2]);

    roomRef.state.networkedEntities.forEach((value, key) => {
        if (value.id == gathererID) {
            gatherer = value;
        }
    })
        
    if (gatherableType == "wood") {
        logger.silly(`${gatherer.id} is at ${gatherer.wood}`);
        gatherer.wood += amount;
        logger.silly(`${gatherer.id} gathered ${amount} from a ${gatherableType}`);
    } else if (gatherableType == "seeds") {
        logger.silly(`${gatherer.id} is at ${gatherer.fruit}`);
        logger.silly(`${gatherer.id} is at ${gatherer.seeds}`);
        gatherer.fruit += amount;
        gatherer.seeds += amount * 3;
        logger.silly(`${gatherer.id} gathered ${amount} fruit and ${amount * 3} seeds from a ${gatherableType}`);
    }  else if (gatherableType == "fruit") {
        logger.silly(`${gatherer.id} is at ${gatherer.fruit}`);
        gatherer.fruit += amount;
        logger.silly(`${gatherer.id} gathered ${amount} from a ${gatherableType}`);
    } else if (gatherableType == "meat") {
        logger.silly(`${gatherer.id} is at ${gatherer.meat}`);
        gatherer.meat += amount;
        logger.silly(`${gatherer.id} gathered ${amount} from a ${gatherableType}`);
    }
}

/**
 * Called by the client when a player observes (self-reported)
 * @param roomRef Reference to the room
 * @param client The reporting client
 * @param request In order, the Gatherer's ID and the Source's ID, for scoring purposes
 */
customMethods.observe = function (roomRef: MyRoom, client: Client, request: any) {
        //Don't count gathering until a round is going
        if(getGameState(roomRef, CurrentState) != StoneAgeServerGameState.SimulateRound) {
            logger.silly("Cannot score observe until the game has begun or in gather round!");
            return;
        }
    
        const param = request.param;
    
        // 0 = Gatherer ID | 1 = observed object | 2 = teamIndex
        if(param == null || param.length < 2){
            throw "Missing observe parameters";
            return;
        }

        const observerID = param[0];
        const observedType = param[1];
        const teamIndex = Number(param[2]);

        if (roomRef.teams.get(teamIndex).has(client.id)) {
            let score: number = 1 * roomRef.observeScoreMultiplier;
            updateTeamScores(roomRef, observerID, "observe", score);
            updateTypeAmount(roomRef, teamIndex, observedType);
            logger.silly(`${observerID} scored ${score} observe for team${teamIndex}`);
        } else {
            logger.silly(`No client with id of ${client.id} to score.`)
        }
}

customMethods.create = function (roomRef: MyRoom, client: Client, request: any) {
    const param = request.param;
    
    // 0 = Gatherer ID | 1 = observed object | 2 = teamIndex
    if(param == null || param.length < 2){
        throw "Missing observe parameters";
        return;
    }

    const creatorID = param[0];
    const createdType = param[1];
    const createScore = Number(param[2]);
    logger.silly(`${createScore} and ${param[2]}`);

    const teamIndex = Number(param[3]);

    if (roomRef.teams.get(teamIndex).has(client.id)) {
        let score: number = createScore * roomRef.createScoreMultiplier;
        updateTeamScores(roomRef, creatorID, "create", score);
        logger.silly(`${creatorID} scored ${score} create for team${teamIndex}`);
    } else {
        logger.silly(`No client with id of ${client.id} to score.`)
    }
}

customMethods.lose = function (roomRef: MyRoom, client: Client, request: any) {
    const param = request.param;

    if (param == null || param.length < 2) {
        throw "Missing spend parameters";
        return;
    }   
    const gathererID = param[0];
    const gatherableType = param[1];
    const amount = Number(param[2]);

    roomRef.state.networkedEntities.forEach((value, key) => {
        if (value.id == gathererID) {
            gatherer = value;
        }
    })
        
    if (gatherableType == "wood") {
        logger.silly(`${gatherer.id} is at ${gatherer.wood}`);
        gatherer.wood -= amount;
        logger.silly(`${gatherer.id} lost ${amount}`);
    } else if (gatherableType == "fruit") {
        logger.silly(`${gatherer.id} is at ${gatherer.fruit}`);
        gatherer.fruit -= amount;
        logger.silly(`${gatherer.id} lost ${amount}`);
    } else if (gatherableType == "meat") {
        logger.silly(`${gatherer.id} is at ${gatherer.meat}`);
        gatherer.meat -= amount;
        logger.silly(`${gatherer.id} lost ${amount}`);
    } 
}

customMethods.spend = function (roomRef: MyRoom, client: Client, request: any) {
    const param = request.param;

    if (param == null || param.length < 2) {
        throw "Missing spend parameters";
        return;
    }

    const spenderID = param[0];
    const createdID = param[1];
    const spentType = param[2];
    const spentAmount = Number(param[3]);
    const teamIndex = Number(param[4]);

    const progCost1 = Number(param[5]);

    let progCost2: number;

    roomRef.state.scorableObjects.forEach((value, key) => {
        if (value.id == createdID) {
            sState = value;
        }
    })

    roomRef.state.networkedEntities.forEach((value, key) => {
        if (value.id == spenderID) {
            spender = value;
        }
    })

    if (spentType == "wood") {
        logger.silly(`${spender.id} is at ${spender.wood}`);
        sState.woodPaid += spentAmount;
        spender.wood -= spentAmount; 
        logger.silly(`${spender.id} spent ${spentAmount} to bring ${sState.id} to ${sState.woodPaid} and is at ${spender.wood}`);
        if (sState.woodPaid > progCost1) {
            spender.wood = sState.woodPaid - progCost1;
            logger.silly(`${spender.id} got ${spender.wood} back`);
        }
    } else if (spentType == "seeds") {
        sState.seedsPaid += spentAmount;
        spender.seeds -= spentAmount; 
        logger.silly(`${spender.id} spent ${spentAmount} to bring ${sState.id} to ${sState.seedsPaid} and is at ${spender.seeds}`);
        if (sState.seedsPaid > progCost1) {
            spender.seeds = sState.seedsPaid - progCost1;
            logger.silly(`${spender.id} got ${spender.seeds} back`);
        }    
    } else if (spentType == "fruit") {
        sState.fruitPaid += spentAmount;
        spender.fruit -= spentAmount;
        if (roomRef.teams.get(teamIndex).has(client.id)) {
            let score: number = spentAmount * roomRef.foodScoreMultiplier;
            updateTeamScores(roomRef, spenderID, "gather", score );
            logger.silly(`${spenderID} scored ${score} gather for team ${teamIndex}`);
        } else {
            logger.silly(`No client with id of ${client.id} to score.`)
        } 
    } else if (spentType == "meat") {
        sState.meatPaid += spentAmount;
        spender.meat -= spentAmount; 
        if (roomRef.teams.get(teamIndex).has(client.id)) {
            let score: number = (spentAmount * 5) * roomRef.foodScoreMultiplier;
            updateTeamScores(roomRef, spenderID, "gather", score );
            logger.silly(`${spenderID} scored ${score} gather for team ${teamIndex}`);
        } else {
            logger.silly(`No client with id of ${client.id} to score.`)
        } 
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
 let checkIfUsersReady = function(users: Map<string, NetworkedUser>) {
    let playersReady = true;

    let userArr: NetworkedUser[] = Array.from<NetworkedUser>(users.values());

    if(userArr.length <= 0)
        playersReady = false;

    for(let user of userArr) {
        
        let readyState = user.attributes.get(ClientReadyState);
        
        if(readyState == null || readyState != "ready"){
            playersReady = false;
            break;
        }
    }

    return playersReady;
}

/**
 * Get the amount of object types observed of a given team
 * @param roomRef Reference to the room
 * @param teamIndex The index of the team who's score items we want
 * @param scoreItem The id of the type of object to be checked and updated
 */
 let updateTypeAmount = function(roomRef: MyRoom, teamIndex: number, scoreItem: string) {
    let typeAmount: number = Number(roomRef.state.attributes.get(`team${teamIndex.toString()}_${scoreItem}Observed`));

    if(isNaN(typeAmount)) {
        typeAmount = 0;
    }
    typeAmount += 1;
    setRoomAttribute(roomRef, `team${teamIndex.toString()}_${scoreItem}Observed`, typeAmount.toString());
    logger.info(`team${teamIndex} has observed ${typeAmount} ${scoreItem}`);
}

/**
 * Returns the most observed object for the team
 * @param roomRef Reference to the room
 * @param teamIndex Team number to check number of observed objects
 */
let findMostObserved = function(roomRef: MyRoom, teamIndex: number ): string {
    let mostObservedAmt: number = 0;
    let mostObserved: string = "";
    roomRef.observeObjects.forEach(function(object) {
        let currentObservedAmt: number = Number(roomRef.state.attributes.get(`team${teamIndex.toString()}_${object}Observed`));
        if (currentObservedAmt > mostObservedAmt) {
            mostObservedAmt = currentObservedAmt;
            mostObserved = object;
        }
    })
    setRoomAttribute(roomRef, `team${teamIndex.toString()}_${mostObserved}Observed`, "0");
    return mostObserved;
}

/**
 * Get the score of a given team
 * @param roomRef Reference to the room
 * @param teamIndex The index of the team who's score we want
 */
let getTeamScores = function(roomRef: MyRoom, teamIndex: number, scoreType: string): number {

    let score: number = Number(roomRef.state.attributes.get(`team${teamIndex.toString()}_${scoreType}Score`));


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
    
    setUsersAttribute(roomRef, ClientReadyState, "waiting");

    unlockIfAble(roomRef);
}

let resetPlayerData = function(roomRef: MyRoom) {
    //Remove winning team
    if(roomRef.state.attributes.has(WinningTeamId))
    {
        roomRef.state.attributes.delete(WinningTeamId);
    }
}

/**
 * Reset the score for each score for each team to zero
 * @param roomRef Reference to the room
 */
let resetTeamScores = function(roomRef: MyRoom) {
    // Set teams initial score
    roomRef.teams.forEach((team) => {
        setRoomAttribute(roomRef, `team${team.toString()}_gatherScore`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_observeScore`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_createScore`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_totalScore`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_TreeObserved`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_Fruit_TreeObserved`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_AurochsObserved`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_Other_PlayerObserved`, "0");
        setRoomAttribute(roomRef, `team${team.toString()}_Fishing_SpotObserved`, "0");
    });
    
}

/**
 * Updates team's select score sent to server and adds it to the total score for that team
 * @param roomRef Reference to the room
 * @param teamMateId Team member scoring points
 * @param scoreType The type of scoring sent to the server "gather," "observe," or "create"
 * @param amount The amount to be added to the select score and the total score
 */
let updateTeamScores = function(roomRef: MyRoom, teamMateId: string, scoreType: string, amount: number) {

    let teamIdx: number = -1;
    let clientId: string = "";
    let teamScore: number = 0;
    let totalScore: number = 0;

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
            teamScore = getTeamScores(roomRef, teamIdx, scoreType);
            totalScore = getTeamScores(roomRef, teamIdx, "total");
            
            teamScore += amount;
            totalScore += amount;

            if (scoreType == "observe" && teamScore == 100) {
                let mostObserved: string = findMostObserved(roomRef, teamIdx);
                roomRef.broadcast("onCreateUnlock", { teamIndex: teamIdx, mostObserved });
                logger.info(`team${teamIdx.toString()} unlocked`);
            }

            setRoomAttribute(roomRef, `team${teamIdx.toString()}_${scoreType}Score`, teamScore.toString());
            setRoomAttribute(roomRef, `team${teamIdx.toString()}_totalScore`, totalScore.toString())

            roomRef.broadcast("onScoreUpdate", { teamIndex: teamIdx, scoreType: scoreType, updatedScore: teamScore.toString()});
            roomRef.broadcast("onScoreUpdate", { teamIndex: teamIdx, scoreType: "total", updatedScore: totalScore.toString()});

            logger.info(`team${teamIdx.toString()}_${scoreType}Score: ${teamScore.toString()}`);
            logger.info(`team${teamIdx.toString()}_totalScore: ${totalScore.toString()}`);

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
 * Sets attribute of the room
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
    /*
    roomRef.teams.forEach((teamMap, teamIdx) => {
        
        if(teamMap.size == 0) {
            // This team no longer has any players 
            enough = false;
        }
    });
    */

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

    // LastState = CurrentState
    setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));
            
    // CurrentState = newState
    setRoomAttribute(roomRef, CurrentState, newState);

    logger.silly(`** Moving to new state - ${getGameState(roomRef, CurrentState)}**`)
}

/**
 * The logic run when the server is in the Waiting state
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef: MyRoom, deltaTime: number) {
    
    let playersReady: boolean = false;
    let enoughPlayers: boolean = false;

    // Switch on LastState since the waiting logic gets used in multiple places
    switch(getGameState(roomRef, LastState)){
        case StoneAgeServerGameState.None:
        case StoneAgeServerGameState.EndRound:
            
            // Check if there are enough players
            playersReady = checkIfUsersReady(roomRef.state.networkedUsers);
            enoughPlayers = checkIfEnoughPlayers(roomRef);

            // Return out if game has not started yet
            if(playersReady == false || enoughPlayers == false) { 
                
                setRoomAttribute(roomRef, GeneralMessage, `${(playersReady == false ? "Waiting for players to ready up." : "")}${(enoughPlayers == false ? " There aren't enough players to begin." : "")}`);

                return;
            }

            setRoomAttribute(roomRef, GeneralMessage, "");

            // Lock the room
            roomRef.lock();

            resetPlayerData(roomRef);
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
            setRoomAttribute(roomRef, BeginRoundCountDown, "Get Ready!");
            
            // Show the "Get Ready!" message for 1 seconds
            if(roomRef.currCountDown < 2){
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
            setRoomAttribute(roomRef, BeginRoundCountDown, `${Math.ceil(roomRef.currCountDown).toString()}`);

            // Added 1 to the aurochsTotal to make the division spawn an extra otherwise it would spawn right at the end of the game.
            spawnInterval = (roomRef.gatherTime / (roomRef.aurochsTotal + 1)) * 1000;
            spawnTime = spawnInterval;

            // Update Count Down value
            if(roomRef.currCountDown >= 0){
                roomRef.currCountDown -= deltaTime;
                return;
            } 
            
            setRoomAttribute(roomRef, BeginRoundCountDown, "Start!");
            // TODO: beginRound is expecting a boss health
            let time = roomRef.gatherTime;
            roomRef.broadcast("beginRound", { time });

            // Move to the Simulation state
            moveToState(roomRef, StoneAgeServerGameState.SimulateRound);
            clock.start(true);

            // Clear user's ready state for round begin
            setUsersAttribute(roomRef, ClientReadyState, "waiting");

            // Reset Current Count Down state for next round
            roomRef.CurrentCountDownState = StoneAgeCountDownState.Enter;
            break;
    }

    
}

/**
 * The logic run when the server is in the SimulateRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let simulateRoundLogic = function (roomRef: MyRoom, deltaTime: number) {
    
    // Check if there are enough players to continue
    if (checkIfEnoughPlayers(roomRef) == false) {

        // End round since there are not enough players on a team to finish the round
        moveToState(roomRef, StoneAgeServerGameState.EndRound);

        return;
    }

    setRoomAttribute(roomRef, ElapsedTime, String(clock.elapsedTime));

    /// Random Aurochs Spawner at certain intervals
    if(clock.elapsedTime >= (spawnTime))
    {
        let spawnPoint: number;
        let  doaBool: boolean;
        let doa: number = Math.floor(Math.random() * 2);
        if (doa === 1 && roomRef.deadAurochs === true) {
            doaBool = false;
            spawnPoint = Math.floor(Math.random() * 5);
        } else {
            doaBool = true;
            spawnPoint = Math.floor(Math.random() * 9);
        }
        roomRef.broadcast("spawnAurochs", { alive: doaBool, spawnPoint: spawnPoint });
        spawnTime += spawnInterval;
        logger.info(`Elapse time: ${clock.elapsedTime} and Spawn Time: ${spawnTime}`);
        logger.info(`Spawning an aurochs that is alive:${doaBool} and at spawn point:${spawnPoint}`);
    }

    if(clock.elapsedTime >= (roomRef.gatherTime * 1000))
    {
        if(roomRef.paintRound) {
            moveToState(roomRef, StoneAgeServerGameState.BeginPaintRound);
            logger.info(clock.elapsedTime);
        } else {
            moveToState(roomRef, StoneAgeServerGameState.EndRound);
        }
    }
    /*
    roomRef.teams.forEach((teamMap, teamIdx) => {
        let gatherScore: number = getTeamScores(roomRef, teamIdx, "gather");
    });
    */

    //setRoomAttribute(roomRef, WinningTeamId, teamIdx.toString());
}

let beginPaintRoundLogic = function (roomRef: MyRoom, deltaTime: number) {
    let time = roomRef.paintTime;
    roomRef.broadcast("beginPaintRound", { time });
    moveToState(roomRef, StoneAgeServerGameState.PaintRound);
}

/**
 * The logic run when the server is in the PaintRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let paintRoundLogic = function (roomRef: MyRoom, deltaTime: number) {

    // Check if there are enough players to continue
    if(checkIfEnoughPlayers(roomRef) == false) {

        // End round since there are not enough players on a team to finish the round
        moveToState(roomRef, StoneAgeServerGameState.EndRound);

        return;
    }
    let paintDiff = roomRef.gatherTime * 1000;

    setRoomAttribute(roomRef, ElapsedTime, String(clock.elapsedTime - paintDiff));

    if(clock.elapsedTime >= ((roomRef.gatherTime + roomRef.paintTime) * 1000))
    {
        moveToState(roomRef, StoneAgeServerGameState.BeginVoteRound);
        logger.info(clock.elapsedTime);
    }
}

let beginVoteRoundLogic = function (roomRef: MyRoom, deltaTime: number) {
    let time = roomRef.voteTime;
    roomRef.broadcast("beginVoteRound", { time });
    moveToState(roomRef, StoneAgeServerGameState.VoteRound);
}

let voteRoundLogic = function (roomRef: MyRoom, deltaTime: number) {

    // Check if there are enough players to continue
    if(checkIfEnoughPlayers(roomRef) == false) {

        // End round since there are not enough players on a team to finish the round
        moveToState(roomRef, StoneAgeServerGameState.EndRound);

        return;
    }
    let voteDiff = (roomRef.gatherTime + roomRef.paintTime) *  1000;

    setRoomAttribute(roomRef, ElapsedTime, String(clock.elapsedTime - voteDiff));

    if (clock.elapsedTime >= ((roomRef.gatherTime + roomRef.paintTime + roomRef.voteTime) * 1000))
    {
        moveToState(roomRef, StoneAgeServerGameState.EndRound);
        logger.info(clock.elapsedTime);
    }

}

/**
 * The logic run when the server is in the EndRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let endRoundLogic = function (roomRef: MyRoom, deltaTime: number) {

    // Let all clients know that the round has ended
    roomRef.broadcast("onRoundEnd", { });

    // Reset the server state for a new round
    resetForNewRound(roomRef);

    // Move to Waiting state, waiting for all players to ready up for another round of play
    moveToState(roomRef, StoneAgeServerGameState.Waiting);
}

let alertClientsOfTeamChange = function (roomRef: MyRoom, clientID: string, teamIndex: Number, added: boolean){
    roomRef.broadcast("onTeamUpdate", { teamIndex: teamIndex, clientID: clientID, added: added.toString()});
}
//====================================== END GAME STATE LOGIC

// VME Room accessed functions
//======================================
/**
 * Initialize the Stone Age Comp logic
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

    logger.silly(`*** Competitive Mode - Gather the most by time's end ***`);

    //If we ever want more than 2 teams, this will need to be updated
    roomRef.teams = new Map();
    roomRef.teams.set(0, new Map());
    roomRef.teams.set(1, new Map());
    roomRef.teams.set(2, new Map());
    roomRef.teams.set(3, new Map());

    roomRef.alliances = new Map();
    roomRef.alliances.set(0, []);
    roomRef.alliances.set(1, []);
    roomRef.alliances.set(2, []);

    // Set initial game state to waiting for all clients to be ready
    setRoomAttribute(roomRef, CurrentState, StoneAgeServerGameState.Waiting)
    setRoomAttribute(roomRef, LastState, StoneAgeServerGameState.None);
    logger.silly(`*** Room State set to Waiting ***`);

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