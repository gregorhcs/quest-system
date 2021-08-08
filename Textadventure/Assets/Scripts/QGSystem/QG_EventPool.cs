using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.QGSystem
{
    /* Represents a node in a QG_Quest. Contains QG_Events and might be 
     * referenced by them via their endings. */
    [CreateAssetMenu(menuName = "Cybertext/Pool")]
    public class QG_EventPool : ScriptableObject
    {
        public List<QG_Event> pool;

        // has this pool been active?
        public bool used = false;

        // saves whether connections from an event in this pool
        // to another pool have been used
        public Dictionary<QG_EventPool, bool> connUsed =
           new Dictionary<QG_EventPool, bool>();

        // how many events of this pool are currently active in its quest?
        public int activeEvents = 0;

        // represents the depth at which this node was found 
        // in the breadth-first-search of its quest
        public int wave;


        /* Clears runtime parameter that have been written to persistently saved pools. */
        public void Init(QG_Quest quest)
        {
            used = false;
            activeEvents = 0;

            foreach (QG_Event e in pool)
            {
                e.Init(quest);

                foreach (QG_EventPool p in e.endingEventPools)
                    connUsed[p] = false;

                if (e is QG_Quest)
                    ((QG_Quest)e).Init();
            }
        }

        /* Returns whether this pool has active events. */
        public bool IsActive()
        {
            return activeEvents > 0;
        }

        /* Returns whether this pool is an ending node. */
        public bool IsEnding()
        {
            return pool.Count == 0;
        }
    }
}
