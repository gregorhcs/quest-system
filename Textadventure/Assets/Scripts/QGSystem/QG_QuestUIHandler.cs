using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.QGSystem
{
    public class QG_QuestUIHandler : MonoBehaviour
    {
        public static QG_QuestUIHandler Instance;

        public GameObject uiQuestPanel;
        public GameObject uiPoolButtonPrefab;
        public GameObject uiLayerButtonPrefab;
        public GameObject uiLinePrefab;
        public GameObject uiStarPrefab;

        private QG_Quest questToDraw;

        private Vector3 _origin = new Vector3(-1.2f, 0, 0);

        private float HORIZ_GAP = 0.7f;
        private float VERT_GAP = 0.7f;

        private Dictionary<QG_EventPool, Vector3> nodePosRegistry = new Dictionary<QG_EventPool, Vector3>();

        private Bounds uiBoundaries = new Bounds(new Vector3(0, 0, 0), new Vector3(4.5f, 1.6f));

        private void Awake()
        {
            Instance = this;
        }

        // ---------------------------------------------------------------------
        //          methods that are concerned with the whole quest UI
        // ---------------------------------------------------------------------

        private void Fade(bool in_, float duration)
        {
            if (in_)
            {
                Debug.Log("Fade in");
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(0, 0, false);
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(1, duration, false);
            }
            else
            {
                Debug.Log("Fade out");
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(1, 0, false);
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(0, duration, false);
            }

            foreach (Transform child in uiQuestPanel.transform)
            {
                Image im = child.gameObject.GetComponent<Image>();
                if (im != null)
                {
                    if (in_)
                    {
                        im.CrossFadeAlpha(0, 0, false);
                        im.CrossFadeAlpha(1, duration, false);
                    }
                    else
                    {
                        im.CrossFadeAlpha(1, 0, false);
                        im.CrossFadeAlpha(0, duration, false);
                    }
                }

                Text tx = child.gameObject.GetComponent<Text>();
                if (tx != null)
                {
                    if (in_)
                    {
                        tx.CrossFadeAlpha(0, 0, false);
                        tx.CrossFadeAlpha(1, duration, false);
                    }
                    else
                    {
                        tx.CrossFadeAlpha(1, 0, false);
                        tx.CrossFadeAlpha(0, duration, false);
                    }
                }
            }
        }

        public void ClearAll()
        {
            if (questToDraw != null)
                Fade(false, 0.25f);

            uiQuestPanel.SetActive(false);
        }

        public void DrawQuest(QG_Quest quest, int depth = -1, bool fade = false)
        {
            if (questToDraw != null && fade)
                Fade(false, 0.25f);

            DrawQuest(quest, depth);

            if (fade)
                Fade(true, 0.25f);
        }
        private void DrawQuest(QG_Quest baseQuest, int depth)
        {
            foreach (Transform child in uiQuestPanel.transform)
            {
                Destroy(child.gameObject);
            }
            nodePosRegistry.Clear();

            uiQuestPanel.SetActive(true);

            //Debug.Log("Begin Draw Quest " + baseQuest.name + " " + depth);
            //Debug.Log(quest);

            // ------------ sub quest handling ------------

            QG_Quest previousQuestToDraw = questToDraw;

            QG_Quest curQuest = baseQuest;
            int depthCounter = 0;

            while (curQuest != null)
            {
                bool isActive = false;

                if (   (depth == -1 && curQuest.currentSubQuest == null) 
                    || (depth != -1 && depthCounter == depth           ))
                {
                    questToDraw = curQuest;
                    isActive = true;
                }

                DrawLayerButton(baseQuest, depthCounter, isActive);
                curQuest = curQuest.currentSubQuest;
                depthCounter++;
            }
            Debug.Log(questToDraw);

            // ------------ shift origin ------------
            //
            // origin shifts leftwards d units, where d is the 
            // distance from start to the last active node (lan)
            //
            // does not apply if start node is close (d < 3) to lan

            

            // ------------ node registry ------------
            //
            // draw from left to right via breadth first search
            // nodes with same depth are stacked vertically

            HashSet<QG_EventPool> wavesAhead = new HashSet<QG_EventPool>(questToDraw.eventPools);
            wavesAhead.Remove(questToDraw.start);

            HashSet<QG_EventPool> wave = new HashSet<QG_EventPool>();
            wave.Add(questToDraw.start);

            HashSet<QG_EventPool> wavesBehind = new HashSet<QG_EventPool>();

            int waveCount = 0;

            int activeNodeDist = -1;


            while (wave.Count() != 0)
            {
                ComputeNodePositions(waveCount, wave); // registers node positions in nodePosRegistry

                HashSet<QG_EventPool> newWave = new HashSet<QG_EventPool>();

                foreach (QG_EventPool pool in wave)
                {
                    pool.wave = waveCount;

                    if (pool.isActive())
                    {
                        activeNodeDist = waveCount;
                    }

                    foreach (QG_Event event_ in pool.pool)
                    {
                        foreach (QG_EventPool p in event_.endingEventPools)
                        {
                            if (!wavesBehind.Contains(p) && !wave.Contains(p))
                            {
                                newWave.Add(p);
                            }
                        }
                    }

                }

                foreach (QG_EventPool p in newWave)
                    if (wavesAhead.Contains(p))
                        wavesAhead.Remove(p);

                foreach (QG_EventPool p in wave)
                    wavesBehind.Add(p);

                wave = newWave;

                waveCount++;
            }

            if (activeNodeDist == -1)
                activeNodeDist = waveCount;

            // ------------ node draw ------------------

            float activeNodeOffset = activeNodeDist < 1 ?
                0 : (activeNodeDist - 1) * HORIZ_GAP;

            int farawayDrawn = 0;

            foreach (QG_EventPool p in questToDraw.eventPools)
            {
                nodePosRegistry[p] = new Vector3(nodePosRegistry[p].x - activeNodeOffset, nodePosRegistry[p].y, 0);

                if (-1 < (activeNodeDist - p.wave) && (activeNodeDist - p.wave) < 3)
                    DrawPool(p, true);

                else if (-1 == (activeNodeDist - p.wave))
                    DrawPool(p, false);

                else if (UnityEngine.Random.value > 0.7f && !(farawayDrawn >= 3))
                {
                    DrawPool(p, false);
                    farawayDrawn++;
                }

                p.inCount = p.outCount = 0;
            }

            // ------------ arrow draw ------------
            //
            // draw an arrow from pool A to pool B if pool A
            // contains an event that has an ending leading to B

            DrawArrow(new Vector3(_origin.x - HORIZ_GAP - activeNodeOffset, 0, 0), nodePosRegistry[questToDraw.start], true);
            questToDraw.start.inCount += 1;

            foreach (QG_EventPool p1 in questToDraw.eventPools)
            {
                var outPools = new List<QG_EventPool>();
                var starred = new List<QG_EventPool>();

                foreach (QG_Event event_ in p1.pool)
                {
                    foreach (QG_EventPool p2 in event_.endingEventPools)
                    {
                        if (!(-1 < (activeNodeDist - p1.wave) && (activeNodeDist - p1.wave) < 3)) // forward, backwards
                            continue;

                        Vector3 s = nodePosRegistry[p1];
                        Vector3 d = nodePosRegistry[p2];
                        bool r = p1.connUsed[p2];

                        if (!outPools.Contains(p2))
                        {
                            DrawArrow(s, d, r);
                            p1.outCount += 1;
                            p2.inCount += 1;
                            outPools.Add(p2);
                        }
                        else if (!starred.Contains(p2))
                        {
                            DrawArrowStar(s, d, r);
                            starred.Add(p2);
                        }

                    }
                }
            }

            // ---------- optional fade in -------------
            //
            // occurs if a change in quests occurs, e.g.
            // - change into subquest
            // - whole quest exchanged

            if (previousQuestToDraw != questToDraw)
                Fade(true, 0.25f);

        }


        // ---------------------------------------------------------------------
        //                     methods to draw sub elements
        // ---------------------------------------------------------------------

        private void ComputeNodePositions(int horizDist, HashSet<QG_EventPool> nodes)
        {
            float x = _origin.x + horizDist * HORIZ_GAP;

            int nodesCount = nodes.Count();

            if (nodesCount == 0)
                return;

            else if (nodesCount == 1)
                nodePosRegistry[nodes.ElementAt(0)] = new Vector3(x, _origin.y, 0);

            else
            {
                float yOffset = - (nodesCount - 1) * HORIZ_GAP / 2;

                for (int i = 0; i < nodesCount; i++)
                    nodePosRegistry[nodes.ElementAt(i)] = new Vector3(x, _origin.y + yOffset + i * VERT_GAP, 0);
            }

        }
        
        private void DrawPool(QG_EventPool node, bool nearby)
        {
            // boundary test

            Vector3 pos = nodePosRegistry[node];

            if (!uiBoundaries.Contains(pos))
                return;

            // draw

            GameObject newUINode = Instantiate(uiPoolButtonPrefab, uiQuestPanel.transform) as GameObject;
            newUINode.transform.Translate(pos);

            newUINode.transform.Find("Text").gameObject.GetComponent<Text>().text = node.name_;

            // faraway

            if (! nearby)
            {
                Debug.Log(node);

                if (node.pool.Count == 0 || node.pool[0] is NoChoiceTimedEvent || node.pool[0] is TutorialEvent)
                {
                    Debug.Log("c");
                    newUINode.transform.Find("AheadMisc").gameObject.SetActive(true);
                }

                else if (node.pool[0] is QG_Quest)
                {
                    Debug.Log("a");
                    newUINode.transform.Find("AheadSubquest").gameObject.SetActive(true);
                }

                else // is choice
                {
                    Debug.Log("b");
                    newUINode.transform.Find("AheadChoice").gameObject.SetActive(true);
                }

                return;
            }

            // nearby

            newUINode.transform.Find("HereBase").gameObject.SetActive(true);

            if (node.pool.Count == 0 || node.pool[0] is NoChoiceTimedEvent || node.pool[0] is TutorialEvent)
            {
                if (node.isActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereCircle-On").gameObject.SetActive(true);

                if (node.used)
                    newUINode.transform.Find("HereCircle").gameObject.SetActive(true);

                if (node.pool.Count() == 0)
                {
                    string suffix = (node.isActive() || questToDraw.poolsQueue.Contains(node)) ? "-On" : "";

                    newUINode.transform.Find("HereL45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereL-45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereR45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereR-45" + suffix).gameObject.SetActive(true);
                }
            }

            else if (node.pool[0] is QG_Quest)
                {
                if (node.isActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereSubquest-On").gameObject.SetActive(true);
                else
                    newUINode.transform.Find("HereSubquest").gameObject.SetActive(true);
            }

            else
            {
                if (node.isActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereChoice-On").gameObject.SetActive(true);
                else
                    newUINode.transform.Find("HereChoice").gameObject.SetActive(true);
            }
        }

        private void DrawArrow(Vector3 start, Vector3 end, bool used)
        {
            // boundary test

            bool startInside = uiBoundaries.Contains(start);
            bool endInside = uiBoundaries.Contains(end);

            if (!startInside && endInside)
                start += (end - start) / 2;

            else if (startInside && !endInside)
                end -= (end - start) / 2;

            else if (!startInside && !endInside)
                return;

            // drawing

            GameObject newUILine = Instantiate(uiLinePrefab, uiQuestPanel.transform) as GameObject;

            newUILine.transform.Translate(start);

            LineRenderer lineRenderer = newUILine.GetComponent<LineRenderer>();

            if (used)
            {
                lineRenderer.startColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                lineRenderer.endColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                lineRenderer.startColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
                lineRenderer.endColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            }

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.SetPosition(0, (end - start) * 28);
            lineRenderer.SetPosition(1, (end - start) * 80);
            lineRenderer.useWorldSpace = false;

        }

        private void DrawArrowStar(Vector3 start, Vector3 end, bool used)
        {
            Vector3 pos = start + (end - start) / 2 + new Vector3(0.0f, 0.1f);// + new Vector3(1.15f, -0.5f);

            if (!uiBoundaries.Contains(pos))
                return;

            GameObject newUINode = Instantiate(uiStarPrefab, uiQuestPanel.transform) as GameObject;
            newUINode.transform.Translate(pos);
        }

        private void DrawLayerButton(QG_Quest baseQuest, int depth, bool isActive)
        {
            GameObject newUINode = Instantiate(uiLayerButtonPrefab, uiQuestPanel.transform) as GameObject;

            newUINode.transform.Translate(new Vector3(4.6f, - 0.2f - 0.25f * depth, 2));

            newUINode.GetComponent<Button>().onClick.AddListener(() => DrawQuest(baseQuest, depth, true));

            if (isActive)
                newUINode.GetComponent<Image>().color = new Color(0f, 0.5f, 0f, 1f);

        }

    }
}
