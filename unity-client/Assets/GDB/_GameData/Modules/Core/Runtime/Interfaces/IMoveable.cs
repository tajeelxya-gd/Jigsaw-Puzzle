using System;
using UnityEngine;
public interface IMoveable
{
    public bool CanMove { get; set; }
    public void Move(Vector3 target);
}