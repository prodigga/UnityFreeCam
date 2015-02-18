using UnityEngine;

public abstract class FreeCamInput
{
    public float Sensitivity = 1;

    public abstract Vector2 GetPrimaryDelta();
    public abstract float GetZoomDelta();
    public abstract bool ShouldRotate();
    public abstract bool ShouldDrag();
    public abstract bool ShouldCenterOnTarget();

    public abstract void Update();
}
