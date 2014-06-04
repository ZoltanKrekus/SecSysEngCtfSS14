namespace SpaceshipAI.VM
{
    internal class SpaceVM
    {
        ////////////////////////////////////////////////////////////////// 
        // VM
        /////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Global memory size
        /// </summary>
        public const int MemorySize = 0x8000000;

        /// <summary>
        /// Memory size per user
        /// </summary>
        public const int VirtualMemorySize = 1024;

        /// <summary>
        /// The global memory
        /// </summary>
        public static readonly byte[] Memory = new byte[MemorySize];
        /////////////////////////////////////////////////////////////////
    }

    public delegate void WriteLineDelegate(string data);
}