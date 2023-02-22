using Unity.Entities;

// Authoring MonoBehaviours are regular GameObject components.
// They constitute the inputs for the baking systems which generates ECS data.
class TurretAuthoring : UnityEngine.MonoBehaviour
{
    // Bakers convert authoring MonoBehaviours into entities and components.
    // (Nesting a baker in its associated Authoring component is not necessary but is a common convention.)
    class TurretBaker : Baker<TurretAuthoring>
    {
        public override void Bake(TurretAuthoring authoring)
        {
            AddComponent<Turret>();
        }
    }
}

// An ECS component.
// An empty component is called a "tag component".
struct Turret : IComponentData
{
}