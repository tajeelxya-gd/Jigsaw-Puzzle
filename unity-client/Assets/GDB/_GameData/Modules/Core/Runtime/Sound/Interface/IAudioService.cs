using UnityEngine;

public interface IAudioService
{
    public void PlaySFX(AudioType audioType, float vol = 1, float spatialBlend = 1);
    public void PlaySFX(AudioClip audioClip, float vol = 1, float spatialBlend = 1);
    public void PlayOnDemandSFX(AudioType audioType, float vol = 1);
    public void PlayBG(AudioType audioType, float vol = 1);
    public void StopBG();
    public void ChangeBGVolumeLevel(float vol);
}
public enum AudioType
{
    BGSimple, BGHard, BGSuperHard, Hit, ButtonClick, CoinAddition, HealthAddition, EnemiesAddition, TrophiesAddition, Win, Loss, MagnetEffect, ShuffleEffect,
    HammerEffect, SlotPopperEffect, CannonTouch, CannonMerge, ItemRattle, WallHit, SettingButtonClick, DailyRewards, PuzzleUnlock, RaceWin, RaceFail,
    InstantiatingSound, Piggy, ChestOpen, CollectSoft, KeyCollect,PanelPop,RewardPopUp,PanelClose
} //Audio Type Here