using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.QGSystem
{
    /* Represents an event in a QG_Quest.
     * - Can have different endings that point to QG_EventPools. 
     * - Contains a callback that is called when an ending is chosen. */
    public class QG_Event : ScriptableObject
    {
        // endings are defined by a string and an event pool
        // having the same indices in the following lists
        public List<string>       endings = new List<string>();
        public List<QG_EventPool> endingEventPools;

        // the chosen ending, when submitted
        public string ending;

        // code to be called when an ending was chosen
        public Action callback;

        public QG_Quest quest;

        /* Clears runtime parameter that have been written to persistently saved events. */
        public void Init(QG_Quest q)
        {
            quest = q;
            ending = "";
        }

        /* Has an ending been assigned? */
        public bool IsFinished()
        {
            return ending != "";
        }

    }
}
