using UnityEngine;

public class KinematicMovement2D : MonoBehaviour
{

    [SerializeField] protected float _shellRadius = 0.02f;
    [SerializeField] [Range(0, 1.015f)] protected float _bounciness; 
    //Интересная ситуация. Для сохранения инерации значение 1.015 подохдит лучше всего. Проблема в применении гравитации, которая перебивает скорость и сводит в минус велосити.
    [SerializeField] protected bool _useSlope;
    [SerializeField] [Range(0, 90)] protected float _slopeLimit;
    [SerializeField] public ContactFilter2D _collisionsFilter; //Слопы можно использовать отсюда, но пока нет особой необходимости перерабатывать этот кусок кода. 

    public bool IsGrounded { get; private set; }

    protected Rigidbody2D _rb2d;
    protected readonly RaycastHit2D[] _raycast2d = new RaycastHit2D[8];

    protected float _testAngle;

    protected Vector2 _newPositionCheck;
    protected Vector2 _newtPosition;

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

    protected virtual void CalculateNewPosition(ref Vector2 velocity)
    {
        _newtPosition = CollisionCheck(ref velocity.x, true);
        _newtPosition += CollisionCheck(ref velocity.y, false);
    }

    protected virtual Vector2 CollisionCheck(ref float velocityValue, bool isHorizontal)
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

    //Хелперы.

    protected virtual Vector2 RotateToAngle(Vector2 vector, float angle)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle) * vector.x - Mathf.Sin(Mathf.Deg2Rad * angle) * vector.y, 
            Mathf.Sin(Mathf.Deg2Rad * angle) * vector.x + Mathf.Cos(Mathf.Deg2Rad * angle) * vector.y);
    }
}



