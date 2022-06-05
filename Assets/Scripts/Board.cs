using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using TMPro;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;

public class Board : NetworkBehaviour
{
    public GameObject tile;

    private char[,] scrabble_board;
    private char[,] checking_board_copy;
    private char[,] server_board_copy;
    private Dictionary<int, HashSet<string>> Scrabble_dictionary;
    private Dictionary<char, int> tiles_points;

    private (int, int, char)[] server_end_turn_tiles_buffor;
    private int server_end_turn_how_many_tiles;
    private int server_end_turn_tile_index = 0;

    private HashSet<(int, int, int, int)> placed_words;
    
    private HashSet<(int, int)> double_letter;
    private HashSet<(int, int)> triple_letter;
    private HashSet<(int, int)> double_word;
    private HashSet<(int, int)> triple_word;

    public string incorrect_word_error_text = "Ułożono nieprawidłowy wyraz:";
    public string actual_player_information_text = "Aktualny gracz:";
    public string player_change_tiles_info_text = "Gracz {0} wymienił <br> {1} literaków";
    public string prev_player_error_word_text = "Gracz {0} ułożył <br> nieprawidłowe słowo: <br> {1}";

    private GameObject board_connectivity_error;
    private GameObject incorrect_word_error;
    private GameObject board_one_line_error;
    private GameObject actual_player_information;
    private GameObject player_change_tiles_info;
    private GameObject prev_player_error_word;
    private GameObject tile_spawner;
    private GameObject[] points_amount_information;
    private GameObject end_turn_button;
    private GameObject end_game_information;
    private GameObject restart_game_button;
    private GameObject restart_game_info;
    private GameObject act_player_wood;

    private int[] points;

    private int players_number;
    private int[] players_points;
    private bool[] players_ready;
    private string[] players_nicks;
    private ulong[] players_ids;
    private int act_player;
    private int my_index_as_player;

    private int turns_without_move = 0;
    private bool is_all_connected = false;
    private bool is_all_players_synced = false;
    private bool is_game_started = false;

    public int GetActualPlayer() { return act_player; }
    public int GetMyIndexAsPlayer() { return my_index_as_player; }

    private void InitBoard() {
        scrabble_board = new char[15,15];
    }

    private HashSet<string> InitSingleHashSet(string file_path) {
        HashSet<string> result_hashset = new HashSet<string>();

        TextAsset txt = (TextAsset)Resources.Load(file_path, typeof(TextAsset));
        string fs = txt.text;
        string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );
        
        for ( int i=0; i < fLines.Length; i++ ) {
            string line = fLines[i];
    
            if (line != null) {  
                result_hashset.Add(line);    
            }  
        }

