using System;
using System.Drawing;

namespace application {
    public struct RoomDefinition {
        public Rect RoomArea;
        public int ID;
        private static int ids = 0;

        /// <summary>
        /// Creates a Room Definition containing the room area and room GUID 
        /// </summary>
        /// <param name="roomArea">Area of room</param>
        /// <param name="id">ID of room. Should be a GUID!</param>
        public RoomDefinition(Rect roomArea, int id) {
            RoomArea = roomArea;
            ID = id;
        }
        
        public RoomDefinition(Rect roomArea) : this(roomArea, ids++) {}
    }
}