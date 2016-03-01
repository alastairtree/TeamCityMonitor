using System.Collections.Generic;

namespace BuildMonitor.Repository
{
    public static class RepositoryConstants
    {
        public static readonly List<string> IrrelevantProjectIds = new List<string>
        {
            "project149",  // Competition Manager
            "project136",  // Global iPlayer - File Transfer Service
            "project128",  // Global iPlayer - PlayMaker
            "project159",  // Good Food .Net Rewrite
            "project117",  // Magnet
            "project127",  // My Radio Times
            "project21",   // Top Gear
            "project131",  // Top Gear AU
            "project16",   // Windmill Road
        };
    }
}