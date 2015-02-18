using UnityEngine;

public class FreeCamMouseInput : FreeCamInput
{
    private Vector3 _mouseDelta;
    private Vector3 _lastMousePosition;
    private float _middleDownTime;

    private bool _caughtMouseButton1, _caughtMouseButton2;


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
        return _caughtMouseButton1 && !Input.GetMouseButtonDown(1) && Input.GetMouseButton(1);
    }

    public override bool ShouldDrag()
    {
        return _caughtMouseButton2 && !Input.GetMouseButtonDown(2) && Input.GetMouseButton(2);
    }

    public override bool ShouldCenterOnTarget()
    {
        return _caughtMouseButton2 && Input.GetMouseButtonUp(2) && (_middleDownTime < 0.125f);
    }

    public override void Update()
    {
        //If input becomes enabled while the user is dragging the mouse across the screen with MB1 clicked,
        //the camera would start rotating all of a sudden (since MB1 = true). The user should have to release the mouse and re-click 
        //to start interacting with the camera. Thats where this bool comes in to play
        if (Input.GetMouseButtonDown(1))
            _caughtMouseButton1 = true;
        else if (!Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1))
            _caughtMouseButton1 = false;

        if (Input.GetMouseButtonDown(2))
            _caughtMouseButton2 = true;
        else if (!Input.GetMouseButton(2) && !Input.GetMouseButtonUp(2))
            _caughtMouseButton2 = false;

        //Update mouse info
        _mouseDelta = _lastMousePosition - Input.mousePosition;
        _lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(2))
            _middleDownTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(2))
            _middleDownTime = 0;

    }
}
