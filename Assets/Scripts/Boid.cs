using UnityEngine;

namespace TestComputeShader
{
    public sealed class Boid : MonoBehaviour
    {
        /// <summary>
        /// Met à jour le boid à partir des données calculées par le compute shader
        /// </summary>
        /// <param name="data">Les données à utiliser</param>
        public void SetData(BoidData data)
        {
            this.transform.position = data.position;
            this.transform.rotation = Quaternion.LookRotation(data.velocity);
        }
    }
}
