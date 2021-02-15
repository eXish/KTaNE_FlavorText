using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;

public class FlavorTextOption
{
    public String name;
    public long steam_id;
    public String module_id;
    public String text;
}

public class FlavorText : MonoBehaviour
{
    public TextAsset flavorTextJson;
    public Text textDisplay;
    public KMSelectable[] buttons;
    public KMBombInfo bombInfo;

    List<FlavorTextOption> textOptions;
    FlavorTextOption textOption;
    bool isActive = false;
    bool beSpecial = false;
    List<string> moduleIds;
    static int _moduleIdCounter = 1;
    int _moduleId = 0;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        moduleIds = bombInfo.GetModuleIDs();
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        isActive = true;
        Debug.LogFormat("[Flavor Text #{0}] It's on.", _moduleId);
        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
        if (textOption.text == "And here's the Countdown clock...")
        {
            beSpecial = true;
            Debug.LogFormat("[Flavor Text #{0}] It's looking for (Cruel) Countdown.", _moduleId);
        }
        else
        {
            Debug.LogFormat("[Flavor Text #{0}] It's looking for {1}.", _moduleId, textOption.name);
        }
        string modname = textOption.text;
        if (textOption.module_id == "brainf")
            modname = modname.Replace("\n", "");
        else
            modname = modname.Replace("\n", " ");
        Debug.LogFormat("[Flavor Text #{0}] It said: {1}", _moduleId, modname);
        if (moduleIds.Contains(textOption.module_id) || (beSpecial && (moduleIds.Contains("countdown") || moduleIds.Contains("cruelCountdown"))))
        {
            Debug.LogFormat("[Flavor Text #{0}] Do you accept it? (You probably should...)", _moduleId);
        }
        else
        {
            Debug.LogFormat("[Flavor Text #{0}] Do you accept it? (You probably shouldn't...)", _moduleId);
        }
        for (int i = 0; i < buttons.Count(); i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
    }

    void OnReactivate()
    {
        beSpecial = false;
        isActive = true;
        Debug.LogFormat("[Flavor Text #{0}] It's back on.", _moduleId);
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
        if (textOption.text == "And here's the Countdown clock...")
        {
            beSpecial = true;
            Debug.LogFormat("[Flavor Text #{0}] It's looking for (Cruel) Countdown.", _moduleId);
        }
        else
        {
            Debug.LogFormat("[Flavor Text #{0}] It's looking for {1}.", _moduleId, textOption.name);
        }
        Debug.LogFormat("[Flavor Text #{0}] It said: {1}", _moduleId, textOption.text);
        if (moduleIds.Contains(textOption.module_id) || (beSpecial && (moduleIds.Contains("countdown") || moduleIds.Contains("cruelCountdown"))))
        {
            Debug.LogFormat("[Flavor Text #{0}] Do you accept it? (You probably should...)", _moduleId);
        }
        else
        {
            Debug.LogFormat("[Flavor Text #{0}] Do you accept it? (You probably shouldn't...)", _moduleId);
        }
    }

    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pressedButton].transform);
        buttons[pressedButton].AddInteractionPunch();

        if (isActive)
        {
            Debug.LogFormat("[Flavor Text #{0}] You chose {1}to accept.", _moduleId, (pressedButton == 0) ? "not " : "");
            if (((pressedButton > 0) == moduleIds.Contains(textOption.module_id)) || ((pressedButton > 0) && beSpecial && (moduleIds.Contains("countdown") || moduleIds.Contains("cruelCountdown"))))
            {
                Debug.LogFormat("[Flavor Text #{0}] Flavor Text was spared.", _moduleId);
                textDisplay.text = "";
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                GetComponent<KMBombModule>().HandlePass();
                isActive = false;
            }
            else
            {
                Debug.LogFormat("[Flavor Text #{0}] But it refused.", _moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                OnReactivate();
            }
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} yes/y [Presses the yes button] | !{0} no/n [Presses the no button]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (parameters[1].EqualsIgnoreCase("yes") || parameters[1].EqualsIgnoreCase("y"))
                {
                    buttons[1].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("no") || parameters[1].EqualsIgnoreCase("n"))
                {
                    buttons[0].OnInteract();
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the button to press!";
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*yes\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*y\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[1].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*no\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*n\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[0].OnInteract();
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!isActive) { yield return true; }
        if (moduleIds.Contains(textOption.module_id) || (beSpecial && (moduleIds.Contains("countdown") || moduleIds.Contains("cruelCountdown"))))
        {
            buttons[1].OnInteract();
        }
        else
        {
            buttons[0].OnInteract();
        }
        yield return new WaitForSeconds(0.1f);
    }
}