        return result_hashset;  
    }

    private void InitBonuses() {
        double_letter = new HashSet<(int, int)>();
        triple_letter = new HashSet<(int, int)>();
        double_word = new HashSet<(int, int)>();
        triple_word = new HashSet<(int, int)>();

        double_letter.Add((0, 3));
        double_letter.Add((0, 11));
        double_letter.Add((2, 6));
        double_letter.Add((2, 8));
        double_letter.Add((3, 0));
        double_letter.Add((3, 7));
        double_letter.Add((3, 14));
        double_letter.Add((6, 2));
        double_letter.Add((6, 6));
        double_letter.Add((6, 8));
        double_letter.Add((6, 12));
        double_letter.Add((7, 3));
        double_letter.Add((7, 11));
        double_letter.Add((8, 2));
        double_letter.Add((8, 6));
        double_letter.Add((8, 8));
        double_letter.Add((8, 12));
        double_letter.Add((11, 0));
        double_letter.Add((11, 7));
        double_letter.Add((11, 14));
        double_letter.Add((12, 6));
        double_letter.Add((12, 8));
        double_letter.Add((14, 3));
        double_letter.Add((14, 11));

        triple_letter.Add((1, 5));
        triple_letter.Add((1, 9));
        triple_letter.Add((5, 1));
        triple_letter.Add((5, 5));
        triple_letter.Add((5, 9));
        triple_letter.Add((5, 13));
        triple_letter.Add((9, 1));
        triple_letter.Add((9, 5));
        triple_letter.Add((9, 9));
        triple_letter.Add((9, 13));
        triple_letter.Add((13, 5));
        triple_letter.Add((13, 9));

        double_word.Add((1, 1));
        double_word.Add((2, 2));
        double_word.Add((3, 3));
        double_word.Add((4, 4));
        double_word.Add((1, 13));
        double_word.Add((2, 12));
        double_word.Add((3, 11));
        double_word.Add((4, 10));
        double_word.Add((10, 4));
        double_word.Add((11, 3));
        double_word.Add((12, 2));
        double_word.Add((13, 1));
        double_word.Add((10, 10));
        double_word.Add((11, 11));
        double_word.Add((12, 12));
        double_word.Add((13, 13));

        double_word.Add((7, 7));

        triple_word.Add((0, 0));
        triple_word.Add((0, 7));
        triple_word.Add((0, 14));
        triple_word.Add((7, 0));
        triple_word.Add((7, 14));
        triple_word.Add((14, 0));
        triple_word.Add((14, 7));
        triple_word.Add((14, 14));
    }

    private void InitDictionary() {
        string language_version = ChangeSceneClass.language_abbreviation;

        HashSet<string> hashset_2_letters = InitSingleHashSet("Dictionaries/" + language_version + "/2_letter");
        HashSet<string> hashset_3_letters = InitSingleHashSet("Dictionaries/" + language_version + "/3_letter");
        HashSet<string> hashset_4_letters = InitSingleHashSet("Dictionaries/" + language_version + "/4_letter");
        HashSet<string> hashset_5_letters = InitSingleHashSet("Dictionaries/" + language_version + "/5_letter");
        HashSet<string> hashset_6_letters = InitSingleHashSet("Dictionaries/" + language_version + "/6_letter");
        HashSet<string> hashset_7_letters = InitSingleHashSet("Dictionaries/" + language_version + "/7_letter");
        HashSet<string> hashset_8_letters = InitSingleHashSet("Dictionaries/" + language_version + "/8_letter");
        HashSet<string> hashset_9_letters = InitSingleHashSet("Dictionaries/" + language_version + "/9_letter");
        
        HashSet<string> hashset_10_letters = InitSingleHashSet("Dictionaries/" + language_version + "/10_letter");
        HashSet<string> hashset_11_letters = InitSingleHashSet("Dictionaries/" + language_version + "/11_letter");
        HashSet<string> hashset_12_letters = InitSingleHashSet("Dictionaries/" + language_version + "/12_letter");
        HashSet<string> hashset_13_letters = InitSingleHashSet("Dictionaries/" + language_version + "/13_letter");
        HashSet<string> hashset_14_letters = InitSingleHashSet("Dictionaries/" + language_version + "/14_letter");
        HashSet<string> hashset_15_letters = InitSingleHashSet("Dictionaries/" + language_version + "/15_letter");
        
        Scrabble_dictionary.Add(2, hashset_2_letters);
        Scrabble_dictionary.Add(3, hashset_3_letters);
        Scrabble_dictionary.Add(4, hashset_4_letters);
        Scrabble_dictionary.Add(5, hashset_5_letters);
        Scrabble_dictionary.Add(6, hashset_6_letters);
        Scrabble_dictionary.Add(7, hashset_7_letters);
        Scrabble_dictionary.Add(8, hashset_8_letters);
        Scrabble_dictionary.Add(9, hashset_9_letters);
        Scrabble_dictionary.Add(10, hashset_10_letters);
        Scrabble_dictionary.Add(11, hashset_11_letters);
        Scrabble_dictionary.Add(12, hashset_12_letters);
        Scrabble_dictionary.Add(13, hashset_13_letters);
        Scrabble_dictionary.Add(14, hashset_14_letters);
        Scrabble_dictionary.Add(15, hashset_15_letters);
    }

    private void InitTilesPoints() {
        tiles_points = new Dictionary<char, int>();

        TextAsset txt = (TextAsset)Resources.Load("Tiles_amount/" + ChangeSceneClass.language_abbreviation + "/tiles", typeof(TextAsset));
        string fs = txt.text;
        string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );
        
        for ( int i=0; i < fLines.Length; i++ ) {
            string line = fLines[i].Trim();;

            if (line != null) {  
                string[] elements = line.Split();
                if (elements.Length < 3) { continue; }
                char letter_to_add = elements[0].ToCharArray()[0];
                int how_many_letter = Int32.Parse(elements[1]);
                int how_many_points = Int32.Parse(elements[2]);

                tiles_points.Add(letter_to_add, how_many_points);
            }  
        }
    }

    private void InitializeGameObjects() {
        board_connectivity_error = GameObject.Find("Board_connectivity_error");
        board_connectivity_error.GetComponent<Fading_text>().MakeTextInvisible();

        incorrect_word_error = GameObject.Find("Incorrect_word_error");
        incorrect_word_error.GetComponent<Fading_text>().MakeTextInvisible();

        board_one_line_error = GameObject.Find("Board_one_line_error");
        board_one_line_error.GetComponent<Fading_text>().MakeTextInvisible();

        points_amount_information = new GameObject[4];
        points_amount_information[0] = GameObject.Find("Points_amount_0");
        points_amount_information[1] = GameObject.Find("Points_amount_1");
        points_amount_information[2] = GameObject.Find("Points_amount_2");
        points_amount_information[3] = GameObject.Find("Points_amount_3");

        actual_player_information = GameObject.Find("Actual_player_information");
        tile_spawner = GameObject.Find("Tile_spawner");

        end_turn_button = GameObject.Find("End_turn");
        GameObject.Find("End_turn_information").GetComponent<TextMeshProUGUI>().text = ChangeSceneClass.language_abbreviation;

        end_game_information = GameObject.Find("End_game_information");
        end_game_information.GetComponent<Fading_text>().MakeTextInvisible();

        restart_game_button = GameObject.Find("Restart_game");
        restart_game_button.SetActive(false);

        restart_game_info = GameObject.Find("Restart_game_info");
        restart_game_info.GetComponent<Fading_text>().MakeTextInvisible();

        player_change_tiles_info = GameObject.Find("Player_change_tiles_info");
        player_change_tiles_info.GetComponent<Fading_text>().MakeTextInvisible();

        prev_player_error_word = GameObject.Find("Prev_player_error_word");
        prev_player_error_word.GetComponent<Fading_text>().MakeTextInvisible();

        act_player_wood = GameObject.Find("Actual_player_wood");
    }

    void SetMyIndexAsPlayer() {
        ulong my_client_id = NetworkManager.Singleton.LocalClientId;
        my_index_as_player = 0;

        for (int i = 0; i < players_number; ++i) {
            if (my_client_id == players_ids[i]) {
                my_index_as_player = i;
                return;
            }
        }
    }

    private void InitializePlayers(int players_number, string[] players_nicks) {
        players_ready = new bool[players_number];

        this.players_number = players_number;
        players_points = new int[players_number];
        this.players_nicks = players_nicks;
        
        for (int i = 0; i < players_number; ++i) {
            points_amount_information[i].GetComponent<TextMeshProUGUI>().text = players_nicks[i] + ": " + points[i].ToString();
            players_ready[i] = false;
        }

        for (int i = players_number; i < 4; ++i) {
            points_amount_information[i].GetComponent<TextMeshProUGUI>().text = "";
        }

        act_player = -1;

        SetMyIndexAsPlayer();
        tile_spawner.GetComponent<Tile_spawner>().DrawTilesServerRpc(my_index_as_player, 7, NetworkManager.Singleton.LocalClientId);
    }

    private void InitServerBuffors() {
        server_end_turn_tiles_buffor = new (int, int, char)[7];
        server_end_turn_how_many_tiles = 0;
    }

    private void InitializeLanguage() {
        GameObject.Find("Loading_players").GetComponent<Image>().sprite = Resources.Load<Sprite>("Other_graphics/" + ChangeSceneClass.language_abbreviation + "/loading");

        BoardTexts.ChangeVersion(ChangeSceneClass.language_abbreviation); 
    }

    void Awake() {
        Scrabble_dictionary = new Dictionary<int, HashSet<string>>();
        placed_words = new HashSet<(int, int, int, int)>();
        points = new int[4];
        
        InitDictionary();
        InitBoard();
        InitBonuses();
        InitTilesPoints();

        InitializeGameObjects();
        InitializeLanguage();

        if (NetworkManager.Singleton.IsServer) {
            int clients_number = ChangeSceneClass.clients_number;
            ulong[] clients_ids = ChangeSceneClass.clients_ids;
            string[] clients_nicks = ChangeSceneClass.clients_nicks;

            players_ids = clients_ids;
            this.players_nicks = clients_nicks;
            this.players_number = clients_number;

            InitServerBuffors();
        }
    }

    [ClientRpc]
    void AddNicksClientRpc(int players_number, string nick1, string nick2, string nick3, string nick4, ulong ci1, ulong ci2, ulong ci3, ulong ci4) {
        if (!NetworkManager.Singleton.IsServer) {
            string[] players_nicks = new string[]{nick1, nick2, nick3, nick4};
            this.players_number = players_number;
            players_points = new int[players_number];
            this.players_nicks = players_nicks;
            players_ids = new ulong[]{ci1, ci2, ci3, ci4};
            
            for (int i = 0; i < players_number; ++i) {
                points_amount_information[i].GetComponent<TextMeshProUGUI>().text = players_nicks[i] + ": " + points[i].ToString();
            }

            for (int i = players_number; i < 4; ++i) {
                points_amount_information[i].GetComponent<TextMeshProUGUI>().text = "";
            }

            SetMyIndexAsPlayer();

            tile_spawner.GetComponent<Tile_spawner>().DrawTilesServerRpc(my_index_as_player, 7, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void ChangeActualPlayerServerRpc(int how_many_tiles = 0, string incorrect_word = "") {
        if (act_player == -1) {
            StartGameClientRpc();
        }

        act_player += 1;
        act_player %= players_number;

        SyncChangeActualPlayerClientRpc(act_player, how_many_tiles, incorrect_word);
    }

    [ClientRpc]
    public void StartGameClientRpc() {
        Destroy(GameObject.Find("Loading_players"));
    }

    [ClientRpc]
    public void SyncChangeActualPlayerClientRpc(int new_act_player, int how_many_tiles, string incorrect_word) {
        act_player = new_act_player;

        int prev_player = act_player - 1;
        if (prev_player < 0) prev_player = players_number - 1;

        if (how_many_tiles > 0) {
            player_change_tiles_info.GetComponent<TextMeshProUGUI>().text = 
                String.Format(player_change_tiles_info_text, players_nicks[prev_player], how_many_tiles);

            player_change_tiles_info.GetComponent<Fading_text>().FadeText();
        }

        if (incorrect_word != "" && prev_player != my_index_as_player) {
            prev_player_error_word.GetComponent<TextMeshProUGUI>().text = 
                String.Format(prev_player_error_word_text, players_nicks[prev_player], incorrect_word.ToLower());

            prev_player_error_word.GetComponent<Fading_text>().FadeText();
        }

        actual_player_information.GetComponent<TextMeshProUGUI>().text = actual_player_information_text + "\n" + players_nicks[act_player];

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            tile_script.NotMyTurn();

            if (tile_script.GetPlayerID() == act_player) {
                tile_script.MyTurn();
            }
        }

        if (act_player != my_index_as_player) {
            act_player_wood.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            end_turn_button.GetComponent<End_turn>().SetButtonInactive();
        }
        else {
            act_player_wood.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 1);
            end_turn_button.GetComponent<End_turn>().SetButtonActive();
        }
    }

    private bool IsWordValid(string word) {
        int key = word.Length;

        return Scrabble_dictionary[key].Contains(word.ToLower());
    } 

    private List<(string, (int, int), (int, int))> SplitWordsInLine(List<char> line, int index, bool horizontal) {
        List<(string, (int, int), (int, int))> words_in_line = new List<(string, (int, int), (int, int))>();

        string current_word = "";
        for (int i = 0; i < line.Count; ++i) {
            if (line[i] == '\0') {
                if (current_word.Length > 1) {
                    if (horizontal) {
                        words_in_line.Add((current_word, (i - current_word.Length, index), (i - 1, index)));
                    }
                    else {
                        words_in_line.Add((current_word, (index, i - current_word.Length), (index, i - 1)));
                    }
                }

                current_word = "";
            }
            else {
                current_word += line[i];
            }
        }

        if (current_word.Length > 1) {
            if (horizontal) {
                words_in_line.Add((current_word, (line.Count - current_word.Length - 1, index), (line.Count - 1, index)));
            }
            else {
                words_in_line.Add((current_word, (index, line.Count - current_word.Length - 1), (index, line.Count - 1)));
            }
        }

        return words_in_line; 
    }

    private List<(string, (int, int), (int, int))> GetAllWordsFromBoard() {
        List<(string, (int, int), (int, int))> all_words = new List<(string, (int, int), (int, int))>();

        int rowLength = scrabble_board.GetLength(0);
        int colLength = scrabble_board.GetLength(1);

        for (int i = 0; i < rowLength; i++) {
            List<char> actual_row = SliceRow(scrabble_board, i).ToList();

            all_words.AddRange(SplitWordsInLine(actual_row, i, true));
        }

        for (int i = 0; i < colLength; i++) {
            List<char> actual_col = SliceColumn(scrabble_board, i).ToList();

            all_words.AddRange(SplitWordsInLine(actual_col, i, false));
        }

        return all_words;
    }

    private IEnumerable<T> SliceRow<T>(T[,] array, int row)    {
        for (var i = array.GetLowerBound(1); i <= array.GetUpperBound(1); i++)  {
            yield return array[row, i];
        }
    }

    private IEnumerable<T> SliceColumn<T>(T[,] array, int column)  {
        for (var i = array.GetLowerBound(0); i <= array.GetUpperBound(0); i++)  {
            yield return array[i, column];
        }
    }

    public (bool, string) CheckBoardWordsCorrectness() {
        foreach ((string, (int, int), (int, int)) word in GetAllWordsFromBoard()) {
            if (!IsWordValid(word.Item1)) {
                return (false, word.Item1);
            }
        }

        return (true, "");
    }

    public bool CheckBoardConnectivity() {
        char[,] board_copy = scrabble_board.Clone() as char[,];
        if (board_copy[7,7] == '\0') {
            return false;
        }

        Queue<(int, int)> chars_to_wipeout = new Queue<(int, int)>();

        int rowLength = board_copy.GetLength(0);
        int colLength = board_copy.GetLength(1);

        for (int i = 0; i < rowLength; i++) {
            for (int j = 0; j < colLength; j++) {
                if (board_copy[i, j] != '\0') {
                    chars_to_wipeout.Enqueue((i, j));
                    i = rowLength + 1;
                    break;
                }                
            }
        }

        int how_many_tiles = 0;

        while (chars_to_wipeout.Count != 0) {
            (int, int) actual_char = chars_to_wipeout.Dequeue();
            how_many_tiles++;
            
            int x = actual_char.Item1;
            int y = actual_char.Item2;

            board_copy[x, y] = '\0';

            if (x-1 >= 0 && board_copy[x-1, y] != '\0') chars_to_wipeout.Enqueue((x-1, y));
            if (x+1 < rowLength && board_copy[x+1, y] != '\0') chars_to_wipeout.Enqueue((x+1, y));
            if (y-1 >= 0 && board_copy[x, y-1] != '\0') chars_to_wipeout.Enqueue((x, y-1));
            if (y+1 < colLength && board_copy[x, y+1] != '\0') chars_to_wipeout.Enqueue((x, y+1));
        }

        for (int i = 0; i < rowLength; i++) {
            for (int j = 0; j < colLength; j++) {
                if (board_copy[i, j] != '\0') {
                    return false;
                }                
            }
        }

        if (how_many_tiles == 1) return false;

        return true;
    }

    public bool CheckBoardOneLine() {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        List<Vector2> new_tiles = new List<Vector2>();

        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            
            if (tile_script.new_setted_on_board) {
                new_tiles.Add(tile_script.GetBoardFieldCoord());
            }
        }

        if (new_tiles.Count == 0) { return true; }

        int x_start = (int)new_tiles.First().x;
        int y_start = (int)new_tiles.First().y;

        bool x_changed = false;
        bool y_changed = false;

        foreach (Vector2 tile_coords in new_tiles) {
            if (tile_coords.x != x_start) { x_changed = true; }
            if (tile_coords.y != y_start) { y_changed = true; }
        }

        return !x_changed || !y_changed;
    }

    /* TestAll() error codes:

        1 -> CheckBoardConnectivity
        2 -> CheckBoardWordsCorrectness
        3 -> CheckBoardOneLine
    */
    public (bool, int, string) TestAll() {
        checking_board_copy = scrabble_board.Clone() as char[,];

        if (!CheckBoardConnectivity()) {
            scrabble_board = checking_board_copy.Clone() as char[,];
            return (false, 1, "");
        }

        (bool, string) words_correctness = CheckBoardWordsCorrectness();
        if (!words_correctness.Item1) {
            scrabble_board = checking_board_copy.Clone() as char[,];
            return (false, 2, words_correctness.Item2);
        }

        if (!CheckBoardOneLine()) {
            scrabble_board = checking_board_copy.Clone() as char[,];
            return (false, 3, "");
        }

        return (true, 0, "");
    }

    private int GetPointsFromOneChar(char c) {
        if (char.IsUpper(c)) {
            return 2;
        }
        
        return tiles_points[c];
    }

    public void GetPoints(bool seven_tiles) {
        List<(string, (int, int), (int, int))> all_words = GetAllWordsFromBoard();
        List<(string, (int, int), (int, int))> new_words = new List<(string, (int, int), (int, int))>();

        foreach ((string, (int, int), (int, int)) word in all_words) {
            if (!placed_words.Contains((word.Item2.Item1, word.Item2.Item2, word.Item3.Item1, word.Item3.Item2))) {
                new_words.Add(word);
            }
        }

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        List<Vector2> new_tiles = new List<Vector2>();

        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            
            if (tile_script.new_setted_on_board) {
                new_tiles.Add(tile_script.GetBoardFieldCoord());
            }
        }

        if (seven_tiles) points[act_player] += 50;

        List<(int, int, int)> bonuses_to_remove = new List<(int, int, int)>(); // Item1 points to which bonus. 1 -> double letter, 2 -> triple letter, 3 -> double word, 4 -> triple word

        foreach ((string, (int, int), (int, int)) word_tuple in new_words) {
            int tmp_points = 0;

            string word = word_tuple.Item1;            
            (int, int) start = word_tuple.Item2;
            (int, int) end = word_tuple.Item3;

            foreach (char c in word) {
                tmp_points += GetPointsFromOneChar(c);
            }

            if (start.Item1 == end.Item1) { // horizontal
                int x = start.Item1;
                int y = start.Item2;
                int index = 0;

                while (y <= end.Item2) {
                    if (double_letter.Contains((x, y))) {
                        tmp_points += GetPointsFromOneChar(word[index]);
                        bonuses_to_remove.Add((1, x, y));
                    }

                    if (triple_letter.Contains((x, y))) {
                        tmp_points += GetPointsFromOneChar(word[index]) * 2;
                        bonuses_to_remove.Add((2, x, y));
                    }

                    ++index;
                    ++y;
                }

                x = start.Item1;
                y = start.Item2;
                index = 0;

                while (y <= end.Item2) {
                    if (double_word.Contains((x, y))) {
                        tmp_points *= 2;
                        bonuses_to_remove.Add((3, x, y));
                    }

                    if (triple_word.Contains((x, y))) {
                        tmp_points *= 3;
                        bonuses_to_remove.Add((4, x, y));
                    }

                    ++index;
                    ++y;
                }
            }

            if (start.Item2 == end.Item2) { // vertical
                int x = start.Item1;
                int y = start.Item2;
                int index = 0;

                while (x <= end.Item1) {
                    if (double_letter.Contains((x, y))) {
                        tmp_points += GetPointsFromOneChar(word[index]);
                        bonuses_to_remove.Add((1, x, y));
                    }

                    if (triple_letter.Contains((x, y))) {
                        tmp_points += GetPointsFromOneChar(word[index]) * 2;
                        bonuses_to_remove.Add((2, x, y));
                    }

                    ++index;
                    ++x;
                }

                x = start.Item1;
                y = start.Item2;
                index = 0;

                while (x <= end.Item1) {
                    if (double_word.Contains((x, y))) {
                        tmp_points *= 2;
                        bonuses_to_remove.Add((3, x, y));
                    }

                    if (triple_word.Contains((x, y))) {
                        tmp_points *= 3;
                        bonuses_to_remove.Add((4, x, y));
                    }

                    ++index;
                    ++x;
                }
            }

            points[act_player] += tmp_points;
        }

        foreach ((int, int, int) bonus in bonuses_to_remove) {
            switch(bonus.Item1) {
                case 1:
                    double_letter.Remove((bonus.Item2, bonus.Item3));
                    break;
                case 2:
                    triple_letter.Remove((bonus.Item2, bonus.Item3));
                    break;
                case 3:
                    double_word.Remove((bonus.Item2, bonus.Item3));
                    break;
                case 4:
                    triple_word.Remove((bonus.Item2, bonus.Item3));
                    break;
                default:
                    break;
                }
        }

        foreach ((string, (int, int), (int, int)) word_tuple in new_words) {
            placed_words.Add((word_tuple.Item2.Item1, word_tuple.Item2.Item2, word_tuple.Item3.Item1, word_tuple.Item3.Item2));
        }
    }

    private void EndGame() {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            tile_script.NotMyTurn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckEndGameServerRpc(int points0, int points1, int points2, int points3) {

        if (turns_without_move >= players_number * 2) {
            int[] points_loss = new int[]{points0, points1, points2, points3};
            
            for (int i = 0; i < players_number; ++i) {
                int new_points = points[i] - points_loss[i];
                points[i] = new_points;
                SyncPointsClientRpc(i, new_points);
            }

            restart_game_button.SetActive(true);
            restart_game_info.GetComponent<Fading_text>().MakeTextVisible();;
            EndGameClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc() {
        restart_game_button.SetActive(true);
        restart_game_info.GetComponent<Fading_text>().MakeTextVisible();
        EndGameClientRpc();
    }

    [ClientRpc]
    public void EndGameClientRpc() {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            tile_script.GameEnded();
        }

        end_game_information.GetComponent<Fading_text>().MakeTextVisible();
        end_turn_button.GetComponent<End_turn>().game_ended = true; 
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGamePointsCalculateServerRpc(int act_player) {
        for (int i = 0; i < players_number; ++i) {
            if (i == act_player) continue;

            ulong clientId = players_ids[i];

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };

            EndGameSubstractPointsClientRpc(act_player, clientRpcParams);
        }
    }

    [ClientRpc]
    public void EndGameSubstractPointsClientRpc(int first_player, ClientRpcParams clientRpcParams = default) {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        int players_points_in_hand = 0;

        foreach(GameObject tile in tiles) {
            if (!tile.GetComponent<Tile>().new_setted_on_board && !tile.GetComponent<Tile>().permament_setted_on_board && tile.GetComponent<Tile>().GetPlayerID() == my_index_as_player) {
                players_points_in_hand += tile.GetComponent<Tile>().GetValue();
            }
        }

        AddAndSubstractEndGamePointsServerRpc(my_index_as_player, first_player, players_points_in_hand);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddAndSubstractEndGamePointsServerRpc(int substract_from, int add_to, int points_to_add) {
        points[add_to] += points_to_add;
        points[substract_from] -= points_to_add;

        SyncPointsServerRpc(add_to, points[add_to]);
        SyncPointsServerRpc(substract_from, points[substract_from]);      
    }

    public bool CheckEndGame() {
        bool spawner_empty = tile_spawner.GetComponent<Tile_spawner>().SpawnerEmpty();
        int tiles_on_hand = 0;
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach(GameObject tile in tiles) {
            if (!tile.GetComponent<Tile>().new_setted_on_board && !tile.GetComponent<Tile>().permament_setted_on_board && tile.GetComponent<Tile>().GetPlayerID() == act_player) {
                tiles_on_hand++;
            }
        }

        if (!spawner_empty) { return false; }

        if (tiles_on_hand == 0) {
            EndGamePointsCalculateServerRpc(act_player);
            EndGameServerRpc();
            return true;
        }

        int[] possible_points_loss = new int[4];

        for (int i = 0; i < players_number; ++i) {
                
            int players_points_in_hand = 0;

            foreach(GameObject tile in tiles) {
                if (!tile.GetComponent<Tile>().new_setted_on_board && !tile.GetComponent<Tile>().permament_setted_on_board && tile.GetComponent<Tile>().GetPlayerID() == i) {
                    players_points_in_hand += tile.GetComponent<Tile>().GetValue();
                }
            }

            possible_points_loss[i] = players_points_in_hand;  
        }

        CheckEndGameServerRpc(possible_points_loss[0], possible_points_loss[1], possible_points_loss[2], possible_points_loss[3]);

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(int index_as_player) {
        players_ready[index_as_player] = true;

        foreach (bool ready in players_ready) {
            if (!ready) return;
        }

        is_all_players_synced = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnTilePermanentOnBoardServerRpc(int index, char letter, bool is_blank) {
        SpawnTilePermanentOnBoardClientRpc(index, letter, is_blank);
    }

    [ClientRpc]
    public void SpawnTilePermanentOnBoardClientRpc(int index, char letter, bool is_blank) {
        Vector2 tile_coords = tile_spawner.GetComponent<Tile_spawner>().GetBoardFieldCoords(index);
        GameObject new_tile = Instantiate(tile, tile_coords, Quaternion.identity) as GameObject;
        
        Tile created_tile = new_tile.GetComponent<Tile>();
        created_tile.SetParameters(letter, 0, act_player);
        created_tile.SpawnOnClient(is_blank);

        tile_spawner.GetComponent<Tile_spawner>().SetTilePermamentOnBoardServerRpc(index);
    }   

    [ServerRpc(RequireOwnership = false)]
    public void SyncPointsServerRpc(int player_index, int new_points) {
        points[player_index] = new_points;
        SyncPointsClientRpc(player_index, new_points);
    }

    [ClientRpc]
    public void SyncPointsClientRpc(int player_index, int new_points) {
        points[player_index] = new_points;

        points_amount_information[player_index].GetComponent<TextMeshProUGUI>().text = players_nicks[player_index] + ": " + new_points.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void HowManyLettersUsedServerRpc(int how_many_tiles) {
        server_end_turn_how_many_tiles = how_many_tiles;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddLetterToBufforServerRpc(int x, int y, char letter) {
        server_end_turn_tiles_buffor[server_end_turn_tile_index] = (x, y, letter);
        server_end_turn_tile_index++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnResetServerRpc() {
        server_end_turn_tiles_buffor = new (int, int, char)[7];
        server_end_turn_how_many_tiles = 0;
        server_end_turn_tile_index = 0;
    }

    [ClientRpc]
    public void FailedTestClientRpc(int error_code, string error_word, ClientRpcParams clientRpcParams = default) {
        switch(error_code) {
            case 1:
                board_connectivity_error.GetComponent<Fading_text>().FadeText();
                break;
            case 2:
                incorrect_word_error.GetComponent<TextMeshProUGUI>().text = incorrect_word_error_text + '\n' + error_word.ToLower();
                incorrect_word_error.GetComponent<Fading_text>().FadeText();
                break;
            case 3:
                board_one_line_error.GetComponent<Fading_text>().FadeText();
                break;
            default:
                break;
            }

        GameObject[] tiles_to_back = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles_to_back) {
            Tile tile_script = tile.GetComponent<Tile>();
            
            if (tile_script.new_setted_on_board) {
                tile_script.BackToHand();
            }
        }

        bool change_player = false;

        if (error_code == 2) {
            change_player = true;
        }

        GameObject end_turn_button_sync = GameObject.Find("End_turn");
        end_turn_button_sync.GetComponent<End_turn>().SetEndTurnSynVars(false, 0, 0, change_player, error_word);
    }

    [ClientRpc]
    public void PassedTestClientRpc(int act_player, ClientRpcParams clientRpcParams = default) {        
        int how_many_tiles_to_draw = 0;

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in tiles) {
            Tile tile_script = tile.GetComponent<Tile>();
            
            if (tile_script.new_setted_on_board) {

                Vector2 coords = tile_script.GetBoardFieldCoord();

                scrabble_board[(int)coords[0], (int)coords[1]] = tile_script.GetLetter();

                tile_script.PlaceTilePermamentOnBoard();
                SpawnTilePermanentOnBoardServerRpc(tile_script.GetCoordsListIndex(), tile_script.GetLetter(), tile_script.IsBlank());

                ++how_many_tiles_to_draw;
            }
        }  

        GameObject end_turn_button_sync = GameObject.Find("End_turn");
        end_turn_button_sync.GetComponent<End_turn>().SetEndTurnSynVars(true, act_player, how_many_tiles_to_draw, true, "");
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ulong clientId) {
        turns_without_move += 1;
        server_board_copy = scrabble_board.Clone() as char[,];

        for (int i = 0; i < server_end_turn_how_many_tiles; ++i) {
            scrabble_board[server_end_turn_tiles_buffor[i].Item1, server_end_turn_tiles_buffor[i].Item2] = server_end_turn_tiles_buffor[i].Item3;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        (bool, int, string) test_result = TestAll();

        if (!test_result.Item1) {
            FailedTestClientRpc(test_result.Item2, test_result.Item3, clientRpcParams);

            scrabble_board = server_board_copy.Clone() as char[,];
            return;
        }       

        GetPoints(server_end_turn_how_many_tiles == 7);
        SyncPointsServerRpc(act_player, points[act_player]);

        bool move_done = server_end_turn_how_many_tiles != 0;

        if (move_done) {
            turns_without_move = 0;
        }

        PassedTestClientRpc(act_player, clientRpcParams);
    }

    void Update() {
        if (!is_all_connected) {
            if(NetworkManager.Singleton.IsServer) {
                if(players_number == NetworkManager.Singleton.ConnectedClientsIds.Count) {
                    is_all_connected = true;
                    InitializePlayers(players_number, players_nicks);
                    AddNicksClientRpc(players_number, players_nicks[0], players_nicks[1], players_nicks[2], players_nicks[3], players_ids[0], players_ids[1], players_ids[2], players_ids[3]);
                }
            }
        }

        if (is_all_players_synced && !is_game_started) {
            if(NetworkManager.Singleton.IsServer) {
                ChangeActualPlayerServerRpc();
                ChangeSceneClass.is_board_sync = true;
                is_all_players_synced = false;
                is_game_started = true;
            }
        }
    }
}