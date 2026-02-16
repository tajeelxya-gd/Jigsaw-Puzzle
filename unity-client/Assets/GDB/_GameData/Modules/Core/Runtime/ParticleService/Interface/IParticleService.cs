using UnityEngine;

public interface IParticleService
{
    public void PlayParticle(ParticleType particleType, Transform transform = null, bool makeChild = false, bool pool = true, bool SetColor = false, Color color = new Color());
    public void PlayParticle(ParticleType particleType, Vector3 Pos, bool pool = true, bool SetColor = false, Color color = new Color());
}
public enum ParticleType { CannonShot, Hit, Shatter, CoinBurst, WallHit, TreasureOpen, HammerHit, MergeEffect, StarEffect }