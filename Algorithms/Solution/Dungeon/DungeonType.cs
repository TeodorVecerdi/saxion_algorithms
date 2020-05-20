using System;

namespace application {
    [Flags]
    public enum DungeonType : byte {
        Sufficient = 1,
        Good = 2,
        Excellent = 4
    }
}