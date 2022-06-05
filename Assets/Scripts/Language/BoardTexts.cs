using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System;

public static class BoardTexts {
    private static HashSet<string> gameobjects_with_texts = new HashSet<string> {
        "Tile_change_information",
        "End_turn_information",
        "Tile_change_error",
        "Board_connectivity_error",
        "Points",
        "Incorrect_word_error",
        "Board_one_line_error",
        "Blank_tile_information",
        "Actual_player_information",
        "End_game_information",
        "Restart_game_info",
        "Player_change_tiles_info"
    };

    private static Dictionary<string, string> extra_text = new Dictionary<string, string>() {
        {"Tile_change_information",""},
        {"End_turn_information",""},
        {"Tile_change_error",""},
        {"Board_connectivity_error",""},
        {"Points",""},
        {"Incorrect_word_error",""},
        {"Board_one_line_error",""},
        {"Blank_tile_information",""},
        {"Actual_player_information",""},
        {"End_game_information",""},
        {"Restart_game_info",""},
        {"Player_change_tiles_info",""}
    };

    private static string language_version = "EN";
    private static string languages_directory = "Languages/";

    public static void AddText(string key, string new_text) {
        extra_text[key] += new_text;
    }

    public static void ReplaceText(string key, string new_text) {
        extra_text[key] = new_text;
    }

    public static void Sync() {
        ChangeVersion(language_version);
    }

    public static void ChangeVersion(string version) {
        language_version = version;
        
        TextAsset txt = (TextAsset)Resources.Load(languages_directory + language_version, typeof(TextAsset));
        string fs = txt.text;
        string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );

        GameObject board = GameObject.Find("Board");
        
        for ( int i=0; i < fLines.Length; i++ ) {
            string line = fLines[i];

            string key = line.Split("!=!")[0].Trim();
            string value = line.Split("!=!")[1].Trim().Replace("<br>", "\n");

            if ( gameobjects_with_texts.Contains(key)) value += extra_text[key];

            if (key == "Incorrect_word_error") {
                board.GetComponent<Board>().incorrect_word_error_text = value;
                continue;
            }

            if (key == "Actual_player_information") {
                board.GetComponent<Board>().actual_player_information_text = value;
                continue;
            }

            if (key == "Player_change_tiles_info") {
                board.GetComponent<Board>().player_change_tiles_info_text = value;
                continue;
            }

            if (key == "Prev_player_error_word") {
                board.GetComponent<Board>().prev_player_error_word_text = value;
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