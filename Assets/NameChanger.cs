﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class NameChanger : MonoBehaviour
{
    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio MAudio;
    public KMSelectable[] buttons;
    public KMSelectable submitBtn;
    public TextMesh netext;

    private bool solved = false;
    private int moduleId;
    private static int counter = 1;
    private int currentLetter, currentWord, startingWord, startingLetter, chosenLetter = 0, chosenWord = 0;
    private bool auto;
    private static readonly string[] words =
    {
        "STEREOTYPE",
        "MICROPHONE",
        "PRESIDENCY",
        "ACCEPTANCE",
        "PRODUCTIVE",
        "ARTICULATE",
        "CONSTRAINT",
        "INVESTMENT",
        "CONCLUSION",
        "DISABILITY",
        "INHABITANT",
        "PSYCHOLOGY",
        "ACCOUNTANT",
        "DEFICIENCY",
        "ATTRACTIVE",
        "BASKETBALL",
        "EXPRESSION",
        "MASTERMIND",
        "ACCEPTABLE",
        "THOUGHTFUL",
        "TOURNAMENT",
        "POPULATION",
        "ASSUMPTION",
        "LITERATURE"
    };
    private string SelectedWord;
    private string InitialWord;

    void Start()
    {
        moduleId = counter++;

        for (int x = 0; x < buttons.Length; x++)
        {
            int y = x;
            buttons[x].OnInteract += delegate
            {
                MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[y].transform);
                if (!solved)
                {
                    switch (y)
                    {
                        case 0:
                            LeftPress();
                            break;
                        case 1:
                            RightPress();
                            break;
                        case 2:
                            UpPress();
                            break;
                        case 3:
                            DownPress();
                            break;
                    }
                }
                return false;
            };

        }

        submitBtn.OnInteract += delegate
        {
            MAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitBtn.transform);
            if (!solved)
                HandleSubmit();
            return false;
        };

        module.OnActivate += Activation;
    }

    void GenerateInitialSet()
    {
        startingWord = Rnd.Range(0, words.Length); // Generate a starting word index from 0,23 inclusive

        currentWord = startingWord;
        SelectedWord = InitialWord = words[currentWord];
        Debug.LogFormat("[Name Changer #{0}] Starting on the word: {1}, at position {2} on the table in reading order.", moduleId, InitialWord, currentWord + 1);

        startingLetter = Rnd.Range(0, 10);

        currentLetter = startingLetter;
        Debug.LogFormat("[Name Changer #{0}] Starting letter from the given word: {1} (the {2} letter)", moduleId, InitialWord[currentLetter], new[] { 0, 1, 2 }.Contains(startingLetter) ? new[] { "1st", "2nd", "3rd" }[startingLetter] : ((startingLetter + 1).ToString() + "th"));
        ChangeLetter();
    }

    void GenerateSolution()
    {
        chosenLetter = (startingWord + bomb.GetIndicators().Count()) % 10;
        Debug.LogFormat("[Name Changer #{0}] The letter to choose is the {1} letter.", moduleId, new[] { 0, 1, 2 }.Contains(chosenLetter) ? new[] { "1st", "2nd", "3rd" }[chosenLetter] : ((chosenLetter + 1).ToString() + "th"));

        chosenWord = (startingWord + bomb.GetPortCount()) % 24;
        Debug.LogFormat("[Name Changer #{0}] The target word to choose is {1}, which is in position {2} on the table in reading order.", moduleId, words[chosenWord], chosenWord + 1);
    }

    void Activation()
    {
        GenerateInitialSet();
        GenerateSolution();
    }

    void ChangeLetter()
    {
        netext.text = SelectedWord.Substring(currentLetter, 1);
    }

    void NewWord()
    {
        SelectedWord = words[currentWord];
        ChangeLetter();
    }

    void LeftPress()
    {
        currentLetter = Mathf.Max(0, currentLetter - 1);
        ChangeLetter();
    }

    void RightPress()
    {
        currentLetter = Mathf.Min(9, currentLetter + 1);
        ChangeLetter();
    }

    void UpPress()
    {
        currentWord = Mathf.Max(0, currentWord - 1);
        NewWord();
    }

    void DownPress()
    {
        currentWord = Mathf.Min(23, currentWord + 1);
        NewWord();
    }

    void HandleSubmit()
    {
        if (auto)
        {
            Correct();
        }
        else
        {
            string highlightedWord = words[currentWord];
            char s = highlightedWord[currentLetter];

            Debug.LogFormat("[Name Changer #{0}] Attempting to submit on the word: {1}", moduleId, highlightedWord);
            Debug.LogFormat("[Name Changer #{0}] Attempting to submit the following letter from the word: {1} ({2} letter)", moduleId, s, new[] { 0, 1, 2 }.Contains(currentLetter) ? new[] { "1st", "2nd", "3rd" }[currentLetter] : ((currentLetter + 1).ToString() + "th"));

            if (chosenLetter == currentLetter && chosenWord == currentWord)
                Correct();
            else
                Incorrect();
        }
    }

    void Correct()
    {
        solved = true;
        module.HandlePass();
        Debug.LogFormat("[Name Changer #{0}] Module solved.", moduleId);
        netext.text = auto ? "Forced\nSolved" : "Good Job!";
        netext.transform.localPosition = new Vector3(0, 0.0151f, 0);
        netext.fontSize = 65;
        buttons[0].transform.Translate(x: 0, y: (float) -0.01, z: 0);
        buttons[1].transform.Translate(x: 0, y: (float) -0.01, z: 0);
        buttons[2].transform.Translate(x: 0, y: (float) -0.01, z: 0);
        buttons[3].transform.Translate(x: 0, y: (float) -0.01, z: 0);
        submitBtn.transform.Translate(x: 0, y: (float) -0.01, z: 0);
    }

    void Incorrect()
    {
        Debug.LogFormat("[Name Changer #{0}] The letter deleted is incorrect!", moduleId);
        module.HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} left/right/up/down 3 [press the specified button that many times] | !{0} submit [submit the selected letter] | !{0} reset/restart [go back to the initial letter and word]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^(l(eft)?|r(ight)?|u(p)?|d(own)?)\s\d+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            string[] splittedParts = command.ToLowerInvariant().Split();
            Dictionary<char, KMSelectable> directionsToButtons = new Dictionary<char, KMSelectable>() {
                {'l',buttons[0] },
                {'r',buttons[1] },
                {'u',buttons[2] },
                {'d',buttons[3] },
            };

            int timesToPress;
            if (!directionsToButtons.ContainsKey(splittedParts[0].ElementAtOrDefault(0)))
            {
                yield return string.Format("sendtochaterror There are no buttons to press in conjunction with a specified direction.");
                yield break;
            }
            KMSelectable curSelected = directionsToButtons[splittedParts[0][0]];
            if (!int.TryParse(splittedParts[1], out timesToPress) || timesToPress <= 0 || timesToPress > 24)
            {
                yield return string.Format("sendtochaterror The module does not want to accept \"{0}\" as a valid number of times to press.", splittedParts[1]);
                yield break;
            }
            for (int x = 0; x < timesToPress; x++)
            {
                yield return null;
                curSelected.OnInteract();
            }
        }
        else if (Regex.IsMatch(command, @"^submit$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            submitBtn.OnInteract();
        }
        else if (Regex.IsMatch(command, @"^res(et|tart)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            currentLetter = startingLetter;
            currentWord = startingWord;
            NewWord();
            ChangeLetter();
            yield return "sendtochat I have now resetted Name Changer (#{1}) to the initial letter and word.";
            yield break;

        }
        else
        {
            yield return string.Format("sendtochaterror I do not know of a command {0} in the module, check your command for typos.", command);
            yield break;
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        auto = true;

        while (currentWord != chosenWord)
        {
            yield return null;
            if (currentWord > chosenWord)
                buttons[2].OnInteract();
            else
                buttons[3].OnInteract();
        }

        while (currentLetter != chosenLetter)
        {
            yield return null;
            if (currentLetter > chosenLetter)
                buttons[0].OnInteract();
            else
                buttons[1].OnInteract();
        }
        submitBtn.OnInteract();
    }
}
