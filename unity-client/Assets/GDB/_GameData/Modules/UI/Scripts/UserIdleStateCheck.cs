using UnityEngine;

public class UserIdleStateCheck : MonoBehaviour
{
    [SerializeField] private float _idleThreshold = 45f;
    [SerializeField] private CharacterAnimation _character;
    private float _timer = 0;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetTimer();
        }
        _timer += Time.deltaTime;
        if (!(_timer > _idleThreshold)) return;
        ResetTimer();
        _character.PlayAnimation("Angry");
    }
    private void ResetTimer()
    {
        _timer = 0;
    }
}
