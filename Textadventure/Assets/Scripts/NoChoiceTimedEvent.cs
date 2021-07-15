using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
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
