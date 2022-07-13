import { GatherableState } from "../rooms/schema/RoomState"

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
                harvestTrigger : 9
            });
            break;
        }
        case("FRUIT_TREE"):
        {
            state.assign({
                gatherableType : type,
                harvestTrigger : 16 
            });
            break;
        }
        case("DEAD_AUROCHS"):
        {
            state.assign({
                gatherableType : type,
                harvestTrigger : 1
            });
            break;
        }
        case("LIVE_AUROCHS"):
        {
            state.assign({
                gatherableType : type,
                harvestTrigger : 1
            });
            break;
        }
    }

    return state;
}