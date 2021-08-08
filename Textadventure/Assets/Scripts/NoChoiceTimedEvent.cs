using UnityEngine;

namespace Assets.Scripts
{
    /* Special CybertextEvent displaying no choice - instead
     * it is automatically ended after secondsToWait. */
    [CreateAssetMenu(menuName = "Cybertext/TimedEvent")]
    public class NoChoiceTimedEvent : CybertextEvent
    {
        public float secondsToWait = 2.0f;

        public NoChoiceTimedEvent()
        {
            endings.Add("standard");
        }
    }
}
