﻿using Assets.Scripts.QGSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(menuName = "Cybertext/TextEvent")]
    public class CybertextEvent : QG_Event
    {
        [Multiline] public string text;
        public List<string> decisionTexts;
    }
}
