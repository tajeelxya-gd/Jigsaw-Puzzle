using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIManager : MonoBehaviour
{
    [Header("AI Cars")]
    [SerializeField] private CarMovement[] _aiCars;
    [SerializeField] private float _timer = 60f;

    private IProgressBar[] _aiProgressBars;
    private IAIWinCount[] _aiWinCounts;
    private IUpdateText[] _aiTexts;
    private IAIRacerName[] _aiRacerNames;
    private IAiProfilePicture[] _aiRacerPictures;

    private Coroutine _autoAICoroutine;
    private bool _aiActive = false;

    private DataBaseService<AICarSaveData> _aiDataService;
    private AICarSaveData _aiData;

    [SerializeField] private AIProfile _aiProfile;

    public bool CanAct { get; set; } = false;
    private static AIManager _instance;

    private void Awake()
    {
        // if (_instance != null && _instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // _instance = this;
        // DontDestroyOnLoad(gameObject);

    }

    private bool initiated = false;
    private WaitForSeconds _startWait = new WaitForSeconds(0.1f);
    private IEnumerator Start()
    {
        yield return _startWait;
        _aiDataService = new DataBaseService<AICarSaveData>();
        _aiData = _aiDataService.Load_Get();
        yield return _startWait;
        _aiProgressBars = new IProgressBar[_aiCars.Length];
        _aiWinCounts = new IAIWinCount[_aiCars.Length];
        _aiTexts = new IUpdateText[_aiCars.Length];
        _aiRacerNames = new IAIRacerName[_aiCars.Length];
        _aiRacerPictures = new IAiProfilePicture[_aiCars.Length];


        if (_aiData == null || _aiData.aiCars == null || _aiData.aiCars.Length != _aiCars.Length)
        {
            _aiData = new AICarSaveData();
            _aiData.aiCars = new AICarData[_aiCars.Length];
            yield return null;
            for (int i = 0; i < _aiCars.Length; i++)
            {
                _aiData.aiCars[i] = new AICarData();
                _aiData.aiCars[i].raceProgression = 0.25f;
                yield return null;

            }

            _aiData.nextAITickUtc = DateTime.UtcNow.AddSeconds(_timer).Ticks;
            _aiDataService.Save(_aiData);
        }

        for (int i = 0; i < _aiCars.Length; i++)
        {
            _aiProgressBars[i] = _aiCars[i].GetComponent<IProgressBar>();
            _aiWinCounts[i] = _aiCars[i].GetComponent<IAIWinCount>();
            _aiTexts[i] = _aiCars[i].GetComponent<IUpdateText>();
            _aiRacerNames[i] = _aiCars[i].GetComponent<IAIRacerName>();
            _aiRacerPictures[i] = _aiCars[i].GetComponent<IAiProfilePicture>();

            _aiProgressBars[i].SetCurrentProgress(_aiData.aiCars[i].progress);
            _aiWinCounts[i].SetWins(_aiData.aiCars[i].wins);
            _aiCars[i].SetCarProgression(_aiData.aiCars[i].raceProgression);
            _aiTexts[i].UpdateText();
            _aiRacerNames[i].SetRandomName(_aiData.aiCars[i].savedName);

            int picIndex = Mathf.Clamp(_aiData.aiCars[i].savedPictureIndex, 0, _aiProfile._aiData.Length - 1);
            _aiRacerPictures[i].SetProfilePicture(_aiProfile._aiData[picIndex]._profilePicture);
        }

        yield return null;
        _aiDataService.Save(_aiData);
        ApplyOfflineAIProgress();
        //AssignNamesAndPictures();
        initiated = true;
    }

    private void Update()
    {
        if (!initiated) return;
        if (_aiActive && CanAct)
            ApplyOfflineAIProgress();
    }

    private void ApplyOfflineAIProgress()
    {
        if (_aiData == null) return;
        if (_aiData.nextAITickUtc <= 0)
        {
            _aiData.nextAITickUtc = DateTime.UtcNow.AddSeconds(_timer).Ticks;
            _aiDataService.Save(_aiData);
            return;
        }

        DateTime nextTick = new DateTime(_aiData.nextAITickUtc, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;

        // Calculate how many full timer intervals have passed
        int ticksToApply = 0;
        if (now >= nextTick)
        {
            TimeSpan elapsed = now - nextTick;
            ticksToApply = (int)Math.Floor(elapsed.TotalSeconds / _timer) + 1;
        }

        for (int i = 0; i < ticksToApply; i++)
        {
            IncreaseRandomAIs(GetRandomAIIncrementCount());
        }

        SignalBus.Publish(new AssignRanks());

        _aiData.nextAITickUtc = nextTick.AddSeconds(_timer * ticksToApply).Ticks;
        _aiDataService.Save(_aiData);

        if (ticksToApply > 0)
            SignalBus.Publish(new WinCheck());
    }

    [Button]
    public void StartAutoAI()
    {
        if (_aiActive || !CanAct) return;

        ApplyOfflineAIProgress();
        AssignNamesAndPictures();

        _aiActive = true;
    }

    [Button]
    public void StopAutoAI()
    {
        _aiActive = false;

        if (_autoAICoroutine != null)
        {
            StopCoroutine(_autoAICoroutine);
            _autoAICoroutine = null;
        }

        ResetNameAndPicture();
    }

    private int GetRandomAIIncrementCount()
    {
        float r = Random.value;
        if (r < 0.5f) return 1;
        if (r < 0.75f) return 2;
        return 3;
    }

    private void IncreaseRandomAIs(int count)
    {
        int[] indices = new int[_aiCars.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for (int i = 0; i < indices.Length; i++)
        {
            int j = Random.Range(i, indices.Length);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (int i = 0; i < Mathf.Min(count, _aiCars.Length); i++)
        {
            int idx = indices[i];

            _aiWinCounts[idx].AddWin();
            _aiCars[idx].MoveCar(0.15f);
            _aiProgressBars[idx].UpdateFillBar(_aiWinCounts[idx].GetAIWins());

            _aiData.aiCars[idx].wins = _aiWinCounts[idx].GetAIWins();
            _aiData.aiCars[idx].raceProgression = _aiCars[idx].GetCurrentRaceProgress();
            _aiData.aiCars[idx].progress = _aiProgressBars[idx].GetCurrentProgress();
        }
        _aiDataService.Save(_aiData);
    }

    private void AssignNamesAndPictures()
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < _aiProfile._aiData.Length; i++)
            availableIndices.Add(i);

        for (int i = 0; i < _aiCars.Length; i++)
        {
            if (!string.IsNullOrEmpty(_aiData.aiCars[i].savedName))
            {
                _aiRacerNames[i].SetRandomName(_aiData.aiCars[i].savedName);
                int picIndex = Mathf.Clamp(_aiData.aiCars[i].savedPictureIndex, 0, _aiProfile._aiData.Length - 1);
                _aiRacerPictures[i].SetProfilePicture(_aiProfile._aiData[picIndex]._profilePicture);
                availableIndices.Remove(_aiData.aiCars[i].savedPictureIndex);
                continue;
            }

            int randListIndex = Random.Range(0, availableIndices.Count);
            int pickedIndex = availableIndices[randListIndex];
            AiData picked = _aiProfile._aiData[pickedIndex];

            _aiRacerNames[i].SetRandomName(picked._name);
            _aiRacerPictures[i].SetProfilePicture(picked._profilePicture);

            _aiData.aiCars[i].savedName = picked._name;
            _aiData.aiCars[i].savedPictureIndex = pickedIndex;

            availableIndices.RemoveAt(randListIndex);
        }
    }

    private void ResetNameAndPicture()
    {
        for (int i = 0; i < _aiData.aiCars.Length; i++)
        {
            _aiData.aiCars[i].savedName = "";
            _aiData.aiCars[i].savedPictureIndex = -1;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < _aiCars.Length; i++)
        {
            _aiWinCounts[i].SetWins(0);
            _aiCars[i].Reset();
            _aiProgressBars[i].Reset();

            _aiData.aiCars[i].wins = 0;
            _aiData.aiCars[i].raceProgression = 0.25f;
            _aiData.aiCars[i].progress = 0.25f;

            _aiTexts[i].UpdateText();
        }

        _aiData.nextAITickUtc = DateTime.UtcNow.AddSeconds(_timer).Ticks;
        _aiDataService.Save(_aiData);
    }
}

[Serializable]
public class AICarSaveData
{
    public AICarData[] aiCars;
    public long nextAITickUtc;
}

[Serializable]
public class AICarData
{
    public int wins = 0;
    public float progress = 0.25f;
    public string savedName = "";
    public int savedPictureIndex = -1;
    public float raceProgression = 0.25f;
}
