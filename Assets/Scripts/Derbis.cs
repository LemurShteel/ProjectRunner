using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Derbis : MonoBehaviour
{
    private KinematicMovement2D _km2d;
    [SerializeField] private Vector2 _velocity;
    private void Awake()
    {
        _km2d = GetComponent<KinematicMovement2D>();
    }

    private void FixedUpdate()
    {
        _velocity += Physics2D.gravity * Time.fixedDeltaTime;
        _velocity = _km2d.MoveBody(_velocity);
    }
}
