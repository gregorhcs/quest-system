using Assets.Scripts.QGSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    [CreateAssetMenu(menuName = "Cybertext/QuestEvent")]
    public class QG_Quest : QG_Event
    {
        public string name;

        public QG_EventPool start;

        public List<QG_EventPool> eventPools = new List<QG_EventPool>();

        public List<QG_EventPool> poolsQueue = new List<QG_EventPool>();

        public List<QG_EventPool> currentPools = new List<QG_EventPool>();
        public List<QG_Event> currentEvents = new List<QG_Event>();

        public QG_Quest currentSubQuest;

        public QG_EventPool end;

        public override string ToString()
        {
            string s = "";

            s += "---------------\n";

            s += "quest name: " + name + "\n";

            s += "active event pools:\n";
            foreach (QG_EventPool pool in currentPools)
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

        public QG_Quest(string name_, QG_EventPool start, List<QG_EventPool> eventPools, List<string> endings)
        {
            name = name_;
            this.start = start;
            this.eventPools = eventPools;
            this.endings = endings;

            init();
        }

        public void init()
        {
            poolsQueue.Clear();
            poolsQueue.Add(start);

            currentPools.Clear();
            currentEvents.Clear();

            foreach (QG_EventPool p in this.eventPools)
                p.init(this);
        }

        public void EventUpdate(QG_Event event_, String ending)
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
                    ending = event_.endings[0]; // -------------------- TODO
                }
            }

            currentEvents.Remove(event_);

            // pool update

            QG_EventPool pool = currentPools.Find(p => p.pool.Contains(event_));
            pool.activeEvents--;
            pool.used = true;

            if (!pool.isActive())
                currentPools.Remove(pool);

            // event ending update

            event_.ending = ending;

            int endingIndex = event_.endings.FindIndex(str => str.Equals(ending));
            QG_EventPool newPool = event_.endingEventPools[endingIndex];

            pool.connUsed[newPool] = true;

            if (newPool.pool.Count() == 0)
            {
                base.ending = endings[0]; // -------------------- TODO
            }

            poolsQueue.Add(newPool);
        }

        // returns null if no event in pool
        public QG_Event NextEvent()
        {
            // check for an unfinished quest event

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
                Debug.Log(name);
                base.ending = endings[0]; // -------------------- TODO
                return null;
            }


            for (int i = 0; i < poolsQueue.Count(); i++)
            {

                QG_EventPool nextPool = poolsQueue[i];

                int curPoolCount = nextPool.pool.Count();

                // quest is ended
                if (nextPool.pool.Count() == 0)
                {
                    base.ending = endings[0]; // -------------------- TODO
                    return null;
                }

                poolsQueue.RemoveAt(i);

                QG_Event nextEvent = nextPool.pool[Random.Range(0, curPoolCount)];


                // check if quest event
                if (nextEvent is QG_Quest)
                {
                    QG_Quest potentialSubQuest = (QG_Quest)nextEvent;

                    //Debug.Log("subquest found" + potentialSubQuest.name);

                    if (potentialSubQuest.IsFinished())
                        continue;

                    nextEvent = potentialSubQuest.NextEvent();

                    //Debug.Log("subquest found - " + nextEvent.name);

                    if (nextEvent == null)
                        continue;

                    //Debug.Log("subquest found doing it");

                    currentSubQuest = potentialSubQuest;


                    nextPool.activeEvents++;

                    currentPools.Add(nextPool);
                    currentEvents.Add(currentSubQuest);

                    return nextEvent;
                }
                else
                {
                    //Debug.Log("standard event found" + nextEvent.name);

                    nextPool.activeEvents++;

                    currentPools.Add(nextPool);
                    currentEvents.Add(nextEvent);

                    return nextEvent;
                }
            }

            return null;
        }

    }
}
