using System;

public class PowerUpVisualStartSignal : ISignal
{
    public PowerupType powerupType;
    public Action OnClose;
}