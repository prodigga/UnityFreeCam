using UnityEngine;

public class FreeCam : MonoBehaviour
{
    [Header("Position")]
    [Tooltip("How quickly the camera will move left and right (X) and up and down (Y)")]
    public Vector2 PositionalSensitivity;
    [Tooltip("How quickly the camera will actually move towards the new position")]
    public float PositionalFollowSpeed;
    [Tooltip("How many units away from the target position must we be before we start scaling the follow speed by the curve")]
    public float PositionalFollowCurveStart;
    [Tooltip("Scale the follow speed by this curve once we are PositionalFollowCurveStart units away from the target position")]
    public AnimationCurve PositionalFollowCurve;
    [Tooltip("Scale the above values uniformally once the zoom level is passed this point (ie, a value of 5 will mean when we are at zoom value 10, all the values above will be multipled by 2)")]
    public float PositionalZoomScale;

    [Header("Rotation")]
    [Tooltip("How quickly the camera will rotate left and right (X) and up and down (Y)")]
    public Vector2 RotationSensitivity;
    [Tooltip("How quickly the camera will actually rotate towards the new rotation")]
    public float RotationalFollowSpeed;
    [Tooltip("How many degrees away from the target rotation must we be before we start scaling the follow speed by the curve")]
    public float RotationalFollowCurveStart;
    [Tooltip("Scale the follow speed by this curve once we are RotationalFollowCurveStart degrees away from the target rotation")]
    public AnimationCurve RotationalFollowCurve;

    [Header("Zoom")]
    [Tooltip("Scrollwheel delta * this value")]
    public float ZoomSensitivity;
    [Tooltip("How quickly the camera will actually match its zoom value with the target zoom")]
    public float ZoomFollowSpeed;
    [Tooltip("(Target zoom / this value) will be used to sample the curve bellow and the result will be used to scale ZoomFollowSpeed")]
    public float ZoomFollowCurveStart;
    [Tooltip("See ZoomFollowCurveStart")]
    public AnimationCurve ZoomFollowCurve;

    [Space(10)] 
    public float MinZoom;
    public float MaxZoom;

    [Header("Misc")]
    [Tooltip("A GameObject with a camera attached should be childed to GameObject that this script is attached to.")]
    public Camera ChildCamera;
    [Tooltip("When the use middle mouse clicks on an object in the game, we will only center on it if it is on this layer.")]
    public LayerMask CenterClickMask;

    private Vector3 _targetMovePosition;
    private Vector3 _actualMovePosition;

    private Vector2 _targetLookEuler;
    private Vector2 _actualLookEuler;

    private float _targetZoom;
    private float _actualZoom;

    private Vector3 _mouseDelta;
    private Vector3 _lastMousePosition;
    private float _middleDownTime;

    void Update()
    {
        //Update mouse info
        _mouseDelta = _lastMousePosition - Input.mousePosition;
        _lastMousePosition = Input.mousePosition;

        //Update the target rotation
        if (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1))
        {
            _targetLookEuler.y -= _mouseDelta.x * Time.deltaTime * RotationSensitivity.x;
            _targetLookEuler.x += _mouseDelta.y * Time.deltaTime * RotationSensitivity.y;
        }
        if (_targetLookEuler.x > 90) _targetLookEuler.x = 90;
        if (_targetLookEuler.x < -90) _targetLookEuler.x = -90;

        //Update the target position
        if (!Input.GetMouseButtonDown(2) && Input.GetMouseButton(2))
        {
            float scaleFactor = Mathf.Clamp(Mathf.Abs(_actualZoom)/PositionalZoomScale, 1, float.MaxValue);
            _targetMovePosition += transform.up * _mouseDelta.y * Time.deltaTime * PositionalSensitivity.y * scaleFactor;
            _targetMovePosition += transform.right * _mouseDelta.x * Time.deltaTime * PositionalSensitivity.x * scaleFactor;
        }

        //Update the target zoom
        _targetZoom -= Input.mouseScrollDelta.y * Time.deltaTime * ZoomSensitivity * ZoomFollowCurve.Evaluate((Mathf.Abs(_targetZoom) / ZoomFollowCurveStart));
        _targetZoom = Mathf.Clamp(_targetZoom, MinZoom, MaxZoom);

        //Count how long middle mouse has been held for (So we can determine whether it was a click or drag)
        if (Input.GetMouseButton(2))
            _middleDownTime += Time.deltaTime;

        //When mid mouse is released and if it was released within the 'click' threshold,
        //raycast from the currnet mouse position to try and find an object to center on
        if (Input.GetMouseButtonUp(2))
        {
            if (_middleDownTime < 0.125f)
            {
                var ray = ChildCamera.ScreenPointToRay(Input.mousePosition);
                var hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, 1000, CenterClickMask))
                {
                    _targetMovePosition = hit.collider.transform.position;
                }
            }

            //Reset midmouse held down timer
            _middleDownTime = 0;
        }
    }
    
    void LateUpdate()
    {
        //Tween rotation
        _actualLookEuler = Vector3.MoveTowards(_actualLookEuler, _targetLookEuler,
            RotationalFollowSpeed *
            RotationalFollowCurve.Evaluate((Vector3.Distance(_actualLookEuler, _targetLookEuler) / RotationalFollowCurveStart)) *
            Time.deltaTime);

        transform.rotation = Quaternion.Euler(_actualLookEuler);

        //Tween position
        float scaleFactor = Mathf.Clamp(Mathf.Abs(_actualZoom)/PositionalZoomScale, 1, float.MaxValue);
        _actualMovePosition = Vector3.MoveTowards(_actualMovePosition, _targetMovePosition,
            PositionalFollowSpeed *
            PositionalFollowCurve.Evaluate((Vector3.Distance(_actualMovePosition, _targetMovePosition) / PositionalFollowCurveStart)) *
            Time.deltaTime * scaleFactor);

        transform.position = _actualMovePosition;

        //Tween zoom
        _actualZoom = Mathf.MoveTowards(_actualZoom, _targetZoom,
            ZoomFollowSpeed * ZoomFollowCurve.Evaluate((Mathf.Abs(_targetZoom) / ZoomFollowCurveStart)) *
            Time.deltaTime);

        ChildCamera.transform.localPosition = new Vector3(0, 0, -_actualZoom);

    }

    /// <summary>
    /// Snaps to position
    /// </summary>
    public void SetPosition(Vector3 pos)
    {
        _targetMovePosition = _actualMovePosition = pos;
    }

    /// <summary>
    /// Snaps to rotation
    /// </summary>
    public void SetRotation(Quaternion rot)
    {
        _targetLookEuler = _actualLookEuler = rot.eulerAngles;
    }

    /// <summary>
    /// Snaps to zoom
    /// </summary>
    public void SetZoom(float zoom)
    {
        _targetZoom = _actualZoom = zoom;
    }

    /// <summary>
    /// Smoothly moves to position
    /// </summary>
    public void SetSmoothPosition(Vector3 pos)
    {
        _targetMovePosition = pos;
    }

    /// <summary>
    /// Smoothly rotates to rotation
    /// </summary>
    public void SetSmoothRotation(Quaternion rot)
    {
        _targetLookEuler = rot.eulerAngles;
    }

    /// <summary>
    /// Smoothly zooms to zoom
    /// </summary>
    public void SetSmoothZoom(float zoom)
    {
        _targetZoom =  zoom;
    }
}
