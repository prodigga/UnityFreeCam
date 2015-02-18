using UnityEngine;

public class FreeCamMouseInput : FreeCamInput
{
    private Vector3 _mouseDelta;
    private Vector3 _lastMousePosition;
    private float _middleDownTime;


    public override Vector2 GetPrimaryDelta()
    {
        return _mouseDelta * Sensitivity;
    }

    public override float GetZoomDelta()
    {
        return Input.mouseScrollDelta.y * Sensitivity;
    }

    public override bool ShouldRotate()
    {
        return !Input.GetMouseButtonDown(1) && Input.GetMouseButton(1);
    }

    public override bool ShouldDrag()
    {
        return !Input.GetMouseButtonDown(2) && Input.GetMouseButton(2);
    }

    public override bool ShouldCenterOnTarget()
    {
        return Input.GetMouseButtonUp(2) && (_middleDownTime < 0.125f);
    }

    public override void Update()
    {
        //Update mouse info
        _mouseDelta = _lastMousePosition - Input.mousePosition;
        _lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(2))
            _middleDownTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(2))
            _middleDownTime = 0;
    }
}
