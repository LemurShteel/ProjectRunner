using UnityEngine;

public class KinematicMovement2D : MonoBehaviour
{

    [SerializeField] private float _shellRadius = 0.02f;
    [SerializeField] private bool _useRicochet;
    [SerializeField] [Range(0, 1)] private float _bounciness;
    [SerializeField] private bool _useSlope;
    [SerializeField] [Range(0, 90)] private float _slopeLimit;
    [SerializeField] public ContactFilter2D _collisionsFilter;

    public bool IsGrounded { get; private set; }

    private Rigidbody2D _rb2d;
    private readonly RaycastHit2D[] _raycast2d = new RaycastHit2D[8];

    private float _testAngle;

    private Vector2 _newPositionCheck;
    private Vector2 _newtPosition;

    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// FixedDeltaTime уже проверяется внутри. Нет необходимости его сюда давать. Используй чистое значение скорости. А на гравитацию накладывай, это дополнительное ускорение, которое идет сверху.
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>

    public Vector2 MoveBody(Vector2 velocity)
    {

        IsGrounded = false;

        CalculateNewPosition(ref velocity);
        
        //if (minMoveDistance <= nextPosition.magnitude)
        _rb2d.MovePosition(_rb2d.position + _newtPosition);

        return velocity;
    }

    private void CalculateNewPosition(ref Vector2 velocity)
    {
        if (_useRicochet)
            _newtPosition = Ricochet(ref velocity);
        else
        {
            _newtPosition = CollisionCheck(ref velocity.x, true);
            _newtPosition += CollisionCheck(ref velocity.y, false);
        }
    }

    private Vector2 CollisionCheck(ref float velocityValue, bool isHorizontal)
    {
        _newPositionCheck = velocityValue * (isHorizontal ? Vector2.right : Vector2.up);

        //Кастуем в этом направлении проверяемое значение.
        if (_rb2d.Cast(_newPositionCheck, _collisionsFilter, _raycast2d, Mathf.Abs(velocityValue) * Time.fixedDeltaTime + _shellRadius) > 0) //Значит имеется столновение с кастом в том числе благодаря шеллу.
        {
            foreach (RaycastHit2D hit in _raycast2d)
            {
                if (_useSlope)
                {
                    _testAngle = Vector2.SignedAngle(Vector2.up, hit.normal);
                    if (Mathf.Abs(_testAngle) <= _slopeLimit)
                    {
                        IsGrounded = true;
                        if (hit.distance != 0 && isHorizontal)
                        {
                            _newPositionCheck = RotateToAngle(_newPositionCheck, _testAngle);
                            break;
                        }
                    }
                }
                else if (Vector2.Dot(Vector2.down, _newPositionCheck.normalized) > 0.5) IsGrounded = true;

                //Если рейкаст больше чем шелл радиус, значит произойдет столкновениие с объектом с "проникновением".
                if (System.Math.Round(hit.distance, 2) >= _shellRadius)
                {
                    velocityValue = velocityValue * -_bounciness;
                    _newPositionCheck = (hit.distance - _shellRadius) * _newPositionCheck.normalized; 
                    //Ограничиваем движение и аккуратно приближаемся к объекту. 
                    //Можно и толкание, но надо ли оно нам.
                    return _newPositionCheck;
                }
                else //В обратном случае у нас произошло столкновение, но меньше чем шелл радиус.
                {
                    velocityValue = velocityValue * -_bounciness;
                    _newPositionCheck = hit.normal * (hit.distance - _shellRadius); 
                    break;
                }
            }
        }
        else if (_useSlope && isHorizontal) //Если у нас горизонтальное движение и нужно проверить слоп
        {//Делаем проверку на то, чо находится под ногами !!!!! у персонажа. 
            if (_rb2d.Cast(Vector2.down, _collisionsFilter, _raycast2d, Mathf.Abs(velocityValue) * Time.deltaTime + _shellRadius) > 0)
            {
                foreach (RaycastHit2D ray in _raycast2d)
                {
                    if (ray.distance != 0) //Шелл тоже надо использовать
                    {
                        _testAngle = Vector2.SignedAngle(Vector2.up, ray.normal);
                        if (Mathf.Abs(_testAngle) <= _slopeLimit)
                        {
                            IsGrounded = true;
                            _newPositionCheck = RotateToAngle(_newPositionCheck, _testAngle);

                            //Можно добавить дополнительную проверку.
                            //Каст по направлению. Если произошла коллизия, то ничего не делать, возвращать зеро.
                            break;
                        }
                    }
                }
            }
        }
        return _newPositionCheck * Time.fixedDeltaTime;
    }

    private Vector2 Ricochet(ref Vector2 testVelocity) //Это можно вынести и реализовать через наследование, не все объекты будут рикошетить.
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


    //Хелперы.

    private Vector2 RotateToAngle(Vector2 vector, float angle)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle) * vector.x - Mathf.Sin(Mathf.Deg2Rad * angle) * vector.y, 
            Mathf.Sin(Mathf.Deg2Rad * angle) * vector.x + Mathf.Cos(Mathf.Deg2Rad * angle) * vector.y);
    }



}



