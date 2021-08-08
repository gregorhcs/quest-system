using Assets.Scripts.QGSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{

    /* Class that represents a quest that may be played through.
     * 
     * - Can be an event in another quest.
     * - Provides methods NextEvent(..) and EventUpdate(..)
     *   that allow fetching an event and recording its
     *   reception by the player.
     *   
     */
    [CreateAssetMenu(menuName = "Cybertext/QuestEvent")]
    public class QG_Quest : QG_Event
    {
        public QG_EventPool startPool;

        // nodes of this quest
        public List<QG_EventPool> eventPools = new List<QG_EventPool>();

        // list of nodes that have been reached via an ending,
        // but have not yet been active
        public List<QG_EventPool> poolsQueue = new List<QG_EventPool>();

        // pools/events that are currently active and waiing to be ended
        public List<QG_EventPool> activePools = new List<QG_EventPool>();
        public List<QG_Event> activeEvents = new List<QG_Event>();

        // needed if a subquest is active
        public QG_Quest currentSubQuest;


        /* Converts the quest to a string representation. */
        public override string ToString()
        {
            string s = "";

            s += "---------------\n";

            s += "quest name: " + name + "\n";

            s += "active event pools:\n";
            foreach (QG_EventPool pool in activePools)
            {
                s += "    " + pool.name + " (" + pool.activeEvents + "): ";
                foreach (QG_Event e in pool.pool)
                {
                    s += e.name + ", ";
                }
                s += "\n";
            }

            s += "queued event pools: ";
            foreach (QG_EventPool pool in poolsQueue)
            {
                s += pool.name + " (" + pool.pool.Count + "), ";
            }

            s += "\n---------------\n";

            return s;
        }


        /* Constructs a quest object. */
        public QG_Quest(string name_, QG_EventPool startPool, List<QG_EventPool> eventPools, List<string> endings)
        {
            name = name_;
            this.startPool = startPool;
            this.eventPools = eventPools;
            this.endings = endings;

            Init();
        }

        /* Clears runtime parameter that have been written to persistently saved quests. */
        public void Init()
        {
            poolsQueue.Clear();
            poolsQueue.Add(startPool);

            activePools.Clear();
            activeEvents.Clear();

            foreach (QG_EventPool p in eventPools)
                p.Init(this);
        }

        /* Finds event_ in the quest hierarchy, removes it from active-status,
         * calls its attached callback and adds event pool specified for ending 
         * to pool queue. */
        public void EventUpdate(QG_Event event_, string ending)
        {
            // special handling for sub quests

            if (event_.quest != this)
            {
                currentSubQuest.EventUpdate(event_, ending);

                if (!currentSubQuest.IsFinished())
                    return;
                else
                {
                    event_ = currentSubQuest;
                    //---- TODO ---- Transferring subquest-event 
                    // endings to subquest-ending not supported.
                    ending = currentSubQuest.ending;
                }
            }

            event_.callback?.Invoke();

            activeEvents.Remove(event_);

            // pool update

            QG_EventPool pool = activePools.Find(p => p.pool.Contains(event_));

            pool.activeEvents--;
            pool.used = true;

            if (!pool.IsActive())
                activePools.Remove(pool);

            // event ending update

            event_.ending = ending;

            int endingIndex      = event_.endings.FindIndex(str => str.Equals(ending));
            QG_EventPool newPool = event_.endingEventPools[endingIndex];

            pool.connUsed[newPool] = true;

            if (newPool.IsEnding())
            {
                //---- TODO ---- Transferring subquest-event 
                // endings to subquest-ending not supported.
                base.ending = endings[0];
            }

            poolsQueue.Add(newPool);
        }

        /* Fetches the next event to be given to the player.
         * 
         * TODO: Strategy for choosing events from a pool is always random.
         * 
         * Returns null if no event is in the currently active pool. */
        public QG_Event NextEvent()
        {
            // check for an unfinished subquest event

            if (currentSubQuest != null)
            {
                QG_Event subEvent = currentSubQuest.NextEvent();

                if (currentSubQuest.IsFinished())
                    currentSubQuest = null;
                else
                    return subEvent;
            }

            // find next event in pool queue

            if (poolsQueue.Count() == 0)
            {
                //---- TODO ---- Transferring subquest-event 
                // endings to subquest-ending not supported.
                base.ending = endings[0];
                return null;
            }

            for (int i = 0; i < poolsQueue.Count(); i++)
            {

                QG_EventPool nextPool = poolsQueue[i];

                int curPoolCount = nextPool.pool.Count();

                // quest is ended
                if (nextPool.IsEnding())
                {
                    //---- TODO ---- Transferring subquest-event 
                    // endings to subquest-ending not supported.
                    base.ending = endings[0];
                    return null;
                }

                poolsQueue.RemoveAt(i);

                QG_Event nextEvent = nextPool.pool[Random.Range(0, curPoolCount)];


                // next event was found, now register it as active
                if (nextEvent is QG_Quest)
                {
                    QG_Quest potentialSubQuest = (QG_Quest)nextEvent;

                    if (potentialSubQuest.IsFinished())
                        continue;

                    nextEvent = potentialSubQuest.NextEvent();

                    if (nextEvent == null)
                        continue;

                    currentSubQuest = potentialSubQuest;


                    nextPool.activeEvents++;

                    activePools.Add(nextPool);
                    activeEvents.Add(currentSubQuest);

                    return nextEvent;
                }
                else
                {
                    nextPool.activeEvents++;

                    activePools.Add(nextPool);
                    activeEvents.Add(nextEvent);

                    return nextEvent;
                }
            }

            return null; // no fitting pool+event was found in queue
        }

    }
}
