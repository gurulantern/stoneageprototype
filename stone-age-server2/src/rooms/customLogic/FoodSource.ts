export class FoodSource {
    foodCount: number;

    constructor(startingFoodCount: number) {
        if(isNaN(Number(startingFoodCount)))
        {
            throw 'Error - food source - invalid health = ${startingFoodCount}';
        }

        this.foodCount = Number(startingFoodCount);
    }
}

export class PlayerFood {
    foodCount: number;
}

export class CaveFood {
    foodCount: number;
}