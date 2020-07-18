using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

public class FlavorTextCruel : MonoBehaviour
{
    public TextAsset flavorTextJson;
    public Text textDisplay;
    public KMSelectable[] buttons;
    public MeshRenderer[] leds;
    public KMBombInfo bombInfo;

    public Material off;
    public Material green;
    public Material red;
    
    List<FlavorTextOption> textOptions;
    FlavorTextOption textOption;
    bool isActive = false;
    int[] corrAnswers1;
    int[] corrAnswers2;
    int[] buttonNumbers;
    bool[] buttonStates;
    List<long> moduleIds;
    int stage = 0;
    int maxStageAmount = 3;
    static int _moduleIdCounter = 1;
    int _moduleId = 0;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        stage = 0;
        isActive = true;
        Debug.LogFormat("[Flavor Text EX #{0}] It's on.", _moduleId);
        for(int i = 0; i < buttons.Count(); i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
        Randomize();
    }

    void OnReactivate()
    {
        isActive = true;
        Debug.LogFormat("[Flavor Text EX #{0}] It's back on.", _moduleId);
        Randomize();
    }
    
    void Randomize()
    {
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = off;
        }
        corrAnswers1 = new[] { -1, -1, -1, -1 };
        corrAnswers2 = new[] { -1, -1, -1, -1 };
        buttonStates = new[] {false, false, false, false};
        buttonNumbers = new[] {0, 0, 0, 0};
        buttonNumbers[0] = UnityEngine.Random.Range(0, 10);
        buttonNumbers[1] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[1] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[1] == buttonNumbers[0]);
        buttonNumbers[2] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[2] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[2] == buttonNumbers[0] || buttonNumbers[2] == buttonNumbers[1]);
        buttonNumbers[3] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[3] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[3] == buttonNumbers[0] || buttonNumbers[3] == buttonNumbers[1] || buttonNumbers[3] == buttonNumbers[2]);
        string choice = "";
        for(int i = 0; i < buttons.Length; i++)
        {
            string label = buttonNumbers[i].ToString();
            choice += label;
            TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
            buttonText.text = label;
        }

        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
        moduleIds = new List<long>();
        for (int i = 0; i < textOptions.Count; i++)
        {
            if (textOptions[i].text == textOption.text && !moduleIds.Contains(textOptions[i].steam_id))
            {
                moduleIds.Add(textOptions[i].steam_id);
            }
        }
		
        Debug.LogFormat("[Flavor Text EX #{0}] It said: {1}", _moduleId, textOption.text);
        if (textOption.text == "And here's the Countdown clock...")
        {
            Debug.LogFormat("[Flavor Text EX #{0}] It's looking for (Cruel) Countdown.", _moduleId);
            string log1 = "";
            string log2 = "";
            for (int i = 0; i < 2; i++)
            {
                int ct = 0;
                if (i == 0)
                {
                    for (int j = 0; j < textOptions[145].steam_id.ToString().Length; j++)
                    {
                        if (buttonNumbers.Contains(int.Parse(textOptions[145].steam_id.ToString().ElementAt(j) + "")) && !corrAnswers1.Contains(int.Parse(textOptions[145].steam_id.ToString().ElementAt(j) + "")))
                        {
                            corrAnswers1[ct] = int.Parse(textOptions[145].steam_id.ToString().ElementAt(j) + "");
                            ct++;
                        }
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        if (j >= ct)
                            log1 += "#";
                        else
                            log1 += corrAnswers1[j];
                    }
                }
                else if (i == 1)
                {
                    for (int j = 0; j < textOptions[151].steam_id.ToString().Length; j++)
                    {
                        if (buttonNumbers.Contains(int.Parse(textOptions[151].steam_id.ToString().ElementAt(j) + "")) && !corrAnswers2.Contains(int.Parse(textOptions[151].steam_id.ToString().ElementAt(j) + "")))
                        {
                            corrAnswers2[ct] = int.Parse(textOptions[151].steam_id.ToString().ElementAt(j) + "");
                            ct++;
                        }
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        if (j >= ct)
                            log2 += "#";
                        else
                            log2 += corrAnswers2[j];
                    }
                }
            }
            Debug.LogFormat("[Flavor Text EX #{0}] It offered you a choice of {1}. It's looking for {2} or {3}.", _moduleId, choice, log1, log2);
        }
        else
        {
            Debug.LogFormat("[Flavor Text EX #{0}] It's looking for {1}.", _moduleId, textOption.name);
            int ct = 0;
            for (int j = 0; j < textOption.steam_id.ToString().Length; j++)
            {
                if (buttonNumbers.Contains(int.Parse(textOption.steam_id.ToString().ElementAt(j) + "")) && !corrAnswers1.Contains(int.Parse(textOption.steam_id.ToString().ElementAt(j) + "")))
                {
                    corrAnswers1[ct] = int.Parse(textOption.steam_id.ToString().ElementAt(j) + "");
                    ct++;
                }
            }
            string log = "";
            for (int j = 0; j < 4; j++)
            {
                if (j >= ct)
                    log += "#";
                else
                    log += corrAnswers1[j];
            }
            Debug.LogFormat("[Flavor Text EX #{0}] It offered you a choice of {1}. It's looking for {2}.", _moduleId, choice, log);
        }
    }
    
    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pressedButton].transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (isActive)
        {
            Debug.LogFormat("[Flavor Text EX #{0}] You chose {1}.", _moduleId, buttonNumbers[pressedButton]);
            bool buttonIsCorrect = !buttonStates[pressedButton];
            List<long> moduleIdsCopy = new List<long>(moduleIds);
            foreach (int id in moduleIds) {
                string steamId = id.ToString();
                for (int i = 0; i < buttonNumbers.Count(); i++)
                {
                    if (!buttonStates[i] && (steamId.IndexOf(buttonNumbers[pressedButton].ToString()) > steamId.IndexOf(buttonNumbers[i].ToString()) || steamId.IndexOf(buttonNumbers[pressedButton].ToString()) < 0) && steamId.IndexOf(buttonNumbers[i].ToString()) >= 0 && textOption.steam_id > 0)
                    {
                        moduleIdsCopy.Remove(id);
                        break;
                    }
                }
            }
            moduleIds = moduleIdsCopy;
            if (moduleIds.Count() == 0)
            {
                buttonIsCorrect = false;
            }
            if (buttonIsCorrect)
            {
                buttonStates[pressedButton] = true;
                leds[pressedButton].material = green;
                Debug.LogFormat("[Flavor Text EX #{0}] It accepted your choice.", _moduleId);
                if (buttonStates[0] && buttonStates[1] && buttonStates[2] && buttonStates[3])
                {
                    stage++;
                    Debug.LogFormat("[Flavor Text EX #{0}] It became more content.", _moduleId);
                    if (stage == maxStageAmount)
                    {
                        Debug.LogFormat("[Flavor Text EX #{0}] Flavor Text EX was spared.", _moduleId);
                        textDisplay.text = "";
                        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        GetComponent<KMBombModule>().HandlePass();
                        isActive = false;
                    }
                    else
                    {
                        Randomize();
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Flavor Text EX #{0}] It refused your choice.", _moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                StartCoroutine(RedLights());
            }
        }
    }
    
    IEnumerator RedLights()
    {
        isActive = false;
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = red;
        }
        yield return new WaitForSeconds(1f);
        stage = 0;
        OnReactivate();
    }

    //twitch plays
    private bool cmdIsValid1(string cmd)
    {
        char[] valids = { '1','2','3','4' };
        if((cmd.Length >= 1) && (cmd.Length <= 4))
        {
            foreach(char c in cmd)
            {
                if (!valids.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool cmdIsValid2(string cmd)
    {
        int[] valids = { buttonNumbers[0], buttonNumbers[1], buttonNumbers[2], buttonNumbers[3] };
        if ((cmd.Length >= 1) && (cmd.Length <= 4))
        {
            foreach (char c in cmd)
            {
                int test = (int)Char.GetNumericValue(c);
                if (!valids.Contains(test))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    // twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} position/pos/p 1234 [Presses the buttons from top to bottom] | !{0} label/lab/l 6805 [Presses the buttons labelled '6','8','0', then '5']";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*position\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*pos\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*p\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (cmdIsValid1(parameters[1]))
                {
                    foreach (char c in parameters[1])
                    {
                        int temp = int.Parse(c+"");
                        buttons[temp-1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    yield return "sendtochaterror One or more of the specified positions '" + parameters[1] + "' are invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the positions of the buttons you wish to press!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*label\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*lab\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*l\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (cmdIsValid2(parameters[1]))
                {
                    foreach (char c in parameters[1])
                    {
                        int temp = int.Parse(c+"");
                        buttons[Array.FindIndex(buttonNumbers, x => x.Equals(temp))].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    yield return "sendtochaterror One or more of the specified labels '" + parameters[1] + "' are invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the labels of the buttons you wish to press!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!isActive) { yield return true; yield return new WaitForSeconds(0.1f); }
        int start = stage;
        for (int i = start; i < maxStageAmount; i++)
        {
            int rando = UnityEngine.Random.Range(0, 2);
            for (int j = buttonStates.Where(c => c).Count(); j < 4; j++)
            {
                for (int g = 0; g < 4; g++)
                {
                    if (!buttonStates[g])
                    {
                        if (textOption.text == "And here's the Countdown clock...")
                        {
                            if (rando == 0)
                            {
                                if (corrAnswers1[j] == -1)
                                {
                                    buttons[g].OnInteract();
                                    yield return new WaitForSeconds(0.1f);
                                    break;
                                }
                                else if (corrAnswers1[j] == buttonNumbers[g])
                                {
                                    buttons[g].OnInteract();
                                    yield return new WaitForSeconds(0.1f);
                                    break;
                                }
                            }
                            else if (rando == 1)
                            {
                                if (corrAnswers2[j] == -1)
                                {
                                    buttons[g].OnInteract();
                                    yield return new WaitForSeconds(0.1f);
                                    break;
                                }
                                else if (corrAnswers2[j] == buttonNumbers[g])
                                {
                                    buttons[g].OnInteract();
                                    yield return new WaitForSeconds(0.1f);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (corrAnswers1[j] == -1)
                            {
                                buttons[g].OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            }
                            else if (corrAnswers1[j] == buttonNumbers[g])
                            {
                                buttons[g].OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
