using UnityEngine;

public class GlobalService
{
    public static IParticleService ParticleService { get; private set; }
    private static GameData _gameData;

    private static int maxLevelCount = 200;

    public static int MaxLevel 
    { 
        get { return maxLevelCount; }
        set { maxLevelCount = value; }
    }
    public static GameData GameData
    {
        get
        {
            if (_gameData == null)
            {
                _gameData = new GameData();
                _gameData.SetupData();
            }

            return _gameData;
        }
        private set => _gameData = value;
    }

    
    public static IBulkPopService IBulkPopService { get; private set; }

    public static void RegisterBulkPopService(IBulkPopService bulkPopService)
    {
        IBulkPopService = bulkPopService;
    }
    public static void Initialize(IParticleService particleService)
    {
        if (ParticleService == null)
            ParticleService = particleService;

        if (_gameData == null)
        {
            _gameData = new GameData();
            _gameData.SetupData();
        }
    }
}