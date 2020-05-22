using System;
using System.Drawing;

namespace application {
    public struct RoomDefinition {
        public Rect Area;
        public int ID;
        private static int ids = 0;

        /// <summary>
        /// Creates a Room Definition containing the room area and room GUID 
        /// </summary>
        /// <param name="area">Area of room</param>
        /// <param name="id">ID of room. Should be a GUID!</param>
        public RoomDefinition(Rect area, int id) {
            Area = area;
            ID = id;
        }
        
        public RoomDefinition(Rect area) : this(area, ids++) {}
    }
}