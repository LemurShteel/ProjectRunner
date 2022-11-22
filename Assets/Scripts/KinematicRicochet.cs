using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicRicochet : KinematicMovement2D
{
    [SerializeField] protected bool _useRicochet;

    protected override void CalculateNewPosition(ref Vector2 velocity)
    {
        if (_useRicochet)
            _newtPosition = Ricochet(ref velocity);
        else
            base.CalculateNewPosition(ref velocity);
    }

    protected virtual Vector2 Ricochet(ref Vector2 testVelocity)
    {
        //Кастуем в этом направлении проверяемое значение на следующий кадр.
        if (_rb2d.Cast(testVelocity, _collisionsFilter, _raycast2d, testVelocity.magnitude * Time.fixedDeltaTime + _shellRadius) > 0) //Значит имеется столновение с кастом.
        {
            foreach (RaycastHit2D hit2D in _raycast2d)
            {
                //если рейкаст дистанс меньше шелл радиуса, значит объект находится внутри шелл радиуса
                if (hit2D.distance >= _shellRadius)
                {
                    _testAngle = Vector2.SignedAngle(-testVelocity.normalized, hit2D.normal);
                    _newPositionCheck = (hit2D.distance - _shellRadius) * testVelocity.normalized;
                    testVelocity = RotateToAngle(testVelocity, _testAngle * 2) * -_bounciness;
                    return _newPositionCheck;
                }
                else
                {
                    _newPositionCheck = hit2D.normal * -(hit2D.distance - _shellRadius);
                    return _newPositionCheck;
                }
            }
        }
        return testVelocity * Time.fixedDeltaTime;
    }
}
