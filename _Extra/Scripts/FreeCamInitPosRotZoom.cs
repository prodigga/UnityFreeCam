using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FreeCam))]
public class FreeCamInitPosRotZoom : MonoBehaviour
{
    [Header("Initial Values")]
    public Vector3 Position;
    public Vector3 RotationEuler;
    public float Zoom;
    public float SensitivityMouse = 1;
    public float SensitivityTouch = 3;

    void Awake()
    {
        FreeCam fc = GetComponent<FreeCam>();
        fc.SetPosition(Position);
        fc.SetRotation(Quaternion.Euler(RotationEuler));
        fc.SetZoom(Zoom);

        FreeCam.InputMode IM = fc.Mode;
        if(fc.Mode == FreeCam.InputMode.Automatic)
            IM = FreeCam.ResolveAutomaticInputMode();

        fc.SetSensitivity(IM == FreeCam.InputMode.Mouse ? SensitivityMouse : SensitivityTouch);
    }
}
