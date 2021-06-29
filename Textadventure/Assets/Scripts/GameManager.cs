using Assets.Scripts;
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

    private int focusButtonIndex = 0;
    private GameObject focusButton;

    private QG_Quest quest;
    private QG_Event currentEvent;

    private List<GameObject> decisionLines;
    private Dictionary<GameObject, string> decisionLineToEnding;

    private bool drawQuest = false;

    private bool fadeInFinished = false;

    private void Awake()
    {
        Instance = this;

        decisionLines = new List<GameObject>();
        decisionLineToEnding = new Dictionary<GameObject, string>();
    }

    private void Start()
    {
        string[] a = { "end" };

        quest = new QG_Quest(
            "Wand'rer",
            Resources.Load<QG_EventPool>("TestQuest/Pool1_Start"),
            new List<QG_EventPool>(Resources.LoadAll<QG_EventPool>("TestQuest")),
            new List<string>(a)
        );

        currentEvent = quest.NextEvent();

        LoadEvent(currentEvent);
        StartCoroutine(FadeInOut());
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
            //QG_QuestUIHandler.Instance.DrawQuestAndFadeIn(quest, 0.25f);

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

    private void Update()
    {
        if (currentEvent == null)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            quest.EventUpdate(currentEvent, decisionLineToEnding[focusButton]);
            currentEvent = quest.NextEvent();
            LoadEvent(currentEvent);

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
                QG_QuestUIHandler.Instance.DrawQuest(quest);
            }
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