using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public sealed class StaticMlpServerPhysicsCubePresenter : MonoBehaviour
    {
        [Header("Hierarchy")] [SerializeField] private Transform viewRoot;
        [SerializeField] private GameObject cubePrefab;

        [Header("Physics")] [SerializeField] private float mass = 1f;
        [SerializeField] private Vector3 spawnImpulse = new(0f, 2f, 4f);
        [SerializeField] private bool showServerPhysicsObjects;
        [SerializeField] private Color fallbackColor = new(0.35f, 0.9f, 0.55f);

        private readonly Dictionary<EntityGID, Rigidbody> _bodies = new();
        private readonly HashSet<EntityGID> _knownBodies = new();
        private readonly List<EntityGID> _deadBodies = new();

        private void Awake()
        {
            if (viewRoot == null)
                viewRoot = transform;
        }

        private void FixedUpdate()
        {
            if (SW.Status != WorldStatus.Initialized)
                return;

            foreach (var e in SW.Query<All<ServerOwned, CubeTag, PhysicsCubeNetState>>().Entities())
            {
                var body = GetOrCreateBody(e);
                ref var state = ref e.Mut<PhysicsCubeNetState>();

                state.Position = body.position;
                state.Velocity = body.linearVelocity;
                state.Rotation = body.rotation;
            }

            CleanupDeadBodies();
        }

        private Rigidbody GetOrCreateBody(SW.Entity e)
        {
            if (_bodies.TryGetValue(e.GID, out var body))
                return body;

            var instance = CreateCubeObject();
            ref readonly var state = ref e.Read<PhysicsCubeNetState>();
            instance.name = "Server Physics Cube";
            instance.transform.SetParent(viewRoot, worldPositionStays: true);
            instance.transform.SetPositionAndRotation(state.Position, state.Rotation);
            SetRenderersVisible(instance, showServerPhysicsObjects);

            body = instance.GetComponent<Rigidbody>();
            if (body == null)
                body = instance.AddComponent<Rigidbody>();

            body.mass = mass;
            body.linearVelocity = state.Velocity;

            if (_knownBodies.Add(e.GID))
                body.AddRelativeForce(spawnImpulse, ForceMode.Impulse);

            _bodies.Add(e.GID, body);
            return body;
        }

        private GameObject CreateCubeObject()
        {
            if (cubePrefab != null)
                return Instantiate(cubePrefab);

            var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.GetComponent<Renderer>().material.color = fallbackColor;
            return primitive;
        }

        private static void SetRenderersVisible(GameObject instance, bool visible)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].enabled = visible;
        }

        private void CleanupDeadBodies()
        {
            _deadBodies.Clear();

            foreach (var pair in _bodies)
            {
                if (!pair.Key.TryUnpack<ServerWT>(out var e) || !e.Has<CubeTag>() || !e.Has<NetworkedTag>())
                    _deadBodies.Add(pair.Key);
            }

            for (var i = 0; i < _deadBodies.Count; i++)
            {
                var gid = _deadBodies[i];
                if (_bodies.TryGetValue(gid, out var body) && body != null)
                    Destroy(body.gameObject);

                _bodies.Remove(gid);
                _knownBodies.Remove(gid);
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in _bodies)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            }

            _bodies.Clear();
            _knownBodies.Clear();
            _deadBodies.Clear();
        }
    }
}
