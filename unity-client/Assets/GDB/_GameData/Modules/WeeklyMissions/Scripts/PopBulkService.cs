using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public interface IBulkPopService
{
    public void PlayEffect(int rewardAmount,PopBulkService.BulkPopUpServiceType serviceType, Vector2 spawnPosition, int animatedCoinsAmount = 10,UnityAction onComplete = null);
}
public class PopBulkService : MonoBehaviour,IBulkPopService
{
    public enum BulkPopUpServiceType
    {
        Coins,Trophy,Health,EnemyBlocks, Hammer, Magnets, Wand, SlotPopper
    }
    [SerializeField] private BulkPopUpEffect _trophiesGeneratorEffect;
    [SerializeField] private BulkPopUpEffect bulkPopUpEffectEffect;
    [SerializeField] private BulkPopUpEffect _hearsGeneratorEffect;
    [SerializeField] private BulkPopUpEffect _enemyBlockGeneratorEffect;
    [SerializeField] private BulkPopUpEffect _hammerGeneratorEffect;
    [SerializeField] private BulkPopUpEffect _wandGeneratorEffect;
    [SerializeField] private BulkPopUpEffect _slotPopperGeneratorEffect;
    [SerializeField] private BulkPopUpEffect _magnetGeneratorEffect;

    private void Start()
    {
        GlobalService.RegisterBulkPopService(this);
    }

    public void PlayEffect(int rewardAmount, PopBulkService.BulkPopUpServiceType serviceType, Vector2 spawnPosition, int animatedCoinsAmount = 10,UnityAction onComplete = null)
    {
        switch (serviceType)
        {
            case BulkPopUpServiceType.Coins:
            {
                bulkPopUpEffectEffect.transform.position = spawnPosition;
                Debug.LogError("adding coin");
                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnCoinsUpdateSignal { Amount = amount });
                };

                bulkPopUpEffectEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }
            
            case BulkPopUpServiceType.EnemyBlocks:
            {
                _enemyBlockGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnEnemyBlockUpdateSignal() { Amount = amount });
                };

                _enemyBlockGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }

            case BulkPopUpServiceType.Trophy:
            {
                _trophiesGeneratorEffect.transform.position = spawnPosition;
                _trophiesGeneratorEffect.Generate(null, animatedCoinsAmount);
                break;
            }

            case BulkPopUpServiceType.Health:
            {
                _hearsGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnHealthUpdateSignal { TimeToAdd = amount });
                };

                _hearsGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }
            case BulkPopUpServiceType.Hammer:
            {
                _hammerGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnHammerUpdateSignal { Amount = amount });
                };

                _hammerGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }
            case BulkPopUpServiceType.Wand:
            {
                _wandGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnWandUpdateSignal { Amount = amount });
                };

                _wandGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }
            case BulkPopUpServiceType.SlotPopper:
            {
                _slotPopperGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnSlotPopperUpdateSignal { Amount = amount });
                };

                _slotPopperGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }
            case BulkPopUpServiceType.Magnets:
            {
                _magnetGeneratorEffect.transform.position = spawnPosition;

                int baseAmount = rewardAmount / animatedCoinsAmount;
                int remainder = rewardAmount % animatedCoinsAmount;
                int index = 0;

                UnityAction action = () =>
                {
                    int amount = baseAmount;
                    if (index < remainder)
                        amount += 1;
                    index++;
                    SignalBus.Publish(new OnMagnetUpdateSignal { Amount = amount });
                };

                _magnetGeneratorEffect.Generate(null, animatedCoinsAmount, action,onComplete);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
        }
    }

    [Button]
    public void TestAddMagnets()
    {
        PlayEffect(10,PopBulkService.BulkPopUpServiceType.Magnets,transform.position, 4);
        
    }



}

