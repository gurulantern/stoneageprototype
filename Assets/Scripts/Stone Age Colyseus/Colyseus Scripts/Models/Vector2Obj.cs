using System;
using UnityEngine;

//Wrapper class for serializable Vector2 response we will receive from the server
[Serializable]
public class Vector2Obj
{
    public Vector2Obj()
    {
        x = 0;
        y = 0;
    }

    public Vector2Obj(Vector2 vector2)
    {
        x = vector2.x;
        y = vector2.y;
    }

    //The X-axis value
    public double x { get; set; }

    //The Y-axis value
    public double y { get; set; }

}
