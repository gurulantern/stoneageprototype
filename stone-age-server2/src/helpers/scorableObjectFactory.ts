import { ScorableState } from "../rooms/schema/RoomState"

 /**
 * Begin the process to matchmake into a room.
 * @param room The room name for the user to matchmake into.
 * @param progress The room filter representing the grid the user is currently in.
 * @returns The seat reservation for the room.
 */
// export async function matchMakeToRoom(room: string = "lobby_room", progress: string = "0,0"): Promise<matchMaker.SeatReservation> {
//     return await matchMaker.joinOrCreate(room, { progress });
// }

export function getStateForType(type: string) : ScorableState {
    let state : ScorableState = new ScorableState();

    //Any new types need an appropriate constructor in here or they will return empty
    switch(type){
        case("CAVE"):
        {
            state.assign({
                scorableType : type,
                finished : true
            });
            break;
        }
        case("FARM"):
        {
            state.assign({
                scorableType : type,
                finished : false
            });
            break;
        }
        case("AUROCHS_PEN"):
        {
            state.assign({
                scorableType : type,
                finished : false
            });
            break;
        }
        case("SAPLING"):
        {
            state.assign({
                scorableType: type,
                finished : true
            });
            break;
        }
        case("FISH_TRAP"):
        {
            state.assign({
                scorableType : type,
                finished : false
            });
            break;
        }
    }

    return state;
}