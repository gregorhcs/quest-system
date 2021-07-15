﻿using Assets.Scripts;
using Assets.Scripts.QGSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject prefabDecisionLine;

    public GameObject optionsUI;
    public GameObject gameplayPanel;
    public GameObject textPanel;

    public GameObject ctPanel1;
    public GameObject ctPanel2;
    public GameObject ctPanel3;
    public GameObject ctPanel4;

    public GameObject scrollView;

    public AudioSource soundLogo, soundTrack;

    private int focusButtonIndex = 0;
    private GameObject focusButton;

    private QG_Quest quest;
    private QG_Event currentEvent;

    private List<GameObject> decisionLines;
    private Dictionary<GameObject, string> decisionLineToEnding;

    private bool drawQuest = false;

    private bool fadeInFinished = false;


    string introText = @"Cybertext 2020 © Night Corp.

> Transmission incoming ... 
| -----------------------------------------------------------------------------------------------------------------------------------
|
| Use ↑↓ or W/A to select options, press ENTER or E to confirm.
|
| Press TAB to toggle Quest UI visibility.
| Mind the small buttons in it on the top right, they let you look at other active (sub-)quest layers.
|
| Legal info: You have 5 min. to consume this product. Do not exceed the deadline. Do not modify
| the product. Do not make copies of the product. All illegal action WILL be punished severely.
|
| Enjoy!
|
| -----------------------------------------------------------------------------------------------------------------------------------

> Transmission End

