using System.Runtime.InteropServices;

namespace TestComputeShader
{
    /// <summary>
    /// Constantes pour le compute shader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)] // Indique au service d'interopérabilité de ne pas réarranger la structure
    public struct Constants
    {
        public float alignmentSqrDistance;
        public float maxVelocityMagnitude;
        public float separationSqrDistance;
        public float separationWeight;
        public float sightAngle;
        public float speed;
        public uint totalCount;

        public float padding; // Doit être aligné sur 16 bits

        public static int GetStructSize() => sizeof(float) * 6 + sizeof(uint) * 1 + sizeof(float) * 1;
    }
}
