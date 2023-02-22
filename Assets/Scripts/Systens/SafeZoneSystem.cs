using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct SafeZoneSystem : ISystem
{
    // The ComponentLookup random accessors should not be created on the spot.
    // Just like EntityQuery, they should be created once and stored in a field.
    ComponentLookup<Shooting> m_ShootingLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();

        m_ShootingLookup = state.GetComponentLookup<Shooting>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float radius = SystemAPI.GetSingleton<Config>().SafeZoneRadius;
        const float debugRenderStepInDegrees = 20;

        // Debug rendering (the white circle).
        for (float angle = 0; angle < 360; angle += debugRenderStepInDegrees)
        {
            var a = float3.zero;
            var b = float3.zero;
            math.sincos(math.radians(angle), out a.x, out a.z);
            math.sincos(math.radians(angle + debugRenderStepInDegrees), out b.x, out b.z);
            UnityEngine.Debug.DrawLine(a * radius, b * radius);
        }

        m_ShootingLookup.Update(ref state);
        var safeZoneJob = new SafeZoneJob
        {
            ShootingLookup = m_ShootingLookup,
            SquaredRadius = radius * radius
        };
        safeZoneJob.ScheduleParallel();
    }
}

// Requires the Turret type without processing it (it's not part of the Execute method).
[WithAll(typeof(Turret))]
[BurstCompile]
partial struct SafeZoneJob : IJobEntity, InternalCompilerInterface.IIsFullyUnmanaged
{
    // When running this job in parallel, the safety system will complain about a
    // potential race condition with ShootingLookup because accessing the same entity
    // from different threads would cause problems.
    // But the code of this job is written in such a way that only the entity being currently
    // processed will be looked up in TurretActiveFromEntity, making this process safe.
    // So we can disable the parallel safety check.
    [NativeDisableParallelForRestriction] public ComponentLookup<Shooting> ShootingLookup;

    public float SquaredRadius;

    void Execute(Entity entity, TransformAspect transform)
    {
        // The tag component Shooting will be enabled only if the tank is outside the given range.
        ShootingLookup.SetComponentEnabled(entity, math.lengthsq(transform.Position) > SquaredRadius);
    }
}