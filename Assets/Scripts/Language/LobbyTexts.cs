using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System;

public static class LobbyTexts {
    private static HashSet<string> gameobjects_with_texts = new HashSet<string> {
        "HostButton",
        "ClientButton",
        "JoinCodeInput",
        "JoinCodeInfo",
        "StartButton",
        "NickInfo",
        "Language_info"
    };

    private static Dictionary<string, string> extra_text = new Dictionary<string, string>() {
        {"HostButton",""},
        {"ClientButton",""},
        {"JoinCodeInput",""},
        {"JoinCodeInfo",""},
        {"StartButton",""},
        {"NickInfo",""},
        {"Language_info",""}
    };

    private static string language_version = "EN";
    private static string languages_directory = "Languages/";
    private static bool buttons_hided = false;

    public static void AddText(string key, string new_text) {
        extra_text[key] += new_text;
    }

    public static void ReplaceText(string key, string new_text) {
        extra_text[key] = new_text;
    }

    public static void Sync() {
        ChangeVersion(language_version);
    }

    public static void HideButtons() {
        buttons_hided = true;
    }

    public static void ChangeVersion(string version) {
        language_version = version;
        
        TextAsset txt = (TextAsset)Resources.Load(languages_directory + language_version, typeof(TextAsset));
        string fs = txt.text;
        string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );
        
        for ( int i=0; i < fLines.Length; i++ ) {
            string line = fLines[i];

            string key = line.Split("!=!")[0].Trim();
            string value = line.Split("!=!")[1].Trim().Replace("<br>", "\n");

            if ( gameobjects_with_texts.Contains(key)) value += extra_text[key];

            if (key == "JoinCodeInput") {
                try {
                    GameObject lobby_code_input = GameObject.Find(key);

                    if (lobby_code_input.gameObject.GetComponent<TMP_InputField>().text.Length == 6) continue;

                    lobby_code_input.gameObject.GetComponent<TMP_InputField>().text = value;
                    continue;
                }
                catch (Exception) {
                    continue;
                }
            }

            if (buttons_hided && ( key == "HostButton" || key == "ClientButton")) {
                continue;
            }
    
            if ( gameobjects_with_texts.Contains(key)) {
                GameObject object_to_translate = GameObject.Find(key);
                
                try {
                    object_to_translate.GetComponent<TextMeshProUGUI>().text = value;
                }
                catch (Exception) {
                    try {
                        object_to_translate.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = value;
                    }
                    catch (Exception) {

                    }
                }
                
            }
        }
    }
}