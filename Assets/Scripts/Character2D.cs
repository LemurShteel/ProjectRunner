using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character2D : MonoBehaviour
{
    [SerializeField] private bool _useControls;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _speed;

    private Vector2 _velocity;
    private KinematicMovement2D _km2d;
    private Vector2 _input;

    private void Awake()
    {
        _km2d = GetComponent<KinematicMovement2D>();
    }

    public void DamagePlayer()
    {
        Debug.Log("Oh no! Player Damaged!");
    }
    void Update()
    {
        if (_useControls)
        {
            _input.x = Input.GetAxis("Horizontal");
            _input.y = Input.GetAxis("Vertical");
        }
    }

    private void FixedUpdate()
    {
        if (_km2d.IsGrounded && _input.y > 0) //При отсутствии вертикального инпута можно сбрасывать вертикальную скорость кстати, будет такой контроль высоты прыжка.
        {
            _velocity += Vector2.up * _jumpForce;
        }
        _velocity.x = Mathf.MoveTowards(_velocity.x, _input.x * _speed, Time.fixedDeltaTime * 3);
        ApplyGravity();
        _velocity = _km2d.MoveBody(_velocity);
    }

    private void ApplyGravity()
    {
        _velocity += Physics2D.gravity * Time.fixedDeltaTime;
    }
}
