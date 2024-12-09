using UmfEngine;

namespace UMFE.Framework
{
    // We'll want to add a new feature of colliders, ICollider.
    // The collider will have a reference to the ITransform, and will come in a few varieties, like CircleCollider, CapsuleCollider, LineCollider,
    // RectCollider, PlaneCollider...
    //
    // We'll also want raycasting, I think
    //
    // Finally, we can have a new iterface, IColliderGameObject : ICollider, IGameObject
    public interface IGameObject
    {
        GameObjectTransform Transform { get; }

        void Update(Engine e, Camera c);
        void Draw(Engine e, Camera c);
    }
}
