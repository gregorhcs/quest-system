using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class UISignal : MonoBehaviour
    {
        public GameObject uiSignalLayer;
        public GameObject uiLinePrefab;
        public GameObject uiSignalPrefab;

        private readonly List<GameObject> lines = new List<GameObject>();

        private void Start()
        {
            DrawLine(new Vector3(3.9f, 2.0f, 0), new Vector3(3.9f, 1.38f, 0), true);

            DrawLine(new Vector3(-5.4f, 2.0f, 0), new Vector3(4.65f, 2.0f, 0), true);

            DrawLine(new Vector3(-5.3f, 2.025f, 0), new Vector3(-5.3f, 0.7f, 0), true);


            DrawLine(new Vector3(-5.3f, -5.5f, 0), new Vector3(-5.3f, -5.7f, 0), true);

            DrawLine(new Vector3(-5.4f, -5.7f, 0), new Vector3(4.65f, -5.7f, 0), true);

            DrawLine(new Vector3(3.9f, -0.74f, 0), new Vector3(3.9f, -6.08f, 0), true);

            StartCoroutine(LineFadingLoop());
        }

        private IEnumerator LineFadingLoop()
        {
            // init

            float[] progress = new float[lines.Count];

            foreach (GameObject go in lines)
                progress[lines.IndexOf(go)] = lines.IndexOf(go) / (lines.Count + 1);

            bool forward = true;

            // loop

            while (true)
            {
                foreach (GameObject go in lines)
                {
                    LineRenderer lineRenderer = go.GetComponent<LineRenderer>();

                    float color = 0.7f + progress[lines.IndexOf(go)] * 0.3f;

                    lineRenderer.startColor = lineRenderer.endColor = new Color(0.0f, color, color, color);


                    if (forward)
                        progress[lines.IndexOf(go)] += 0.02f;
                    else
                        progress[lines.IndexOf(go)] -= 0.02f;


                    if (progress[lines.IndexOf(go)] >= 1.0f)
                    {
                        progress[lines.IndexOf(go)] = 1.0f;
                        forward = false;
                    }
                    if (progress[lines.IndexOf(go)] <= 0.0f)
                    {
                        progress[lines.IndexOf(go)] = 0.0f;
                        forward = true;
                    }
                }

                yield return new WaitForSeconds(0.016f);
            }
        }

        private IEnumerator StartSignals()
        {
            for (int i = 0; i < 5; i++)
            {
                StartCoroutine(MovingSignals());
                yield return new WaitForSeconds(3.0f);
            }
        }

        private IEnumerator MovingSignals()
        {
            GameObject newUISignal = Instantiate(uiSignalPrefab, uiSignalLayer.transform) as GameObject;

            newUISignal.transform.Translate(new Vector3(3.9f, -0.74f, 0.0f));

            float[] progress = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

            Vector3[] beats = { new Vector3(3.9f, -0.74f, 0),
                                new Vector3(3.9f, -2.51f, 0),
                                new Vector3(1.30f, -2.35f, 0),
                                new Vector3(1.35f, 1.06f, 0),
                                new Vector3(-5.35f, 1.0f, 0),
                                new Vector3(-5.3f, 0.7f, 0)
            };

            int currentBeat = 0;

            while (true)
            {
                newUISignal.transform.Translate(beats[currentBeat]);

                currentBeat += 1;
                if (currentBeat == beats.Length)
                    currentBeat = 0;

                yield return new WaitForSeconds(0.016f);
            }
        }

        private void DrawLine(Vector3 start, Vector3 end, bool used)
        {
            // drawing

            GameObject newUILine = Instantiate(uiLinePrefab, uiSignalLayer.transform) as GameObject;
            lines.Add(newUILine);

            newUILine.transform.Translate(start);

            LineRenderer lineRenderer = newUILine.GetComponent<LineRenderer>();

            lineRenderer.startColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            lineRenderer.endColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);

            lineRenderer.startWidth = 0.03f;
            lineRenderer.endWidth = 0.03f;
            lineRenderer.SetPosition(0, (end - start) * 1);
            lineRenderer.SetPosition(1, (end - start) * 100);
            lineRenderer.useWorldSpace = false;

        }

    }
}
