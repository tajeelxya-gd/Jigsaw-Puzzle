using UnityEngine;

public interface IFactory<T, J>
{
    T Create(J type, Transform parent);
}