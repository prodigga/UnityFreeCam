using UnityEngine;

public class FreeCamDisabledInput : FreeCamInput
{
    public override Vector2 GetPrimaryDelta() { return Vector2.zero; }

    public override float GetZoomDelta() { return 0; }

    public override bool ShouldRotate() { return false; }

    public override bool ShouldDrag() { return false; }
    
    public override bool ShouldCenterOnTarget() { return false; }

    public override void Update() { }
}
