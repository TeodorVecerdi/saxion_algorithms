﻿using System;
using System.Drawing;

/**
 * This class represents (the data for) a Room, at this moment only a rectangle in the dungeon.
 */
public class Room {
    public Rectangle area;
    public int id;
    private static int ids = 0;

    public Room(Rectangle pArea, int pId) {
        area = pArea;
        id = pId;
    }
    
    public Room(Rectangle pArea) : this(pArea, ids++){}

    public override string ToString() {
        return $"Room(ID={id}) {area.ToString()}";
    }

    //TODO: Implement a toString method for debugging?
    //Return information about the type of object and it's data
    //eg Room: (x, y, width, height)
}