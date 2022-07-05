using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class GameController : MonoBehaviour
{
    // enumを使って数字に名前をつける
    public enum COLOR
    {
        EMPTY,  //空欄 = 0
    }

    const int WIDTH = 11;
    const int HEIGHT = 11;

    //黒い駒
    [SerializeField]         //変数をインスペクターに表示
    public  Sprite blackSpr = null;  //blackObjectという名前のGameObjectを宣言
    //白い駒
    [SerializeField]
    public  Sprite whiteSpr = null;  //whiteObjectという名前のGameObjectを宣言
    //盤
    [SerializeField]
    public  GameObject emptyObj = null;  //emptyObjectという名前のGameObjectを宣言
    //盤外
    [SerializeField]
    public  Sprite outSpr = null;  //outObjectという名前のGameObjectを宣言

    //盤面のGameObject
    [SerializeField]
    GameObject boardDisplay = null;

    //勝敗を表示するテキスト
    [SerializeField]
    Text resultText = null;

    //盤面
    GameObject[,] board = new GameObject[WIDTH, HEIGHT]; // 8x8の2次元配列

    public Sprite player = null;
    public Sprite opponent = null;

    // Start is called before the first frame update
    void Start()
    {
        Initialize(); //盤面の初期値を設定
        ShowBoard(); //追加
    }

    //盤面の初期値を設定
    public void Initialize()
    {
        board = new GameObject[WIDTH, HEIGHT]; //追加
        player = blackSpr;//追加
        int x, y;

        //board[1, 1].GetComponent<Image>().sprite = blackSpr;

        for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                //boardの色に合わせて適切なPrefabを取得
                //GameObject piece = GetPrefab(board[h, v]);
                GameObject piece = Instantiate(emptyObj);
                //値がEMPTYならpieceに押下時のイベントを設定
                if (board[h, v] == null)
                {
                    //座標を一時的に保持
                    x = h;
                    y = v;
                    
                    //board[h,v] = piece.GetComponent<Image>().sprite;
                    Debug.Log(board[h,v]);

                    //pieceにイベントを設定
                    piece.GetComponent<Button>().onClick.AddListener(() => { PutStone(x + "," + y); });
                    Debug.Log("e");
                    //piece.GetComponent<Button>().onClick.AddListener(() => { SetStone(player, x, y); });
                }

                //取得したPrefabをboardDisplayの子オブジェクトにする
                piece.transform.SetParent(boardDisplay.transform);
            }
        }

        for (y = 0; y < WIDTH; y++)
        {
            board[y, 0].GetComponent<Image>().sprite = outSpr;
            board[y, WIDTH - 1].GetComponent<Image>().sprite = outSpr;
            board[0, y].GetComponent<Image>().sprite = outSpr;
            board[WIDTH - 1, y].GetComponent<Image>().sprite = outSpr;
        }

    }

    //盤面を表示する
    void ShowBoard()
    {
        //boardDisplayの全ての子オブジェクトを削除
        foreach (Transform child in boardDisplay.transform)
        {
            Destroy(child.gameObject); //削除
        }
    }

    //駒を置く
    public void PutStone(string position)
    {
        //positionをカンマで分ける
        int x = int.Parse(position.Split(',')[0]);
        int y = int.Parse(position.Split(',')[1]);

        board[x, y].GetComponent<Image>().sprite = blackSpr;

        //ひっくり返す
        //ReverseAll(x, y);
        //ShowBoard();

        int prisonerN;    /* 取り除かれた石の数（上） */
        int prisonerE;    /* 取り除かれた石の数（右） */
        int prisonerS;    /* 取り除かれた石の数（下） */
        int prisonerW;    /* 取り除かれた石の数（左） */
        int prisonerAll;  /* 取り除かれた石の総数 */
        bool koFlag;       /* 劫かどうか */

        /* 座標(x,y)に石を置く */
        board[x, y].GetComponent<Image>().sprite = blackSpr;
        Debug.Log("a");

    }
}