> Awaiting user input ...";


    private void Awake()
    {
        Instance = this;

        decisionLines = new List<GameObject>();
        decisionLineToEnding = new Dictionary<GameObject, string>();
    }

    private void Start()
    {
        string[] a = { "end" };

        QG_EventPool tutorialPool = Resources.Load<QG_EventPool>("TestQuest/Pool0_Tutorial");

        tutorialPool.pool[0].callback = () =>
            textPanel.GetComponent<Text>().text = "Cybertext 2020 © Night Corp.\n\n";

        quest = new QG_Quest(
            "Wand'rer",
            Resources.Load<QG_EventPool>("TestQuest/Pool0_Tutorial"),
            new List<QG_EventPool>(Resources.LoadAll<QG_EventPool>("TestQuest")),
            new List<string>(a)
        );

        currentEvent = quest.NextEvent();

        LoadEvent(currentEvent);
        StartCoroutine(FadeInOut());

        soundLogo.Play();
    }

    IEnumerator FadeIn(SpriteRenderer image, float duration)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.0f);

        Color curColor = image.color;
        for (int i=0; i<100; i++)
        {
            curColor.a = i * 0.01f;
            image.color = curColor;
            yield return new WaitForSeconds(duration / 100.0f);
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, 1.0f);
    }

    IEnumerator FadeInOut()
    {
        gameplayPanel.SetActive(false);

        // fading in ct

        GameObject[] cts = { ctPanel1, ctPanel2, ctPanel3, ctPanel4 };

        ctPanel1.SetActive(true);
        StartCoroutine(FadeIn(ctPanel1.GetComponent<SpriteRenderer>(), 2.2f));
        yield return new WaitForSeconds(2.7f);
        ctPanel1.SetActive(false);

        for (int i=1; i<4; i++)
        {
            cts[i].SetActive(true);
            //StartCoroutine(FadeIn(cts[i].GetComponent<SpriteRenderer>(), 0.2f));
            yield return new WaitForSeconds(0.1f);
            cts[i].SetActive(false);
        }

        for (int i = 2; i >= 0; i--)
        {
            cts[i].SetActive(true);
            //StartCoroutine(FadeIn(cts[i].GetComponent<SpriteRenderer>(), 0.2f));
            yield return new WaitForSeconds(0.1f);

            if (i != 0)
                cts[i].SetActive(false);
        }

        yield return new WaitForSeconds(2.0f);
        cts[0].SetActive(false);

        // fading in gameplay

        gameplayPanel.SetActive(true);

        textPanel.GetComponent<Text>().CrossFadeAlpha(0, 0, false);
        textPanel.GetComponent<Text>().CrossFadeAlpha(1, 0.25f, false);

        if (drawQuest)
            QG_QuestUIHandler.Instance.DrawQuest(quest);

        if (optionsUI.transform.childCount > 0)
        {
            foreach (Transform t in optionsUI.transform)
            {
                t.GetChild(0).gameObject.GetComponent<Text>().CrossFadeAlpha(0, 0, false);
                t.gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0, false);

                t.GetChild(0).gameObject.GetComponent<Text>().CrossFadeAlpha(1, 0.25f, false);
                t.gameObject.GetComponent<Image>().CrossFadeAlpha(1, 0.25f, false);
            }

            yield return new WaitForSeconds(0.5f);
        }

        fadeInFinished = true;

    }

    IEnumerator ForceScrollDown()
    {
        // Wait for end of frame AND force update all canvases before setting to bottom.
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    IEnumerator Waiting(float seconds)
    {
        seconds -= 0.1f;

        string orig = textPanel.GetComponent<Text>().text;

        string[] waitingStrings = { "\n.", "\n..", "\n...", "\n" };
        int i = 0;

        while ((seconds - 0.25) >= 0)
        {
            textPanel.GetComponent<Text>().text = orig + waitingStrings[i];
            ForceScrollDown();
            yield return new WaitForSeconds(0.25f);
            seconds -= 0.25f;
            i = (i + 1) % 4;
        }

        textPanel.GetComponent<Text>().text = orig;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            string[] a = { "end" };

            QG_EventPool tutorialPool = Resources.Load<QG_EventPool>("TestQuest/Pool0_Tutorial");

            textPanel.GetComponent<Text>().text = introText;

            tutorialPool.pool[0].callback = () =>
                textPanel.GetComponent<Text>().text = "Cybertext 2020 © Night Corp.\n\n";

            quest = new QG_Quest(
                "Wand'rer",
                Resources.Load<QG_EventPool>("TestQuest/Pool0_Tutorial"),
                new List<QG_EventPool>(Resources.LoadAll<QG_EventPool>("TestQuest")),
                new List<string>(a)
            );

            currentEvent = quest.NextEvent();

            LoadEvent(currentEvent);

            if (drawQuest)
                QG_QuestUIHandler.Instance.DrawQuest(quest, -1, true);

            return;
        }

        if (fadeInFinished && Input.GetKeyDown(KeyCode.Tab))
        {
            if (drawQuest)
            {
                drawQuest = false;
                QG_QuestUIHandler.Instance.ClearAll();
            }
            else
            {
                drawQuest = true;
                QG_QuestUIHandler.Instance.DrawQuest(quest, -1, true);
            }
        }

        if (currentEvent == null || currentEvent is NoChoiceTimedEvent)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            quest.EventUpdate(currentEvent, decisionLineToEnding[focusButton]);
            currentEvent = quest.NextEvent();
            LoadEvent(currentEvent);

            if (currentEvent is NoChoiceTimedEvent)
                StartCoroutine(ProcessNoDecisionTimedEvents());

            StartCoroutine(ForceScrollDown());

            if (drawQuest)
                QG_QuestUIHandler.Instance.DrawQuest(quest);

            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            focusButtonIndex = (focusButtonIndex + 1) % optionsUI.transform.childCount;
            focusToButton(optionsUI.transform.GetChild(focusButtonIndex).gameObject);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            focusButtonIndex = focusButtonIndex - 1;
            
            if (focusButtonIndex < 0)
                focusButtonIndex = optionsUI.transform.childCount - 1;

            focusToButton(optionsUI.transform.GetChild(focusButtonIndex).gameObject);
        }
    }

    private IEnumerator ProcessNoDecisionTimedEvents()
    {
        // unload decision lines

        foreach (GameObject g in decisionLines)
            Destroy(g);

        decisionLines.Clear();
        decisionLineToEnding.Clear();

        // wait until next decision

        while (currentEvent != null && currentEvent is NoChoiceTimedEvent)
        {
            float secToWait = ((NoChoiceTimedEvent)currentEvent).secondsToWait;
            StartCoroutine(Waiting(secToWait));
            yield return new WaitForSeconds(secToWait);
            quest.EventUpdate(currentEvent, "standard");
            currentEvent = quest.NextEvent();
            LoadEvent(currentEvent);

            StartCoroutine(ForceScrollDown());

            if (drawQuest)
                QG_QuestUIHandler.Instance.DrawQuest(quest);
        }
    }


    private void LoadEvent(QG_Event event_)
    {
        // unload

        foreach (GameObject g in decisionLines)
            Destroy(g);

        decisionLines.Clear();
        decisionLineToEnding.Clear();

        // load

        if (currentEvent == null) // quest end
            return;

        CybertextEvent cyberEvent = (CybertextEvent)event_;

        textPanel.GetComponent<Text>().text += "\n\n" + cyberEvent.text;

        if (cyberEvent.decisionTexts.Count != 0)
        {
            for (int i = 0; i < event_.endings.Count; i++)
            {
                GameObject newDecisionLine = Instantiate(prefabDecisionLine, optionsUI.transform) as GameObject;
                newDecisionLine.transform.GetChild(0).GetComponent<Text>().text = "  " + cyberEvent.decisionTexts[i];
                decisionLines.Add(newDecisionLine);
                decisionLineToEnding[newDecisionLine] = cyberEvent.endings[i];

                if (i == 0)
                    focusToButton(newDecisionLine);
            }
        }

    }

    private void focusToButton(GameObject newFocusButton)
    {
        if (focusButton != null)
        {
            focusButton.transform.GetChild(0).gameObject.GetComponent<Text>().color = Color.cyan;
            focusButton.GetComponent<Image>().color = Color.clear;
        }

        focusButton = newFocusButton;

        focusButton.transform.GetChild(0).gameObject.GetComponent<Text>().color = Color.black;
        focusButton.GetComponent<Image>().color = Color.yellow;
    }
}