using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionPlacementValidator
    {
        private const float GroundProbeHeight = 12f;
        private const float GroundProbeDistance = 24f;
        private const float GroundTolerance = 0.35f;
        private const float MaxSlopeDegrees = 35f;

        public static PlacementValidationResult ValidateClient(
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            return Validate(definition, position, rotation, ValidateClientWorld);
        }

        public static PlacementValidationResult ValidateAuthoritative(
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            return Validate(definition, position, rotation, ValidateAuthoritativeWorld);
        }

        private static PlacementValidationResult Validate(
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Func<OrientedFootprint, PlacementValidationResult> validateWorld)
        {
            var ground = ValidateGround(position);
            if (!ground.IsValid)
                return ground;

            var footprint = CreateFootprint(position, rotation, definition.FootprintWidth, definition.FootprintLength);
            return validateWorld(footprint);
        }

        private static PlacementValidationResult ValidateClientWorld(OrientedFootprint footprint)
        {
            foreach (var e in CW.Query<All<BuildingFootprint, ConstructionTransform>>().Entities())
            {
                var otherFootprint = e.Read<BuildingFootprint>();
                var otherTransform = e.Read<ConstructionTransform>();
                if (Overlaps(
                        footprint,
                        CreateFootprint(
                            otherTransform.Position,
                            otherTransform.Rotation,
                            otherFootprint.Width,
                            otherFootprint.Length)))
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.Occupied);
            }

            foreach (var e in CW.Query<All<RestrictedBuildZone>>().Entities())
            {
                ref readonly var zone = ref e.Read<RestrictedBuildZone>();
                if (IntersectsCircle(footprint, zone.Center, zone.Radius))
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.RestrictedZone);
            }

            return PlacementValidationResult.Valid();
        }

        private static PlacementValidationResult ValidateAuthoritativeWorld(OrientedFootprint footprint)
        {
            foreach (var e in SW.Query<All<BuildingFootprint, ConstructionTransform>>().Entities())
            {
                var otherFootprint = e.Read<BuildingFootprint>();
                var otherTransform = e.Read<ConstructionTransform>();
                if (Overlaps(
                        footprint,
                        CreateFootprint(
                            otherTransform.Position,
                            otherTransform.Rotation,
                            otherFootprint.Width,
                            otherFootprint.Length)))
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.Occupied);
            }

            foreach (var e in SW.Query<All<RestrictedBuildZone>>().Entities())
            {
                ref readonly var zone = ref e.Read<RestrictedBuildZone>();
                if (IntersectsCircle(footprint, zone.Center, zone.Radius))
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.RestrictedZone);
            }

            return PlacementValidationResult.Valid();
        }

        private static PlacementValidationResult ValidateGround(Vector3 position)
        {
            var origin = position + Vector3.up * GroundProbeHeight;
            if (Physics.Raycast(origin, Vector3.down, out var hit, GroundProbeDistance))
            {
                if (Mathf.Abs(hit.point.y - position.y) > GroundTolerance)
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.OffGround);

                if (Vector3.Angle(hit.normal, Vector3.up) > MaxSlopeDegrees)
                    return PlacementValidationResult.Invalid(PlacementInvalidReason.SlopeTooSteep);
            }
            else if (Mathf.Abs(position.y) > GroundTolerance)
            {
                return PlacementValidationResult.Invalid(PlacementInvalidReason.OffGround);
            }

            return PlacementValidationResult.Valid();
        }

        private static OrientedFootprint CreateFootprint(
            Vector3 position,
            Quaternion rotation,
            float width,
            float length)
        {
            var yaw = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            var right3 = yaw * Vector3.right;
            var forward3 = yaw * Vector3.forward;

            return new OrientedFootprint(
                new Vector2(position.x, position.z),
                new Vector2(right3.x, right3.z).normalized,
                new Vector2(forward3.x, forward3.z).normalized,
                Mathf.Max(0.05f, width * 0.5f),
                Mathf.Max(0.05f, length * 0.5f));
        }

        private static bool Overlaps(in OrientedFootprint a, in OrientedFootprint b)
        {
            return OverlapsOnAxis(a, b, a.Right)
                   && OverlapsOnAxis(a, b, a.Forward)
                   && OverlapsOnAxis(a, b, b.Right)
                   && OverlapsOnAxis(a, b, b.Forward);
        }

        private static bool OverlapsOnAxis(in OrientedFootprint a, in OrientedFootprint b, Vector2 axis)
        {
            var distance = Mathf.Abs(Vector2.Dot(b.Center - a.Center, axis));
            var radiusA = ProjectedRadius(a, axis);
            var radiusB = ProjectedRadius(b, axis);
            return distance <= radiusA + radiusB;
        }

        private static float ProjectedRadius(in OrientedFootprint footprint, Vector2 axis)
        {
            return Mathf.Abs(Vector2.Dot(footprint.Right, axis)) * footprint.HalfWidth
                   + Mathf.Abs(Vector2.Dot(footprint.Forward, axis)) * footprint.HalfLength;
        }

        private static bool IntersectsCircle(in OrientedFootprint footprint, Vector3 center, float radius)
        {
            var circleCenter = new Vector2(center.x, center.z);
            var local = circleCenter - footprint.Center;
            var x = Mathf.Clamp(Vector2.Dot(local, footprint.Right), -footprint.HalfWidth, footprint.HalfWidth);
            var z = Mathf.Clamp(Vector2.Dot(local, footprint.Forward), -footprint.HalfLength, footprint.HalfLength);
            var closest = footprint.Center + footprint.Right * x + footprint.Forward * z;
            return (circleCenter - closest).sqrMagnitude <= radius * radius;
        }

        private readonly struct OrientedFootprint
        {
            public readonly Vector2 Center;
            public readonly Vector2 Right;
            public readonly Vector2 Forward;
            public readonly float HalfWidth;
            public readonly float HalfLength;

            public OrientedFootprint(Vector2 center, Vector2 right, Vector2 forward, float halfWidth, float halfLength)
            {
                Center = center;
                Right = right;
                Forward = forward;
                HalfWidth = halfWidth;
                HalfLength = halfLength;
            }
        }
    }
}
