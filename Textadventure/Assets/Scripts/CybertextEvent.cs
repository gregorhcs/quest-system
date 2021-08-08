using Assets.Scripts.QGSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /* Special QG_Event contains text to be displayed 
     * and decisions to choose from. */
    [CreateAssetMenu(menuName = "Cybertext/TextEvent")]
    public class CybertextEvent : QG_Event
    {
        [Multiline] public string text;
        public List<string> decisionTexts;
    }
}
