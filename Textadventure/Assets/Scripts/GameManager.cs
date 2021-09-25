using Assets.Scripts;
using Assets.Scripts.QGSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/* This singleton handles the entire Cybertext-specific functionality. 
 *
 * In the beginning the manager distinguishes three phases:
 * 1. wait for initial enter ("breach")
 * 2. logo animation is playing
 * 3. gameplay UI is fading in
 * 4. gameplay
 * 
 * The central function is Update, which manages all user input.
 *
 */
public class GameManager : MonoBehaviour
{
    // Statics

    public static GameManager Instance;

    // Prefabs & AudioSources

    public GameObject prefabDecisionLine;

    public GameObject optionsUI;
    public GameObject gameplayPanel;
    public GameObject textPanel;

    public GameObject logoFrame1;
    public GameObject logoFrame2;
    public GameObject logoFrame3;
    public GameObject logoFrame4;

    public GameObject scrollView;

    public GameObject breachButton;

    public AudioSource soundLogo;
    public AudioSource soundTrack;

    // base quest that is played and its
    // currently active event
    private QG_Quest quest;
    private QG_Event currentEvent;

    // current decisions lines and how
    // they map to the active events endings
    private List<GameObject> decisionLines;
    private Dictionary<GameObject, string> decisionLineToEnding;

    // decision line that currently has focus
    private int focusButtonIndex = 0;
    private GameObject focusButton = null;

    private bool waitForBreach  = true;   // are we waiting for the first Return?
    private bool fadeInFinished = false;  // has gameplay UI faded in?
    private bool drawQuest      = true;  // has the user toggled quest drawing to active?


    /* Phase 1: Initializes the game manager. */
    private void Awake()
    {
        Instance = this;

        decisionLines = new List<GameObject>();
        decisionLineToEnding = new Dictionary<GameObject, string>();

        gameplayPanel.SetActive(false);
    }

    /* Phase 2: Loads the quest + first event, issues the logo animation and starts sound. */
    private void StartIntro()
    {
        string[] a = { "end" };

        QG_EventPool tutorialPool = Resources.Load<QG_EventPool>("MainQuest/Pools/Pl_00Tut_D");

        tutorialPool.pool[0].callback = () =>
            textPanel.GetComponent<Text>().text = "Cybertext 2020 © Night Corp.\n";

        quest = new QG_Quest(
            "Wand'rer",
            Resources.Load<QG_EventPool>("MainQuest/Pools/Pl_00Tut_D"),
            new List<QG_EventPool>(Resources.LoadAll<QG_EventPool>("MainQuest")),
            new List<string>(a)
        );

        currentEvent = quest.NextEvent();

        LoadEvent(currentEvent);
        StartCoroutine(TransitionToGameplay());

        soundLogo.Play();
        soundTrack.Play();
    }

    /* Helper function that fades an image in duration seconds. 
       https://forum.unity.com/threads/simple-ui-animation-fade-in-fade-out-c.439825/ */
    private IEnumerator FadeIn(SpriteRenderer image, float duration)
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

