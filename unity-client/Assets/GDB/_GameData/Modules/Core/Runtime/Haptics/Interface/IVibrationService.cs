using Lofelt.NiceVibrations;

public interface IVibrationService
{
    public void Vibrate(HapticPatterns.PresetType presetType);
    public void Vibrate(HapticType hapticType);
}
public enum HapticType
{
    CannonTouch, Merge, Btn, Hammer, LevelFail, Puzzle, Win, Pop, Instantiating, Hit, Wrong
}