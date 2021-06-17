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
    public class QG_Quest
    {
        public string name;

        public QG_EventPool start;

        public List<QG_EventPool> eventPools;

        public List<QG_EventPool> poolsQueue = new List<QG_EventPool>();

        public List<QG_EventPool> currentPools = new List<QG_EventPool>();
        public List<QG_Event> currentEvents = new List<QG_Event>();

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

            s += "---------------\n";

            return s;
        }

        public QG_Quest(string name_, QG_EventPool start, List<QG_EventPool> eventPools)
        {
            name = name_;
            this.start = start;
            this.eventPools = eventPools;

            poolsQueue.Add(start);

            foreach (QG_EventPool p in this.eventPools)
                p.init(this);
        }

        public void EventUpdate(QG_Event event_, String ending)
        {
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

            poolsQueue.Add(newPool);
        }

        // returns null if no event in pool
        public QG_Event NextEvent()
        {
            if (poolsQueue.Count() == 0)
                return null;

            for (int i = 0; i < poolsQueue.Count(); i++)
            {
                QG_EventPool nextPool = poolsQueue[i];

                int curPoolCount = nextPool.pool.Count();

                if (curPoolCount == 0)
                    continue; // quest is ended

                poolsQueue.RemoveAt(i);

                QG_Event nextEvent = nextPool.pool[Random.Range(0, curPoolCount)];
                nextPool.activeEvents ++;

                currentPools.Add(nextPool);
                currentEvents.Add(nextEvent);

                return nextEvent;
            }

            return null;
        }

    }
}
