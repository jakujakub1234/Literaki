using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;
using System.Text.RegularExpressions;

using Random = System.Random;

public class Tile_spawner : NetworkBehaviour {
    public GameObject tile;
    private GameObject tile_spawner_amount;
    private GameObject tile_change_information;
    private GameObject tile_change_error;
    private GameObject board;

    private List<(Vector2, bool)>[] tiles_trail_fields;
    private List<(Vector2, Vector2, bool)> board_fields;

    private List<(char, int)> tiles_in_bag;
    Random rand;

    public bool tiles_changing_action;

    public Vector2 GetBoardFieldCoords(int index) {
        return board_fields[index].Item1;
    }

    public bool SpawnerEmpty() { return tiles_in_bag.Count == 0; }

    private void InitiateTilesInBag() {
        tiles_in_bag = new List<(char, int)>();

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

                for (int j = 0; j < how_many_letter; j++) tiles_in_bag.Add((letter_to_add, how_many_points));
            }  
        }
    }

    private void InitializeTrailFieldsLists() {
        tiles_trail_fields = new List<(Vector2, bool)>[4];

        GameObject tiles_trail = GameObject.Find("Tiles_trail");

        double trail_width = tiles_trail.GetComponent<Renderer>().bounds.size.x;
        double tile_width = trail_width / 9.0;
    
        Vector3 first_tile_on_trail = tiles_trail.transform.position;

        for (int player_id = 0; player_id < 4; ++player_id) {
            tiles_trail_fields[player_id] = new List<(Vector2, bool)>();
            first_tile_on_trail = tiles_trail.transform.position;
            first_tile_on_trail.x -= (float)(4.0 * tile_width);
            for (int i = 0; i < 9; ++i) {
                tiles_trail_fields[player_id].Add((first_tile_on_trail, false));
                first_tile_on_trail.x += (float)tile_width;
            }
        }
    }

    private void InitializeBoardFieldsLists() {
        board_fields = new List<(Vector2, Vector2, bool)>();

        GameObject board = GameObject.Find("Board");

        double board_width = board.GetComponent<Renderer>().bounds.size.x;
        double tile_width = board_width / 15.0;

        double board_height = board.GetComponent<Renderer>().bounds.size.y;
        double tile_height = board_height / 15.0;
    
        Vector3 first_tile_on_board = board.transform.position;
        first_tile_on_board.x -= (float)(7.0 * tile_width);
        first_tile_on_board.y += (float)(7.0 * tile_height);

        float start_x = first_tile_on_board.x;

        for (int i = 0; i < 15; ++i) {
            for (int j = 0; j < 15; ++j) {
                board_fields.Add((first_tile_on_board, new Vector2(i, j), false));
                first_tile_on_board.x += (float)tile_width;
            }
            first_tile_on_board.x = start_x;
            first_tile_on_board.y -= (float)tile_height;
        }
    }

    private void InitializeGameObjects() {
        tile_spawner_amount = GameObject.Find("Tile_spawner_amount");
        tile_change_information = GameObject.Find("Tile_change_information");
        tile_change_information.GetComponent<Fading_text>().MakeTextInvisible();

        tile_change_error = GameObject.Find("Tile_change_error");
        tile_change_error.GetComponent<Fading_text>().MakeTextInvisible();

        board = GameObject.Find("Board");
    }

    void Awake() {
        InitiateTilesInBag();
        InitializeTrailFieldsLists();
        InitializeBoardFieldsLists();
        InitializeGameObjects();

        rand = new Random();
        tiles_changing_action = false;
    }

    private (char, int) GetRandomTile() {
        int index = rand.Next(tiles_in_bag.Count - 1);
        (char, int) value = tiles_in_bag[index];
        tiles_in_bag.RemoveAt(index);

        return value;
    } 

    public GameObject SpawnTile(Vector3 postion, int index, int act_player, (char, int) new_tile_parameters) {
        if (tiles_in_bag.Count == 0) {
            return null;
        }

        GameObject new_tile = Instantiate(tile, postion, Quaternion.identity) as GameObject;
        
        Tile created_tile = new_tile.GetComponent<Tile>();

        created_tile.SetParameters(new_tile_parameters.Item1, new_tile_parameters.Item2, act_player);
        created_tile.SetCoordsListIndex(index);

        tiles_trail_fields[act_player][index] = (tiles_trail_fields[act_player][index].Item1, true);

        return new_tile;
    }

    public (Vector2, bool, Vector2, int) GetClosestAttractField(Vector2 actual_pos) {
        int act_player = board.GetComponent<Board>().GetActualPlayer();
        int my_index_as_player = board.GetComponent<Board>().GetMyIndexAsPlayer();
        Vector2 result = tiles_trail_fields[my_index_as_player][0].Item1;
        int best_index = 0;

        foreach ((Vector2, bool) field_tuple in tiles_trail_fields[my_index_as_player]) {
            if (!field_tuple.Item2) {
                result = field_tuple.Item1;
                break;
            }
            ++best_index;
        } 

        Vector2 result_board_coord = new Vector2(0,0);

        bool is_board_field = false;
        float threshold = 0.8f;

        int index = 0;

        if (act_player == my_index_as_player) {
            foreach ((Vector2, Vector2, bool) field_tuple in board_fields) {
                Vector2 field = field_tuple.Item1;
                float tmp_dist = Vector2.Distance(field, actual_pos);
                if (tmp_dist < threshold && !field_tuple.Item3) {
                    threshold = tmp_dist;
                    result = field;
                    is_board_field = true;
                    result_board_coord = field_tuple.Item2;
                    best_index = index;
                }

                index++;
            }
        }

        index = 0;
        foreach ((Vector2, bool) field_tuple in tiles_trail_fields[my_index_as_player]) {
            Vector2 field = field_tuple.Item1;
            float tmp_dist = Vector2.Distance(field, actual_pos);
            if (tmp_dist < threshold && !field_tuple.Item2) {
                threshold = tmp_dist;
                is_board_field = false;
                result = field;
                best_index = index;
            }

            index++;
        } 

        if (best_index != -1) {
            if (is_board_field) { board_fields[best_index] = (board_fields[best_index].Item1, board_fields[best_index].Item2, true); }
            else { tiles_trail_fields[my_index_as_player][best_index] = (tiles_trail_fields[my_index_as_player][best_index].Item1, true); }
        }
        return (result, is_board_field, result_board_coord, best_index);
    }

    public (Vector2, Vector2, int) BackTileToHand() {
        int act_player = board.GetComponent<Board>().GetActualPlayer();
        Vector2 result = tiles_trail_fields[act_player][0].Item1;
        int best_index = 0;

        foreach ((Vector2, bool) field_tuple in tiles_trail_fields[act_player]) {
            if (!field_tuple.Item2) {
                result = field_tuple.Item1;
                break;
            }
            ++best_index;
        } 

        Vector2 result_board_coord = new Vector2(0,0);

        tiles_trail_fields[act_player][best_index] = (tiles_trail_fields[act_player][best_index].Item1, true); 

        return (result, result_board_coord, best_index);
    }

    public void FreeCoords(bool is_board_field, int best_index) {
        int act_player = board.GetComponent<Board>().GetMyIndexAsPlayer();
        if (best_index != -1) {
            if (is_board_field) { board_fields[best_index] = (board_fields[best_index].Item1, board_fields[best_index].Item2, false); }
            else { tiles_trail_fields[act_player][best_index] = (tiles_trail_fields[act_player][best_index].Item1, false); }
        }
    }

    public void OnMouseDown () {
        if (board.GetComponent<Board>().GetActualPlayer() != board.GetComponent<Board>().GetMyIndexAsPlayer()) {
            return;
        }

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        if (tiles.Length > 0) {
            tiles[0].GetComponent<Tile>().DeactiveBlankTileChanging();
        }

        if (Input.GetKey ("mouse 0") && tiles_changing_action) {
            bool changed = false;
            int how_many_tiles_to_draw = 0;

            foreach (GameObject tile in tiles) {
                if (tile.GetComponent<Tile>().GetIsTileClickedForChange() && tiles_in_bag.Count > 0) {
                    Vector3 previous_position = tile.transform.position;
                    int index = tile.GetComponent<Tile>().GetCoordsListIndex();

                    ChangeTileServerRpc(board.GetComponent<Board>().GetActualPlayer(), NetworkManager.Singleton.LocalClientId, index);                    
                }
            }

            foreach (GameObject tile in tiles) {
                if (tile.GetComponent<Tile>().GetIsTileClickedForChange() && tiles_in_bag.Count > 0) {
                    char previous_letter = tile.GetComponent<Tile>().GetLetter();
                    int previous_value = tile.GetComponent<Tile>().GetValue();

                    if (tile.GetComponent<Tile>().IsBlank()) {
                        previous_letter = '_';
                        previous_value = 2;
                    }

                    Vector3 previous_position = tile.transform.position;
                    int index = tile.GetComponent<Tile>().GetCoordsListIndex();

                    Destroy(tile);
                    AddTilesToBagServerRpc(previous_letter, previous_value);

                    changed = true;
                    how_many_tiles_to_draw++;        
                }
            }

            if (changed) {
                board.GetComponent<Board>().ChangeActualPlayerServerRpc(how_many_tiles_to_draw, "");
            }
        }

        if (Input.GetKey ("mouse 0")) {
            foreach (GameObject tile in tiles) {
                if (tile.GetComponent<Tile>().new_setted_on_board) {
                    tile_change_error.GetComponent<Fading_text>().FadeText();
                    return;
                }
            }

            tiles_changing_action = !tiles_changing_action;

            if (tiles_changing_action) tile_change_information.GetComponent<Fading_text>().MakeTextVisible();
            else tile_change_information.GetComponent<Fading_text>().MakeTextInvisible();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DrawTilesServerRpc(int player_id, int how_many_tiles, ulong clientId) {
        (char, int)[] tiles_to_draw = new(char, int)[7];

        for (int i = 0; i < how_many_tiles; ++i) {
            tiles_to_draw[i] = GetRandomTile();
        }

        SyncDrawTilesNumberClientRpc(tiles_in_bag.Count);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        DrawTilesClientRpc(tiles_to_draw[0].Item1, tiles_to_draw[0].Item2, tiles_to_draw[1].Item1, tiles_to_draw[1].Item2, tiles_to_draw[2].Item1, tiles_to_draw[2].Item2, tiles_to_draw[3].Item1, tiles_to_draw[3].Item2, tiles_to_draw[4].Item1, tiles_to_draw[4].Item2, tiles_to_draw[5].Item1, tiles_to_draw[5].Item2, tiles_to_draw[6].Item1, tiles_to_draw[6].Item2, player_id, clientRpcParams);
        SyncDrawTilesNumberClientRpc(tiles_in_bag.Count);
    }

    [ClientRpc]
    void DrawTilesClientRpc(char ttd0c, int ttd0i, char ttd1c, int ttd1i, char ttd2c, int ttd2i, char ttd3c, int ttd3i, char ttd4c, int ttd4i, char ttd5c, int ttd5i, char ttd6c, int ttd6i, int player_id, ClientRpcParams clientRpcParams = default) {
        (char, int)[] tiles_to_draw = new(char, int)[]{(ttd0c, ttd0i), (ttd1c, ttd1i), (ttd2c, ttd2i), (ttd3c, ttd3i), (ttd4c, ttd4i), (ttd5c, ttd5i), (ttd6c, ttd6i)};
        
        int tiles_on_hand = 0;
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach(GameObject tile in tiles) {
            if (!tile.GetComponent<Tile>().new_setted_on_board && !tile.GetComponent<Tile>().permament_setted_on_board && tile.GetComponent<Tile>().GetPlayerID() == player_id) {
                tiles_on_hand++;
            }
        }

        int tiles_to_draw_index = 0;

        for (int index = 0; index < tiles_trail_fields[player_id].Count && tiles_on_hand < 7; ++index) {
            (Vector2, bool) field = tiles_trail_fields[player_id][index];
            if (!field.Item2) {
                SpawnTile(field.Item1, index, player_id, tiles_to_draw[tiles_to_draw_index]);
                tiles_on_hand++;
                tiles_to_draw_index++;
            } 
        }

        board.GetComponent<Board>().SetPlayerReadyServerRpc(player_id);
    }

    [ClientRpc]
    void SyncDrawTilesNumberClientRpc(int tiles_in_bag) {
        tile_spawner_amount.GetComponent<TextMeshProUGUI>().text = tiles_in_bag.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddTilesToBagServerRpc(char previous_letter, int previous_value) {
        tiles_in_bag.Add((previous_letter, previous_value));

       SyncDrawTilesNumberClientRpc(tiles_in_bag.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeTileServerRpc(int player_id, ulong clientId, int index) {
        (char, int) tiles_to_draw = GetRandomTile();

        SyncDrawTilesNumberClientRpc(tiles_in_bag.Count);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ChangeTileClientRpc(tiles_to_draw.Item1, tiles_to_draw.Item2, player_id, index, clientRpcParams);
    }

    [ClientRpc]
    void ChangeTileClientRpc(char ttd0c, int ttd0i, int player_id, int index, ClientRpcParams clientRpcParams = default) {        
        (Vector2, bool) field = tiles_trail_fields[player_id][index];
        SpawnTile(field.Item1, index, player_id, (ttd0c, ttd0i));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTilePermamentOnBoardServerRpc(int index) {
        SetTilePermamentOnBoardClientRpc(index);
    }

    [ClientRpc]
    void SetTilePermamentOnBoardClientRpc(int index) {
        board_fields[index] = board_fields[index] = (board_fields[index].Item1, board_fields[index].Item2, true);
    }
}