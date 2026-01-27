using UniTx.Runtime.Serialisation;

namespace Client.Runtime
{
    public interface IUserSavedData : ISavedData
    {
        int CurrentLevel { get; set; }

        JigsawLevelState CurrentLevelState { get; set; }
    }
}