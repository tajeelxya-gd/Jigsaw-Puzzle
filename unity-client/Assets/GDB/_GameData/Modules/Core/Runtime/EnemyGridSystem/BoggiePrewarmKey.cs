public readonly struct BoggiePrewarmKey
{
    public readonly int Index;
    public readonly int PropID;
    public readonly ColorType ColorType;

    public BoggiePrewarmKey(int index, int propID, ColorType colorType)
    {
        Index = index;
        PropID = propID;
        ColorType = colorType;
    }

    public override bool Equals(object obj)
    {
        return obj is BoggiePrewarmKey other &&
               Index == other.Index &&
               PropID == other.PropID &&
               ColorType == other.ColorType;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(Index, PropID, ColorType);
    }
}