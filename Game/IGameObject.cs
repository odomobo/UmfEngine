using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmfEngine;

namespace Game
{
    // TODO: restructure game objects to work as follows:
    // First, rename "Transform" to "CameraViewport" or something. "CameraViewport" controls will need to be opposite -
    // rotation will need to rotate objects the other way, instead of "Scale" it will be called "Zoom", etc.
    // In order to do this, I'll need to refactor the current methods to "Inverse" methods, which call the normal (new)
    // methods, and then update all calls of them to work in the opposite way.
    //
    // Next, every game object of course still has Update() and Draw(), but -
    // Now they also have a (new) "ITransform", which has position, scale, and rotation. It can be in 1 of 2
    // forms: either AbsoluteTransform or RelativeTransform. A RelativeTransform has a reference to its parent
    // transform, and is automatically carried along by it.
    // Or maybe, there's just 1 type (Transform) which will work in 1 of two ways depending on whether or not it has a parent. I like that.
    // Transforms will have methods like Scale(), Translate(), Rotate(), which are all relative, but also SetAbsoluteScale(),
    // SetRelativeScale(), etc. We could also add TranslateAbsolute(), but that is almost certainly not needed.
    //
    // In addition to this, we'll want to add a new feature of colliders, ICollider.
    // The collider will have a reference to the ITransform, and will come in a few varieties, like CircleCollider, CapsuleCollider, LineCollider,
    // RectCollider, PlaneCollider...
    //
    // Finally, we can have a new iterface, IColliderGameObject : ICollider, IGameObject
    internal interface IGameObject
    {
        void Update(Engine e, CameraViewport t);
        void Draw(Engine e, CameraViewport t);
    }
}
