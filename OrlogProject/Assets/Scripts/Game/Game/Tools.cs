using UnityEngine;

public class Tools : MonoBehaviour
{
    static public bool Approximation(float _value, float _target, float _limit)
    {
        return Mathf.Abs(_value - _target) < _limit;
    }
}
