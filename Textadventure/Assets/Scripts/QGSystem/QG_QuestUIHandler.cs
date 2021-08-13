using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.QGSystem
{
    /* This singleton manages the quest UI and draws QG_Quests on it.
     * 
     * Public Methods:
     * - Deactivate().  Turns off the quest UI.
     * - DrawQuest(..). Turns on the quest UI and draws the given quest.
     * 
     * There three major sections in this class:
     * 1. Variables and Initializer
     * 2. Methods concerning the whole quest UI
     * 3. Methods concerning specific parts of the quest UI
     * 
     * The most central method is DrawQuest, which contains or delegates
     * almost every functionality implemented.
     */
    public class QG_QuestUIHandler : MonoBehaviour
    {
        // Statics

        public static QG_QuestUIHandler Instance;

        // Constants

        private readonly Vector3 ORIGIN = new Vector3(-1.2f, 0, 0);

        private readonly float HORIZ_GAP = 0.7f;
        private readonly float VERT_GAP  = 0.7f;

        private readonly float CHANCE_HIDE_FARAWAY = 0.7f;

        private readonly Bounds UI_BOUNDARIES = 
                     new Bounds(new Vector3(0, 0, 0), new Vector3(4.5f, 1.6f));

        // Prefabs

        public GameObject uiQuestPanel;
        public GameObject uiPoolButtonPrefab;
        public GameObject uiLayerButtonPrefab;
        public GameObject uiLinePrefab;
        public GameObject uiStarPrefab;
        public GameObject uiLSwitchPrefab;
        public GameObject uiRSwitchPrefab;

        // Changing Variables

        private QG_Quest questToDraw;

        private Dictionary<QG_EventPool, Vector3> nodePosRegistry = 
            new Dictionary<QG_EventPool, Vector3>();

        private int lastDepth = 0;


        /* Sets up the singleton. */
        private void Awake()
        {
            Instance = this;
        }


        // ---------------------------------------------------------------------
        //          methods that are concerned with the whole quest UI
        // ---------------------------------------------------------------------

        /* Fades the quest panel in or out (in_) in duration seconds. 
           Does not trigger activation of the quest panel or quest drawing. */
        private void Fade(bool in_, float duration)
        {
            if (in_)
            {
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(0, 0, false);
                uiQuestPanel.GetComponent<Image>().CrossFadeAlpha(1, duration, false);
            }
            else
            {
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

        /* Fades out the quest panel and deactivates it. */
        public void Deactivate()
        {
            if (questToDraw != null)
                Fade(false, 0.25f);

            uiQuestPanel.SetActive(false);
        }

        /* Clears and draws quest at depth with potential fade-in. */
        public void DrawQuest(QG_Quest quest, int depth = -1, bool fade = false)
        {
            if (questToDraw != null && fade)
                Fade(false, 0.25f);

            DrawQuest(quest, depth);

            if (fade)
                Fade(true, 0.25f);
        }
        /* Clears and draws baseQuest at depth. */
        private void DrawQuest(QG_Quest baseQuest, int depth)
        {
            foreach (Transform child in uiQuestPanel.transform)
            {
                Destroy(child.gameObject);
            }
            nodePosRegistry.Clear();

            uiQuestPanel.SetActive(true);

            // 1) sub quest handling -----------

            QG_Quest previousQuestToDraw = questToDraw;

            QG_Quest curQuest = baseQuest;
            int questToDrawDepth = -1;
            int depthCounter = 0;

            while (curQuest != null)
            {
                bool isActive = false;

                if (   (depth == -1 && curQuest.currentSubQuest == null)  // innermost level wanted & reached
                    || (depth != -1 && depthCounter == depth           )) // other level wanted & reached
                {
                    questToDraw = curQuest;
                    questToDrawDepth = depthCounter;
                    isActive = true;
                }

                DrawLayerButton(baseQuest, depthCounter, isActive);
                curQuest = curQuest.currentSubQuest;
                depthCounter++;
            }

            // 2) node registry -----------
            //
            // draw from left to right via breadth first search
            // nodes with same depth are stacked vertically

            HashSet<QG_EventPool> wavesAhead = new HashSet<QG_EventPool>(questToDraw.eventPools);
            wavesAhead.Remove(questToDraw.startPool);

            HashSet<QG_EventPool> wave = new HashSet<QG_EventPool>();
            wave.Add(questToDraw.startPool);

            HashSet<QG_EventPool> wavesBehind = new HashSet<QG_EventPool>();

            int waveCount = 0;

            int activeNodeDist = -1;


            while (wave.Count() != 0)
            {
                ComputeNodePositions(waveCount, wave); 

                HashSet<QG_EventPool> newWave = new HashSet<QG_EventPool>();

                foreach (QG_EventPool pool in wave)
                {
                    pool.wave = waveCount;

                    if (pool.IsActive())
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

            // 3) node draw  -----------
            //
            // activeNodeOffset ensures active node is always at same pos
            // distinction between nearby/faraway drawn nodes (left/right of dashed line)

            float activeNodeOffset = activeNodeDist < 0 ?
                0 : (activeNodeDist - 1) * HORIZ_GAP;

            int farawayDrawn = 0;

            foreach (QG_EventPool p in questToDraw.eventPools)
            {
                nodePosRegistry[p] = new Vector3(nodePosRegistry[p].x - activeNodeOffset, nodePosRegistry[p].y, 0);

                // nearby: nodes around active node depth
                if (0 <= (activeNodeDist - p.wave) && (activeNodeDist - p.wave) <= 2)
                    DrawPool(p, true);

                // faraway: nodes that border active node
                else if (-1 == (activeNodeDist - p.wave))
                    DrawPool(p, false);

                // faraway: capped number of random other nodes
                else if (Random.value > CHANCE_HIDE_FARAWAY && !(farawayDrawn >= 3))
                {
                    DrawPool(p, false);
                    farawayDrawn++;
                }
            }

            // 4) line draw -----------
            //
            // draw a line from pool A to pool B if pool A
            // contains an event that has an ending leading to B

            if (activeNodeDist >= 0 && activeNodeDist < 3)
            {
                DrawLine(new Vector3(ORIGIN.x - HORIZ_GAP - activeNodeOffset, 0, 0), 
                    nodePosRegistry[questToDraw.startPool], true);
            }

            foreach (QG_EventPool p1 in questToDraw.eventPools)
            {
                var outPools = new List<QG_EventPool>();
                var starred = new List<QG_EventPool>();

                foreach (QG_Event event_ in p1.pool)
                {
                    foreach (QG_EventPool p2 in event_.endingEventPools)
                    {
                        // forward arrow visibility radius is restrained
                        if ((activeNodeDist - p1.wave) < 0)
                            continue;

                        Vector3 pos1 = nodePosRegistry[p1];
                        Vector3 pos2 = nodePosRegistry[p2];
                        bool    used = p1.connUsed[p2];

                        if (!outPools.Contains(p2))
                        {
                            DrawLine(pos1, pos2, used);
                            outPools.Add(p2);
                        }
                        else if (!starred.Contains(p2))
                        {
                            DrawArrowStar(pos1, pos2, used);
                            starred.Add(p2);
                        }

                    }
                }
            }

            // 5) quest layer change anim & fade in -------------
            //
            // can occur if a change in quest layer occurs, e.g.
            // - change into subquest
            // - whole quest exchanged

            if (previousQuestToDraw != questToDraw)
            {
                if (questToDrawDepth != lastDepth)
                {
                    float duration = 0.5f;

                    GameObject leftUINode = Instantiate(uiLSwitchPrefab, uiQuestPanel.transform) as GameObject;
                    GameObject rightUINode = Instantiate(uiRSwitchPrefab, uiQuestPanel.transform) as GameObject;

                    if (questToDrawDepth > lastDepth) // into subquest
                    {
                        leftUINode.transform.Translate(new Vector3(0.0f, +0.25f, 0.0f));
                        rightUINode.transform.Translate(new Vector3(0.0f, +0.25f, 0.0f));
                    }
                    else if (questToDrawDepth < lastDepth) // out of subquest
                    {
                        leftUINode.transform.Rotate(new Vector3(180.0f, 0.0f, 0.0f));
                        rightUINode.transform.Rotate(new Vector3(180.0f, 0.0f, 0.0f));

                        leftUINode.transform.Translate(new Vector3(0.0f, 1.02f, 0.0f));
                        rightUINode.transform.Translate(new Vector3(0.0f, 1.02f, 0.0f));
                    }

                    StartCoroutine(LayerSwitchAnimation(duration, leftUINode, rightUINode));
                }

                Fade(true, 0.25f);
            }

            lastDepth = questToDrawDepth;
        }



        // ---------------------------------------------------------------------
        //                     methods to draw sub elements
        // ---------------------------------------------------------------------

        /* Plays an animation which lets two sets of arrows move and vanish. 
           If a set of arrows is rotated, then it'll move in the respective new direction. 
           https://forum.unity.com/threads/simple-ui-animation-fade-in-fade-out-c.439825/*/
        private IEnumerator LayerSwitchAnimation(float duration, GameObject arrows1, GameObject arrows2)
        {
            float DISTANCE = 1.27f;
            int   STEPS    = 100;

            for (int i = 0; i < STEPS; i++)
            {
                arrows1.transform.Translate(new Vector3(0.0f, - DISTANCE / STEPS, 0.0f));
                arrows2.transform.Translate(new Vector3(0.0f, - DISTANCE / STEPS, 0.0f));
                yield return new WaitForSeconds(duration / STEPS);
            }

            Destroy(arrows1); Destroy(arrows2);
        }

        /* Computes and saves positions in nodePosRegistry - for nodes with same depth. */
        private void ComputeNodePositions(int horizDist, HashSet<QG_EventPool> nodes)
        {
            float x = ORIGIN.x + horizDist * HORIZ_GAP;

            int nodesCount = nodes.Count();

            if (nodesCount == 0)
                return;

            else if (nodesCount == 1)
                nodePosRegistry[nodes.ElementAt(0)] = new Vector3(x, ORIGIN.y, 0);

            else
            {
                float yOffset = - (nodesCount - 1) * HORIZ_GAP / 2;

                for (int i = 0; i < nodesCount; i++)
                    nodePosRegistry[nodes.ElementAt(i)] = new Vector3(x, ORIGIN.y + yOffset + i * VERT_GAP, 0);
            }

        }

        /* Draws an event pool/node based on his registered position and type. */
        private void DrawPool(QG_EventPool node, bool nearby)
        {
            // boundary test

            Vector3 pos = nodePosRegistry[node];

            if (!UI_BOUNDARIES.Contains(pos))
                return;

            // draw

            GameObject newUINode = Instantiate(uiPoolButtonPrefab, uiQuestPanel.transform) as GameObject;
            newUINode.transform.Translate(pos);

            newUINode.transform.Find("Text").gameObject.GetComponent<Text>().text = node.name;

            // faraway

            if (! nearby)
            {
                if (node.pool.Count == 0 || node.pool[0] is NoChoiceTimedEvent || node.pool[0] is TutorialEvent)
                    newUINode.transform.Find("AheadMisc").gameObject.SetActive(true);

                else if (node.pool[0] is QG_Quest)
                    newUINode.transform.Find("AheadSubquest").gameObject.SetActive(true);

                else // is choice
                    newUINode.transform.Find("AheadChoice").gameObject.SetActive(true);

                return;
            }

            // nearby

            newUINode.transform.Find("HereBase").gameObject.SetActive(true);

            if (node.IsEnding() || node.pool[0] is NoChoiceTimedEvent || node.pool[0] is TutorialEvent)
            {
                if (node.IsActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereCircle-On").gameObject.SetActive(true);

                if (node.used)
                    newUINode.transform.Find("HereCircle").gameObject.SetActive(true);

                if (node.IsEnding())
                {
                    string suffix = (node.IsActive() || questToDraw.poolsQueue.Contains(node)) ? "-On" : "";

                    newUINode.transform.Find("HereL45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereL-45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereR45" + suffix).gameObject.SetActive(true);
                    newUINode.transform.Find("HereR-45" + suffix).gameObject.SetActive(true);
                }
            }

            else if (node.pool[0] is QG_Quest)
                {
                if (node.IsActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereSubquest-On").gameObject.SetActive(true);
                else
                    newUINode.transform.Find("HereSubquest").gameObject.SetActive(true);
            }

            else
            {
                if (node.IsActive() || questToDraw.poolsQueue.Contains(node))
                    newUINode.transform.Find("HereChoice-On").gameObject.SetActive(true);
                else
                    newUINode.transform.Find("HereChoice").gameObject.SetActive(true);
            }
        }

        /* Draws a line from start to end with color depending on used. 
           https://unitycoder.com/blog/2017/08/27/drawing-lines/ */
        private void DrawLine(Vector3 start, Vector3 end, bool used)
        {
            // boundary test

            bool startInside = UI_BOUNDARIES.Contains(start);
            bool endInside = UI_BOUNDARIES.Contains(end);

            float startPosScale = 1.0f;
            float endPosScale = 1.0f;

            if (!startInside && endInside)
            {
                start += (end - start) / 4;
                endPosScale = 0.75f;
            }

            else if (startInside && !endInside)
            {
                end -= (end - start) / 4;
                startPosScale = 0.75f;
            }

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
            lineRenderer.SetPosition(0, (end - start) * 28 * startPosScale);
            lineRenderer.SetPosition(1, (end - start) * 80 * endPosScale);
            lineRenderer.useWorldSpace = false;

        }

        /* Draws a star above a line, indicating several edges between start and end. */
        private void DrawArrowStar(Vector3 start, Vector3 end, bool used)
        {
            Vector3 pos = start + (end - start) / 2 + new Vector3(0.0f, 0.1f);// + new Vector3(1.15f, -0.5f);

            if (!UI_BOUNDARIES.Contains(pos))
                return;

            GameObject newUINode = Instantiate(uiStarPrefab, uiQuestPanel.transform) as GameObject;
            newUINode.transform.Translate(pos);

            if (used)
                newUINode.transform.Find("Text").gameObject.GetComponent<Text>().color = new Color(0.0f, 1.0f, 1.0f, 1.0f);

        }

        /* Draws a button that allows player changing quest layers in the quest Ui. */
        private void DrawLayerButton(QG_Quest baseQuest, int depth, bool isActive)
        {
            GameObject newUINode = Instantiate(uiLayerButtonPrefab, uiQuestPanel.transform) as GameObject;

            newUINode.transform.Translate(new Vector3(4.585f, - 0.196f - 0.2f * depth, 2));

            newUINode.GetComponent<Button>().onClick.AddListener(() => DrawQuest(baseQuest, depth, true));

            if (isActive)
                newUINode.GetComponent<Image>().color = new Color(0f, 1f, 0f, 1f);
            else
                newUINode.GetComponent<Image>().color = new Color(1f, 0f, 1f, 1f);

        }

    }
}
