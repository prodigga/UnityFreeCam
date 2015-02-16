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
    [Tooltip("Use mouse or touch screen. Automatic will pick the appropriate one by itself (Recommended)")]
    public InputMode Mode;

    public enum InputMode
    {
        Automatic,
        Mouse,
        Touch,
    }

    private Vector3 _targetMovePosition;
    private Vector3 _actualMovePosition;

    private Vector2 _targetLookEuler;
    private Vector2 _actualLookEuler;

    private float _targetZoom;
    private float _actualZoom;

    private FreeCamInput _input;
    private InputMode _currentMode;

    private float _sensitivity = 1;

    void Awake()
    {
        InitializeFreeCamInput();
    }

    void InitializeFreeCamInput()
    {
        _currentMode = Mode;
        InputMode modeToTest = _currentMode;

        if (_currentMode == InputMode.Automatic)
            modeToTest = ResolveAutomaticInputMode();

        switch (modeToTest)
        {
            case InputMode.Mouse:
               _input = new FreeCamMouseInput();
                break;
            case InputMode.Touch:
                _input = new FreeCamTouchInput();
                break; 
        }
    }

    void Update()
    {
        if (_currentMode != Mode || _input == null)
            InitializeFreeCamInput();
        _input.Update();

        //Update the target rotation
        if (_input.ShouldRotate())
        {
            _targetLookEuler.y -= _input.GetPrimaryDelta().x * Time.deltaTime * RotationSensitivity.x * _sensitivity;
            _targetLookEuler.x += _input.GetPrimaryDelta().y * Time.deltaTime * RotationSensitivity.y * _sensitivity;
        }
        if (_targetLookEuler.x > 90) _targetLookEuler.x = 90;
        if (_targetLookEuler.x < -90) _targetLookEuler.x = -90;

        //Update the target position
        if (_input.ShouldDrag())
        {
            float scaleFactor = Mathf.Clamp(Mathf.Abs(_actualZoom)/PositionalZoomScale, 1, float.MaxValue);
            _targetMovePosition += transform.up * _input.GetPrimaryDelta().y * Time.deltaTime * PositionalSensitivity.y * scaleFactor * _sensitivity;
            _targetMovePosition += transform.right * _input.GetPrimaryDelta().x * Time.deltaTime * PositionalSensitivity.x * scaleFactor * _sensitivity;
        }

        //Update the target zoom
        _targetZoom -= _input.GetZoomDelta() * Time.deltaTime * ZoomSensitivity * ZoomFollowCurve.Evaluate((Mathf.Abs(_targetZoom) / ZoomFollowCurveStart));
        _targetZoom = Mathf.Clamp(_targetZoom, MinZoom, MaxZoom);
        
        //When mid mouse is released and if it was released within the 'click' threshold,
        //raycast from the currnet mouse position to try and find an object to center on
        if (_input.ShouldCenterOnTarget())
        {
            var ray = ChildCamera.ScreenPointToRay(Input.mousePosition);
            var hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 1000, CenterClickMask))
            {
                _targetMovePosition = hit.collider.transform.position;
            }
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

    public void SetSensitivity(float Sensitivity)
    {
        if (_currentMode != Mode || _input == null)
            InitializeFreeCamInput();
        _sensitivity = _input.Sensitivity = Sensitivity;
    }

    public static InputMode ResolveAutomaticInputMode()
    {
        if (Application.isEditor)
        {
            return InputMode.Mouse;
        }
        else
        {
#if UNITY_ANDROID || UNITY_IPHONE
            return InputMode.Touch;
#else
            return InputMode.Mouse;
#endif
        }
    }
}