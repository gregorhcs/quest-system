using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.QGSystem
{
    public class QG_Event : ScriptableObject
    {
        public List<String> endings = new List<string>();

        public String ending = "";

        public List<QG_EventPool> endingEventPools;

        public float intensity;

        public QG_Quest quest;

        public void init(QG_Quest q)
        {
            quest = q;
            ending = "";
        }

        public bool IsFinished()
        {
            return ending != "";
        }

    }
}
