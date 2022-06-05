using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour {
    private char letter;
    private int value;
    private bool is_tile_clicked_for_change;

    [SerializeField]
    private Color clicked_color = new Color(0, 0, 0.5f, 1);
    [SerializeField]
    private Color new_tile_color = new Color(0, 1, 0, 1);
    [SerializeField]
    private Color blank_tile_color = new Color(243, 248, 18, 255);
    private Color neutral_color = new Color(1,1,1,1);

    [SerializeField]
    private Vector3 tile_in_trail_scale = new Vector3(0.6f, 0.6f, 1.0f);
    [SerializeField]
    private Vector3 tile_on_board_scale = new Vector3(0.4f, 0.4f, 1.0f);
    
    private SpriteRenderer my_sprite;

    private GameObject tile_spawner;

    private bool dragging;
    public bool new_setted_on_board;
    public bool permament_setted_on_board;

    private Vector2 board_field_coord;

    private int coords_list_index;

    private bool blank_tile;
    private bool blank_tile_assigning_letter;
    private GameObject blank_tile_information;

    private bool game_ended = false;

    [SerializeField]
    private int player_id;
    private bool my_turn;

    public bool GetIsTileClickedForChange() { return is_tile_clicked_for_change; }
    public char GetLetter() { return letter; }
    public int GetValue() { return value; }
    public Vector2 GetBoardFieldCoord() { return board_field_coord; }
    public int GetCoordsListIndex() { return coords_list_index; }
    public void SetCoordsListIndex(int index) { coords_list_index = index; }
    public bool IsBlank() { return blank_tile; }
    public void DeactiveBlankChangingVariable() { blank_tile_assigning_letter = false; }
    public int GetPlayerID() { return player_id; }

    private void InitializeGameObjects() {
        tile_spawner = GameObject.Find("Tile_spawner");

        blank_tile_information = GameObject.Find("Blank_tile_information");
        blank_tile_information.GetComponent<Fading_text>().MakeTextInvisible();
    }

    void Awake() {
        my_sprite = gameObject.GetComponent<SpriteRenderer>();
        my_sprite.color = neutral_color;
        is_tile_clicked_for_change = false;
        transform.localScale = tile_in_trail_scale;

        dragging = false;
        new_setted_on_board = false;
        permament_setted_on_board = false;

        board_field_coord = new Vector2(0,0);

        coords_list_index = -1;

        blank_tile = false;
        blank_tile_assigning_letter = false;

        player_id = -1;
        InitializeGameObjects();  
        
        my_turn = false;  
    }

    public void SpawnOnClient(bool is_blank) {
        RescaleToBoard();
        if (is_blank) {
            neutral_color = blank_tile_color;
        }

        PlaceTilePermamentOnBoard();
    }

    public void RescaleToBoard() {
        transform.localScale = tile_on_board_scale;
    }

    public void RescaleToTrail() {
        transform.localScale = tile_in_trail_scale;
    }

    public void DeactiveBlankTileChanging() {
        blank_tile_information.GetComponent<Fading_text>().MakeTextInvisible();

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in tiles) {
            if (tile.GetComponent<Tile>().IsBlank()) {
                tile.GetComponent<Tile>().DeactiveBlankChangingVariable();
            }
        }        
    }

    public void OnMouseDown () {
        if (permament_setted_on_board ||  !my_turn ||  game_ended) { return; }
        
        if (Input.GetKey ("mouse 0") && tile_spawner.GetComponent<Tile_spawner>().tiles_changing_action) {
            is_tile_clicked_for_change = !is_tile_clicked_for_change;
            
            if (is_tile_clicked_for_change) {
                my_sprite.color = clicked_color;
            }
            else {
                my_sprite.color = neutral_color;
            }
        }
    }

    private void ChangeBlankLetter() {
        if (!tile_spawner.GetComponent<Tile_spawner>().tiles_changing_action && blank_tile) {
            if (blank_tile_assigning_letter) {
                DeactiveBlankTileChanging();
            }
            else {
                DeactiveBlankTileChanging();
                blank_tile_assigning_letter = true;
                blank_tile_information.GetComponent<Fading_text>().MakeTextVisible();
            }
        }
    }

    private void DragTile() {
        if(dragging && !tile_spawner.GetComponent<Tile_spawner>().tiles_changing_action) {
            tile_spawner.GetComponent<Tile_spawner>().FreeCoords(new_setted_on_board, coords_list_index);
            (Vector2, bool, Vector2, int) new_pos = tile_spawner.GetComponent<Tile_spawner>().GetClosestAttractField(transform.position);
            transform.position = new_pos.Item1;

            coords_list_index = new_pos.Item4;

            if (new_pos.Item2) { 
                RescaleToBoard(); 
                new_setted_on_board = true;
                board_field_coord = new_pos.Item3;
                NewSettedOnBoardColor();
            }
            else { 
                RescaleToTrail();
                new_setted_on_board = false;
                board_field_coord = new_pos.Item3;
                NormalColor();
            }
        }

        if (dragging && !blank_tile) {
            DeactiveBlankTileChanging();
        }

        dragging = false;
    }    

    public void OnMouseUp() {
        if (permament_setted_on_board || /* !my_turn || */ game_ended) { return; }

        if (my_turn) ChangeBlankLetter();
        DragTile();
    }

    private void ChangeSprite() {
        if (permament_setted_on_board) { return; }
        my_sprite.sprite = Resources.Load("Tiles_sprites/" + ChangeSceneClass.language_abbreviation + "/letter_" + char.ToLower(letter), typeof(Sprite)) as Sprite;
    }

    public void NewSettedOnBoardColor() {
        if (permament_setted_on_board) { return; }

        my_sprite.color = new_tile_color; 
    }

    private void NormalColor() {
        my_sprite.color = neutral_color;
    }

    private void BlankTileColor() {
        neutral_color = blank_tile_color;
        my_sprite.color = neutral_color;
    }

    public void SetParameters(char letter, int value, int player_id) {
        if (permament_setted_on_board) { return; }

        this.letter = letter;
        this.value = value;
        this.player_id = player_id;

        if (letter == '_') { blank_tile = true; }

        ChangeSprite();
    }

    public void OnMouseDrag() {
        if (permament_setted_on_board || /* !my_turn || */ game_ended) { return; }

        if (!tile_spawner.GetComponent<Tile_spawner>().tiles_changing_action) {
            transform.position = GetMousePos();
            RescaleToTrail();

            dragging = true;
        }
    }

    Vector3 GetMousePos() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        return mousePos;
    }

    public void PlaceTilePermamentOnBoard() {
        new_setted_on_board = false;
        permament_setted_on_board = true;

        NormalColor(); 
        transform.tag = "UnactiveTile";
        GetComponent<Tile>().enabled = true;; 
    }

    public void BackToHand() {
        tile_spawner.GetComponent<Tile_spawner>().FreeCoords(new_setted_on_board, coords_list_index);
        (Vector2, Vector2, int) new_pos = tile_spawner.GetComponent<Tile_spawner>().BackTileToHand();

        transform.position = new_pos.Item1;

        coords_list_index = new_pos.Item3;

        RescaleToTrail();
        new_setted_on_board = false;
        board_field_coord = new_pos.Item2;
        NormalColor();
    }

    private void ChangeBlankLetterListener() {
        if (!blank_tile_assigning_letter) { return; }
        
        string key_pressed = ""; 
        bool is_alt_clicked = false;

        foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKey(kcode)) {
                string key = kcode + "";
                if (key == "LeftAlt" || key == "RightAlt") { is_alt_clicked = true; }
                else { key_pressed = key; }

                if (key == "LeftControl" || key == "RightControl") { 
                    DeactiveBlankTileChanging();
                    return;    
                }
            }                
        }

        key_pressed = key_pressed.ToUpper();

        if (key_pressed.Length == 1) {
            char c = key_pressed.ToCharArray()[0];

            if (!char.IsLetter(c)) { return; }

            if (is_alt_clicked && ChangeSceneClass.language_abbreviation == "PL") {
                if (c == 'A') c = 'Ą';
                if (c == 'E') c = 'Ę';
                if (c == 'O') c = 'Ó';
                if (c == 'S') c = 'Ś';
                if (c == 'L') c = 'Ł';
                if (c == 'Z') c = 'Ż';
                if (c == 'X') c = 'Ź';
                if (c == 'C') c = 'Ć';
                if (c == 'N') c = 'Ń';
            }

            string control_chars = "AĄBCĆDEĘFGHIJKLŁMNŃOÓPRTSŚTUWZŻŹ";

            if (ChangeSceneClass.language_abbreviation == "EN") {
                control_chars = "QWERTYUIOPASDFGHJKLZXCVBNM";
            }

            if (!control_chars.Contains(c)) { return; }

            SetParameters(c, 2, player_id);
            BlankTileColor();
            DeactiveBlankTileChanging();
        }
    }

    void Update() {
        ChangeBlankLetterListener();
    }

    public void NotMyTurn() {
        my_turn = false;
        //GetComponent<BoxCollider2D>().enabled = false;
    }

    public void MyTurn() {
        my_turn = true;
        //GetComponent<BoxCollider2D> ().enabled = true;
    }

    public void GameEnded() {
        game_ended = true;
    }
}