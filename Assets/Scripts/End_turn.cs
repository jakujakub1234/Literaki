using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;

public class End_turn : NetworkBehaviour
{
    public bool game_ended;
    private bool button_active;
    private SpriteRenderer my_sprite;

    private GameObject board; 
    private GameObject button_text;
    private Tile_spawner tile_spawner;

    [SerializeField]
    private Color clicked_button = new Color(19, 37, 111, 255);
    private Color neutral_color = new Color(1,1,1,1);
    [SerializeField]
    private Color text_active_color = new Color(20, 255, 31, 255);
    [SerializeField]
    private Color text_inactive_color = new Color(255, 20, 31, 255);

    private bool end_turn_correct;
    private int end_turn_player_id;
    private int end_turn_how_many_tiles;

    private void InitializeGameObjects() {
        board = GameObject.Find("Board");
        button_text = GameObject.Find("End_turn_information");
        tile_spawner = GameObject.Find("Tile_spawner").GetComponent<Tile_spawner>();
    }

    void Awake() {
        button_active = true;
        game_ended = false;

        my_sprite = gameObject.GetComponent<SpriteRenderer>();
        my_sprite.color = neutral_color;

        InitializeGameObjects();
    }

    public void SetButtonActive() {
        button_active = true;
        button_text.GetComponent<TextMeshProUGUI>().color = text_active_color;
    }

    public void SetButtonInactive() {
        button_active = false;
        button_text.GetComponent<TextMeshProUGUI>().color = text_inactive_color;
    }

    public void OnMouseDrag() {
        if (game_ended || !button_active || tile_spawner.tiles_changing_action) { return; }

        my_sprite.color = clicked_button;
    }

    public void OnMouseUp() {
        if (game_ended || !button_active || tile_spawner.tiles_changing_action) { return; }
        my_sprite.color = neutral_color;
    }

    public void OnMouseDown () {
        if (game_ended || !button_active || tile_spawner.tiles_changing_action) { return; }

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        if (tiles.Length > 0) {
            tiles[0].GetComponent<Tile>().DeactiveBlankTileChanging();
        }

        if (Input.GetKey ("mouse 0")) {
            my_sprite.color = clicked_button;

            end_turn_correct = false;
            end_turn_player_id = -1;
            end_turn_how_many_tiles = 0;

            board.GetComponent<Board>().EndTurnResetServerRpc();

            int server_how_many_tiles_used = 0;
            GameObject[] tiles_server_sync = GameObject.FindGameObjectsWithTag("Tile");

            foreach (GameObject tile in tiles_server_sync) {
                Tile tile_script = tile.GetComponent<Tile>();
                
                if (tile_script.new_setted_on_board) {
                    server_how_many_tiles_used++;
                }
            }

            board.GetComponent<Board>().HowManyLettersUsedServerRpc(server_how_many_tiles_used);

            foreach (GameObject tile in tiles_server_sync) {
                Tile tile_script = tile.GetComponent<Tile>();
                
                if (tile_script.new_setted_on_board) {
                    Vector2 coords = tile_script.GetBoardFieldCoord();

                    board.GetComponent<Board>().AddLetterToBufforServerRpc((int)coords[0], (int)coords[1], tile_script.GetLetter());
                }
            }

            board.GetComponent<Board>().EndTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    public void SetEndTurnSynVars(bool correct, int player_id, int how_many_tiles, bool change_player, string error_word) {
        end_turn_correct = correct;
        end_turn_player_id = player_id;
        end_turn_how_many_tiles = how_many_tiles;
        
        button_text.GetComponent<TextMeshProUGUI>().color = text_inactive_color;
        my_sprite.color = neutral_color;

        if (end_turn_correct) { 
            board.GetComponent<Board>().CheckEndGame();

            tile_spawner.DrawTilesServerRpc(end_turn_player_id, end_turn_how_many_tiles, NetworkManager.Singleton.LocalClientId);
        }  

        if (change_player) board.GetComponent<Board>().ChangeActualPlayerServerRpc(0, error_word);
    }
}