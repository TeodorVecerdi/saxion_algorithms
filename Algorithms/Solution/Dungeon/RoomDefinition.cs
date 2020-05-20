using System;
using System.Drawing;

namespace application {
    public struct RoomDefinition {
        public Rectangle RoomArea;
        public string ID;

        /// <summary>
        /// Creates a Room Definition containing the room area and room GUID 
        /// </summary>
        /// <param name="roomArea">Area of room</param>
        /// <param name="id">ID of room. Should be a GUID!</param>
        public RoomDefinition(Rectangle roomArea, string id) {
            RoomArea = roomArea;
            ID = id;
        }
        
        public RoomDefinition(Rectangle roomArea) : this(roomArea, Guid.NewGuid().ToString()) {}
    }
}