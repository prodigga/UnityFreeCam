using UnityEngine;

public class FreeCamTouchInput : FreeCamInput
{
    private class TouchDetails
    {
        public bool HasValidTouch
        {
            get { return TouchID != -1; }
        }

        public int TouchID = -1;
        public Vector2 StartPosition;
        public Vector2 CurrentPosition;
        public Vector2 PrevPosition;
        public Vector2 Delta;
        public float DownTime;
        public bool WasDown, WasUp;

        public void SetTouch(Touch t)
        {
            TouchID = t.fingerId;
            StartPosition = CurrentPosition = PrevPosition = t.position;
            Delta = Vector3.zero;
            DownTime = 0;
            WasDown = true;
        }

        public void UpdateTouch(Touch t)
        {
            PrevPosition = CurrentPosition;
            CurrentPosition = t.position;
            Delta = t.deltaPosition;
            DownTime += Time.deltaTime;
        }

        public void RemoveTouch(Touch t)
        {
            TouchID = -1;
            WasUp = true;
        }

        public void ResetUpDown()
        {
            WasUp = WasDown = false;
        }
    }

    private TouchDetails
        _activeFinger = new TouchDetails(),
        _activeSecondFinger = new TouchDetails();

    private Vector3 _mouseDelta;
    private Vector3 _lastMousePosition;
    private float _timeSinceLastTouch;

    public override Vector2 GetPrimaryDelta()
    {
        //invert the delta, coz thats how mobile do
        return _activeFinger.HasValidTouch ? -_activeFinger.Delta * Sensitivity : Vector2.zero;
    }

    public override float GetZoomDelta()
    {
        //no zoom :( 
        return 0 * Sensitivity;
    }

    public override bool ShouldRotate()
    {
        return _activeFinger.HasValidTouch && !_activeSecondFinger.HasValidTouch;
    }

    public override bool ShouldDrag()
    {
        return _activeFinger.HasValidTouch && _activeSecondFinger.HasValidTouch;
    }

    public override bool ShouldCenterOnTarget()
    {
        return (_timeSinceLastTouch < 0.25f) && (_activeFinger.DownTime < 0.125f) && _activeFinger.WasUp;
    }

    public override void Update()
    {
        //we reset the _timeSinceLastTouch here instead of bellow when the 
        //finger actually gets released because it would be reseting too early at that point
        //we need to perform a whole update loop where _timeSinceLastTouch is still valid from the previous touch
        //and _activeFinger is still in 'WasUp' (released) mode. This is all so ShouldCenterOnTarget can work properly
        if (_activeFinger.WasUp)
            _timeSinceLastTouch = 0;

        _activeFinger.ResetUpDown();
        _activeSecondFinger.ResetUpDown();
        _timeSinceLastTouch += Time.deltaTime;

        foreach (var touch in Input.touches)
        {
            if (_activeFinger.TouchID == touch.fingerId)
            {
                _activeFinger.UpdateTouch(touch);
            }
            else if (_activeSecondFinger.TouchID == touch.fingerId)
            {
                _activeSecondFinger.UpdateTouch(touch);
            }

            if (_activeFinger.TouchID == touch.fingerId && touch.phase == TouchPhase.Ended)
            {
                _activeFinger.RemoveTouch(touch);
            }
            else if (_activeSecondFinger.TouchID == touch.fingerId && touch.phase == TouchPhase.Ended)
            {
                _activeSecondFinger.RemoveTouch(touch);
            }

            if (!_activeFinger.HasValidTouch && touch.phase == TouchPhase.Began)
            {
                _activeFinger.SetTouch(touch);
            }
            else if (!_activeSecondFinger.HasValidTouch && touch.phase == TouchPhase.Began)
            {
                _activeSecondFinger.SetTouch(touch);
            }
        }
    }
}