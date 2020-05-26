using System;

namespace application {
    [Flags]
    public enum DungeonType {
        None = 0,
        Sufficient = 1,
        Good = 2,
        Excellent = 4
    }
}