    /* Phase 2-3: Plays the logo animation, then fades in gameplay UI */
    IEnumerator TransitionToGameplay()
    {
        // fading in/out logo frames

        GameObject[] frames = { logoFrame1, logoFrame2, logoFrame3, logoFrame4 };

        logoFrame1.SetActive(true);
        StartCoroutine(FadeIn(logoFrame1.GetComponent<SpriteRenderer>(), 2.2f));
        yield return new WaitForSeconds(2.7f);
        logoFrame1.SetActive(false);

        for (int i=1; i<4; i++)
        {
            frames[i].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            if (i != 0)
                frames[i].SetActive(false);
        }

        for (int i = 2; i >= 0; i--)
        {
            frames[i].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            if (i != 0)
                frames[i].SetActive(false);
        }

        yield return new WaitForSeconds(0.9f);
        frames[0].SetActive(false);

        // fading in/out logo frames 2nd time

        for (int i = 1; i < 3; i++)
        {
            frames[i].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            if (i != 0)
                frames[i].SetActive(false);
        }

        for (int i = 1; i >= 0; i--)
        {
            frames[i].SetActive(true);
            yield return new WaitForSeconds(0.1f);

            if (i != 0)
                frames[i].SetActive(false);
        }

        yield return new WaitForSeconds(2.5f);
        frames[0].SetActive(false);

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

    /* Forces the text field to scroll to the bottom. 
       https://forum.unity.com/threads/scroll-to-the-bottom-of-a-scrollrect-in-code.310919/#post-3461657 */
    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    /* Plays the idle animation in the text field during NoChoiceTimedEvents. */
    IEnumerator WaitingAnimation(float seconds)
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

    /* Responsible for all user input. */
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        // phase 1 ends when Return is pressed
        if (waitForBreach && Input.GetKeyDown(KeyCode.Return))
        {
            waitForBreach = false;
            breachButton.SetActive(false);
            StartIntro();
            return;
        }

        // game reset
        // TODO: bugged
        /*if (Input.GetKeyDown(KeyCode.F7))
        {
            string[] a = { "end" };

            QG_EventPool tutorialPool = Resources.Load<QG_EventPool>("MainQuest/Pools/Pl_00Tut_D");

            textPanel.GetComponent<Text>().text = Resources.Load<TextAsset>("IntroText").text;

            tutorialPool.pool[0].callback = () =>
                textPanel.GetComponent<Text>().text = "Cybertext 2020 © Night Corp.\n\n";

            quest = new QG_Quest(
                "Wand'rer",
                Resources.Load<QG_EventPool>("MainQuest/Pools/Pool0_Tutorial"),
                new List<QG_EventPool>(Resources.LoadAll<QG_EventPool>("MainQuest")),
                new List<string>(a)
            );

            currentEvent = quest.NextEvent();

            LoadEvent(currentEvent);

            if (drawQuest)
                QG_QuestUIHandler.Instance.DrawQuest(quest, -1, true);

            return;
        }*/

        // toggle quest UI visibility
        if (fadeInFinished && Input.GetKeyDown(KeyCode.Tab))
        {
            if (drawQuest)
            {
                drawQuest = false;
                QG_QuestUIHandler.Instance.Deactivate();
            }
            else
            {
                drawQuest = true;
                QG_QuestUIHandler.Instance.DrawQuest(quest, -1, true);
            }
        }

        // none of the folowing user input is sensible when one of these
        // cases is true
        if (currentEvent == null || currentEvent is NoChoiceTimedEvent)
            return;

        // player commits to a selected decision line
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            textPanel.GetComponent<Text>().text += "\n\n[USER DECISION]  »" + focusButton.transform.GetChild(0).GetComponent<Text>().text;

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

        // player switches decision lines
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Index Pre " + focusButtonIndex);

            focusButtonIndex += 1;

            if (focusButtonIndex > optionsUI.transform.childCount - 1)
                focusButtonIndex = 0;

            Debug.Log("Index Seq " + focusButtonIndex);

            FocusToButton(optionsUI.transform.GetChild(focusButtonIndex).gameObject);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Index Pre " + focusButtonIndex);

            focusButtonIndex -= 1;
            
            if (focusButtonIndex < 0)
                focusButtonIndex = optionsUI.transform.childCount - 1;

            Debug.Log("Index Seq " + focusButtonIndex);

            FocusToButton(optionsUI.transform.GetChild(focusButtonIndex).gameObject);
        }
    }

    /* Handles NoDecisionTimedEvents. */
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
            StartCoroutine(WaitingAnimation(secToWait));
            yield return new WaitForSeconds(secToWait);
            quest.EventUpdate(currentEvent, "standard");
            currentEvent = quest.NextEvent();
            LoadEvent(currentEvent);

            StartCoroutine(ForceScrollDown());

            if (drawQuest)
                QG_QuestUIHandler.Instance.DrawQuest(quest);
        }
    }


    /* Loads the content of event_ into the gameplay UI. */
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
                    FocusToButton(newDecisionLine);
            }
        }

        focusButtonIndex = 0;

    }

    /* Switches focus of decision lines to newFocusButton. */
    private void FocusToButton(GameObject newFocusButton)
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