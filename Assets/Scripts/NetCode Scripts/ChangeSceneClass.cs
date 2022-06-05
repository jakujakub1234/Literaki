using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using UnityEngine.UI;

public static class ChangeSceneClass {
    public static int clients_number = 0;
    public static ulong[] clients_ids = new ulong[4];
    public static string[] clients_nicks = new string[4];

    public static bool is_board_sync = false;

    public static int language_id = 0; 
    public static string language_abbreviation = "EN";

    public static bool host_started_game = false;

    public static void Shuffle() {
        Random rnd = new Random();
        for (int i = 0; i < clients_number - 1; i++) {
            int j = rnd.Next(i, clients_number);

            ulong tmp_id = clients_ids[j];
            clients_ids[j] = clients_ids[i];
            clients_ids[i] = tmp_id;

            string tmp_nick = clients_nicks[j];
            clients_nicks[j] = clients_nicks[i];
            clients_nicks[i] = tmp_nick; 
        }
    }

    public static void DropdownLanguageChange(int new_language_id, bool host_change=false) {
        if (host_started_game) return;

        language_id = new_language_id;
        
        switch (language_id) {
            case 0:
                language_abbreviation = "EN";
                break;
            case 1:
                language_abbreviation = "PL";
                break;

            default:
                break;
        }

        if (host_change) host_started_game = true;

        LobbyTexts.ChangeVersion(language_abbreviation);
    }
}