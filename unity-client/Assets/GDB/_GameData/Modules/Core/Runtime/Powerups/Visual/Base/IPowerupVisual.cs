using System;
public interface IPowerupVisual
{
    public Action OnVisualEnd { get; set; }
    public void PerformVisual();
}