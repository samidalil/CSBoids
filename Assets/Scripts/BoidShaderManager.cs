using Unity.Collections;
using UnityEngine;

namespace TestComputeShader
{
    /// <summary>
    /// Classe de gestion des instances de Compute Shader pour les boids
    /// </summary>
    public sealed class BoidShaderManager : MonoBehaviour
    {
        #region Constantes

        private const float THREADS_PER_GROUP = 256f; // float pour la division

        #endregion

        #region Champs Unity

        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private ComputeShader _shader;

        [SerializeField, Min(0)]
        private int _count;

        [Header("Paramètres des boids")]

        [SerializeField, Min(0)]
        private float _alignmentDistance;

        [SerializeField, Min(0)]
        private float _maxVelocityMagnitude;

        [SerializeField, Min(0)]
        [Tooltip("Doit être strictement inférieure à la distance d'alignement")]
        private float _separationDistance;

        [SerializeField, Min(0)]
        private float _separationWeight;

        [SerializeField, Range(0, 180)]
        private float _sightAngle;

        [SerializeField, Min(0)]
        private float _speed;

        #endregion

        #region Champs privés

        private BoidData[] _data;

        private Boid[] _instances;

        private ComputeBuffer _shaderComputeBuffer;

        private ComputeBuffer _shaderConstantBuffer;

        private NativeArray<Constants> _shaderConstantData;

        private int _shaderDeltaTimeProperty;

        private int _shaderKernelId;

        private int _threadsGroupCount;

        #endregion

        #region Routines Unity

        /// <summary>
        /// Se lance au démarrage du script
        /// </summary>
        private void Awake()
        {
            // Précalcul du nombre de groupes de threads pour le compute shader
            this._threadsGroupCount = Mathf.CeilToInt(this._count / BoidShaderManager.THREADS_PER_GROUP);

            this.InitializeBoidData();
            this.InitializeBoids();
            this.InitializeConstantBuffer();
            this.InitializeComputeShader();
        }

        /// <summary>
        /// Se lance sur la destruction du script
        /// </summary>
        private void OnDestroy()
        {
            this._shaderComputeBuffer.Release();
            this._shaderConstantBuffer.Release();
            this._shaderConstantData.Dispose();
        }

        /// <summary>
        /// Se lance lors de la modification d'une valeur dans l'inspecteur Unity
        /// </summary>
        private void OnValidate()
        {
            if (Application.IsPlaying(this) && this._shaderConstantData.Length == 1)
            {
                this._shaderConstantData[0] = new Constants
                {
                    alignmentSqrDistance = this._alignmentDistance,
                    maxVelocityMagnitude = this._maxVelocityMagnitude,
                    separationSqrDistance = this._separationDistance,
                    separationWeight = this._separationWeight,
                    sightAngle = this._sightAngle,
                    speed = this._speed,
                    totalCount = this._shaderConstantData[0].totalCount, // Pas de changement de nombre d'instances pour l'instant
                };
                this._shaderConstantBuffer.SetData(this._shaderConstantData);
            }
        }

        /// <summary>
        /// Se lance sur une tick visuelle
        /// </summary>
        private void Update()
        {
            this._shader.SetFloat(this._shaderDeltaTimeProperty, Time.deltaTime);
            this._shader.Dispatch(this._shaderKernelId, this._threadsGroupCount, 1, 1);
            this._shaderComputeBuffer.GetData(this._data); // Très coûteux

            for (int i = 0; i < this._count; ++i)
                this._instances[i].SetData(this._data[i]);
        }

        #endregion

        #region Méthodes privées

        /// <summary>
        /// Initialise le ComputeBuffer contenant les structures de données des Boids
        /// </summary>
        private void InitializeBoidData()
        {
            this._data = new BoidData[this._count];

            for (int i = 0; i < this._count; ++i)
            {
                BoidData elem = new BoidData
                {
                    position = Random.insideUnitSphere * 50,
                    velocity = Random.onUnitSphere
                };

                this._data[i] = elem;
            }

            this._shaderComputeBuffer = new ComputeBuffer(this._count, BoidData.GetStructSize());
            this._shaderComputeBuffer.SetData(this._data);
        }

        /// <summary>
        /// Instancie tous les boids demandés avec les données voulues
        /// </summary>
        private void InitializeBoids()
        {
            this._instances = new Boid[this._count];

            for (int i = 0; i < this._count; ++i)
            {
                this._instances[i] = GameObject.Instantiate(this._prefab).GetComponent<Boid>();
                this._instances[i].SetData(this._data[i]);
            }
        }

        /// <summary>
        /// Récupère l'ID du kernel à exécuter et assigne le ComputeBuffer pré-initialisé
        /// </summary>
        private void InitializeComputeShader()
        {
            this._shaderKernelId = this._shader.FindKernel("UpdateBoid");
            this._shaderDeltaTimeProperty = Shader.PropertyToID("deltaTime");

            this._shader.SetBuffer(this._shaderKernelId, "boids", this._shaderComputeBuffer);
            this._shader.SetConstantBuffer("Constants", this._shaderConstantBuffer, 0, Constants.GetStructSize());
        }

        /// <summary>
        /// Initialise le buffer de données constantes contenant les paramètres des boids
        /// </summary>
        private void InitializeConstantBuffer()
        {
            this._shaderConstantData = new NativeArray<Constants>(1, Allocator.Persistent);
            this._shaderConstantData[0] = new Constants
            {
                alignmentSqrDistance = this._alignmentDistance * this._alignmentDistance,
                maxVelocityMagnitude = this._maxVelocityMagnitude,
                separationSqrDistance = this._separationDistance * this._separationDistance,
                separationWeight = this._separationWeight,
                sightAngle = this._sightAngle,
                speed = this._speed,
                totalCount = (uint)this._count,
            };

            this._shaderConstantBuffer = new ComputeBuffer(1, Constants.GetStructSize(), ComputeBufferType.Constant);
            this._shaderConstantBuffer.SetData(this._shaderConstantData);
        }

        #endregion
    }
}
