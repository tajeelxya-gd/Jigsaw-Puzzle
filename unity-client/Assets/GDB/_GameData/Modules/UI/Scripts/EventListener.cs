using UnityEngine;

namespace DevFahad
{
    public class EventListener : MonoBehaviour
    {
        [SerializeField] private CharacterAnimation _animation;
        [SerializeField] private BirdAnimation _yellowBirdAnimation;
        [SerializeField] private BirdAnimation _blueBirdAnimation;

        public void OnAnimationFinished()
        {
            _animation.PlayRandomAnimation();
        }

        public void PlayBlueBirdAnimation()
        {
            _blueBirdAnimation.PlayAnimation();
        }
        public void PlayYellowBirdAnimation()
        {
            _yellowBirdAnimation.PlayAnimation();
        }
    }
}