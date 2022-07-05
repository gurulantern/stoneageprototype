import { GatherableState } from "../rooms/schema/RoomState"

 /**
 * Begin the process to matchmake into a room.
 * @param room The room name for the user to matchmake into.
 * @param progress The room filter representing the grid the user is currently in.
 * @returns The seat reservation for the room.
 */
// export async function matchMakeToRoom(room: string = "lobby_room", progress: string = "0,0"): Promise<matchMaker.SeatReservation> {
//     return await matchMaker.joinOrCreate(room, { progress });
// }

export function getStateForType(type: string) : GatherableState {
    let state : GatherableState = new GatherableState();

    //Any new types need an appropriate constructor in here or they will return empty
    switch(type){
        case("FRUIT"):
        {
            state.assign({
                gatherableType : type,
            });
            break;
        }
        case("TREE"):
        {
            state.assign({
                gatherableType : type,
            });
            break;
        }
        case("FRUIT_TREE"):
        {
            state.assign({
                gatherableType : type,
            });
            break;
        }
        case("DEAD_AUROCHS"):
        {
            state.assign({
                gatherableType : type,
            });
            break;
        }
        case("LIVE_AUROCHS"):
        {
            state.assign({
                gatherableType : type,
            });
            break;
        }
    }

    return state;
}