using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FreeCam))]
public class FreeCamInitPosRotZoom : MonoBehaviour
{
    [Header("Initial Values")]
    public Vector3 Position;
    public Vector3 RotationEuler;
    public float Zoom;

    void Awake()
    {
        FreeCam fc = GetComponent<FreeCam>();
        fc.SetPosition(Position);
        fc.SetRotation(Quaternion.Euler(RotationEuler));
        fc.SetZoom(Zoom);
    }
}
