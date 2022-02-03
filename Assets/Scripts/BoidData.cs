using UnityEngine;

namespace TestComputeShader
{
    /// <summary>
    /// Représente les données d'un boid échangées avec le compute shader
    /// </summary>
    public struct BoidData
    {
        public Vector3 position;
        public Vector3 velocity;

        public static int GetStructSize() => sizeof(float) * 3 * 2;
    }
}
