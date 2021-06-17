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
        public List<String> endings;

        public String ending = null;

        public List<QG_EventPool> endingEventPools;

        public float intensity;

        public QG_Quest quest;

        public bool IsFinished()
        {
            return ending != null;
        }

    }
}
