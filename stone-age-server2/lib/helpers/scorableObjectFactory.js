"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getStateForType = void 0;
const RoomState_1 = require("../rooms/schema/RoomState");
/**
* Begin the process to matchmake into a room.
* @param room The room name for the user to matchmake into.
* @param progress The room filter representing the grid the user is currently in.
* @returns The seat reservation for the room.
*/
// export async function matchMakeToRoom(room: string = "lobby_room", progress: string = "0,0"): Promise<matchMaker.SeatReservation> {
//     return await matchMaker.joinOrCreate(room, { progress });
// }
function getStateForType(type) {
    let state = new RoomState_1.ScorableState();
    //Any new types need an appropriate constructor in here or they will return empty
    switch (type) {
        case ("CAVE"):
            {
                state.assign({
                    scorableType: type,
                    finished: true
                });
                break;
            }
        case ("FARM"):
            {
                state.assign({
                    scorableType: type,
                    finished: false
                });
                break;
            }
        case ("AUROCHS_PEN"):
            {
                state.assign({
                    scorableType: type,
                    finished: false
                });
                break;
            }
        case ("SAPLING"):
            {
                state.assign({
                    scorableType: type,
                    finished: true
                });
                break;
            }
        case ("FISH_TRAP"):
            {
                state.assign({
                    scorableType: type,
                    finished: false
                });
                break;
            }
    }
    return state;
}
exports.getStateForType = getStateForType;
