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
        BLACK,  //黒色 = 1
        WHITE,   //白色 = 2
        OUT   //盤外
    }

    const int WIDTH = 11;
    const int HEIGHT = 11;

    //黒い駒
    [SerializeField]         //変数をインスペクターに表示
    GameObject blackObject = null;  //blackObjectという名前のGameObjectを宣言
    //白い駒
    [SerializeField]
    GameObject whiteObject = null;  //whiteObjectという名前のGameObjectを宣言
    //盤
    [SerializeField]
    GameObject emptyObject = null;  //emptyObjectという名前のGameObjectを宣言
    //盤外
    [SerializeField]
    GameObject outObject = null;  //outObjectという名前のGameObjectを宣言

    //盤面のGameObject
    [SerializeField]
    GameObject boardDisplay = null;

    //勝敗を表示するテキスト
    [SerializeField]
    Text resultText = null;

    //盤面
    COLOR[,] board = new COLOR[WIDTH, HEIGHT]; // 8x8の2次元配列
    /* 合法手かどうか調べるのに使う */
    bool[,] checkBoard = new bool[WIDTH, HEIGHT]; // 

    public COLOR player = COLOR.BLACK;
    public COLOR opponent = COLOR.WHITE;

    /* 劫の位置 */
    static int ko_x;
    static int ko_y;

    /* 劫が発生した手数 */
    static int ko_num;

    // Start is called before the first frame update
    void Start()
    {
        Initialize(); //盤面の初期値を設定
        ShowBoard(); //盤面を表示
    }

    //盤面の初期値を設定
    public void Initialize()
    {
        board = new COLOR[WIDTH, HEIGHT]; //追加
        player = COLOR.BLACK; //追加
        int x, y;

        board[4, 3] = COLOR.BLACK;
        board[5, 3] = COLOR.WHITE;

        /*

      for( y=1; y < (WIDTH-1); y++ ){
        for( x=1; x < (WIDTH-1); x++ ){
          board[y,x] = COLOR.EMPTY;
        }
      }
        */

      for( y=0; y < WIDTH; y++ ){
        board[y,0]= COLOR.OUT;
        board[y,WIDTH-1] = COLOR.OUT;
        board[0,y] = COLOR.OUT;
        board[WIDTH-1,y] = COLOR.OUT;
      }
        
        ShowBoard(); //追加

        resultText.text = "";
    }

    //盤面を表示する
    void ShowBoard()
    {
         //boardDisplayの全ての子オブジェクトを削除
        foreach (Transform child in boardDisplay.transform)
        {
            Destroy(child.gameObject); //削除
        }

        for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                 // boardの色に合わせて適切なPrefabを取得
                GameObject piece = GetPrefab(board[h, v]);
                //値がEMPTYならpieceに押下時のイベントを設定
                if (board[h, v] == COLOR.EMPTY)
                {
                    //座標を一時的に保持
                    int x = h;
                    int y = v;
                    //pieceにイベントを設定
                    piece.GetComponent<Button>().onClick.AddListener(() => { PutStone(x + "," + y); });
                }

                //取得したPrefabをboardDisplayの子オブジェクトにする
                piece.transform.SetParent(boardDisplay.transform);
            }
        }

    }

    //色によって適切なprefabを取得して返す
    GameObject GetPrefab(COLOR color)
    {
        GameObject prefab;
        switch (color)
        {
            case COLOR.EMPTY:   //空欄の時
                prefab = Instantiate(emptyObject);
                break;
            case COLOR.BLACK:   //黒の時
                prefab = Instantiate(blackObject);
                break;
            case COLOR.WHITE:   //白の時
                prefab = Instantiate(whiteObject);
                break;
            case COLOR.OUT:   //盤外の時
                prefab = Instantiate(outObject);
                break;
            default:            //それ以外の時(ここに入ることは想定していない)
                prefab = null;
                break;
        }
        return prefab; //取得したPrefabを返す
    }
     //駒を置く
    public void PutStone(string position)
    {
        //positionをカンマで分ける
        int h = int.Parse(position.Split(',')[0]);
        int v = int.Parse(position.Split(',')[1]);

        //board[h, v] = player;

        /* 座標が1～19ならば合法手がどうか調べる */
        if (CheckLegal(player, h, v))
        {
            return;
        }
        else
        {

        }

        //ひっくり返す
        ReverseAll(h, v);
        ShowBoard();
        //ひっくり返していれば相手の番、駒の色を変更する
        if (board[h, v] == player)
        {
            //駒の色を相手の色に変更
            player = player == COLOR.BLACK ? COLOR.WHITE : COLOR.BLACK;
            //相手がパスか判定
            if (CheckPass())
            {
                //相手がパスの場合、駒の色を自分の色に変更
                player = player == COLOR.BLACK ? COLOR.WHITE : COLOR.BLACK;

                //自分もパスか判定
                if (CheckPass())
                {
                    //自分もパスだった場合、勝敗を判定
                    CheckGame();
                }
            }
        }
    }
        //1方向にひっくり返す
    void Reverse(int h, int v, int directionH, int directionV)
    {
        //確認する座標x, yを宣言
        int x = h + directionH, y = v + directionV;
        
                //挟んでいるか確認してひっくり返す
        while (x < WIDTH && x >= 0 && y < HEIGHT && y >= 0)
        {
            //自分の駒だった場合
            if (board[x, y] == player)
            {
                int x2 = h + directionH, y2 = v + directionV;
                int count = 0;　//カウント用の変数を追加
                while (!(x2 == x && y2 == y))
                {
                   board[x2, y2] = player;
                   x2 += directionH;
                   y2 += directionV;
                   count++;
                }
                //1つ以上ひっくり返した場合
                if (count > 0)
                 {
                    //駒を置く
                    board[h, v] = player;
                 }
                break;
            }
            //空欄だった場合
            else if (board[x, y] == COLOR.EMPTY)
            {
                //挟んでいないので処理を終える
                break;
            }

            //確認座標を次に進める
            x += directionH;
            y += directionV;
        }
    }
        //全方向にひっくり返す
    void ReverseAll(int h, int v)
    {
        Reverse(h, v, 1, 0);  //右方向
        Reverse(h, v, -1, 0); //左方向
        Reverse(h, v, 0, -1); //上方向
        Reverse(h, v, 0, 1);  //下方向
        Reverse(h, v, 1, -1); //右上方向
        Reverse(h, v, -1, -1);//左上方向
        Reverse(h, v, 1, 1);  //右下方向
        Reverse(h, v, -1, 1); //左下方向
    }
        //パスを判定する
    bool CheckPass()
    {
        for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                //board[h, v]が空欄の場合
                if (board[h, v] == COLOR.EMPTY)
                {
                    COLOR[,] boardTemp = new COLOR[WIDTH, HEIGHT]; //盤面保存用の変数を宣言
                    Array.Copy(board, boardTemp, board.Length); //盤面の状態を保存用変数に保存しておく
                    ReverseAll(h, v); //座標h,vに駒を置いたとしてひっくり返してみる

                    //ひっくり返せればboard[h, v]に駒が置かれている
                    if (board[h, v] == player)
                    {
                        //ひっくり返したのでパスではない
                        board = boardTemp; //盤面をもとに戻す
                        return false;
                    }
                }
            }
        }
        //1つもひっくり返せなかった場合パス
        return true;
    }

        //勝敗を判定する
    void CheckGame()
    {
        int black = 0;
        int white = 0;

        //駒の数を数える
        for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                switch (board[h, v])
                {
                    case COLOR.BLACK:
                        black++; //黒をカウント
                        break;
                    case COLOR.WHITE:
                        white++; //白をカウント
                        break;
                    default:
                        break;
                }
            }
        }

        if (black > white)
        {
            resultText.text = "黒"+black+"：白"+white+"で黒の勝ち";
        }
        else if (black < white)
        {
            resultText.text = "黒" + black + "：白" + white + "で白の勝ち";
        }
        else
        {
            resultText.text = "黒" + black + "：白" + white + "で引き分け";
        }
    }


    /*------------------------------------------------------------------*/
    /* 合法手かどうか調べる                                             */
    /*------------------------------------------------------------------*/
    public bool CheckLegal(COLOR color, int x, int y)
    {
      /* 自殺手なら置けません */
      if( CheckSuicide( color, x, y )){
        return false;
      }

      /* 以上のチェックをすべてクリアできたので置けます */
      return true;
    }


    /*------------------------------------------------------------------*/
    /* 自殺手かどうか調べる                                             */
    /*------------------------------------------------------------------*/
    public bool CheckSuicide(COLOR color, int x, int y)
    {
      bool rtnVal;
      COLOR opponent;  /* 相手の色 */

      /* 仮に石を置く */
      board[y,x] = player;

      /* マークのクリア */
      ClearCheckBoard();
  
      /* その石は相手に囲まれているか調べる */
      rtnVal = DoCheckRemoveStone( color, x, y );

      /* 囲まれているならば自殺手の可能性あり */
      if(rtnVal){

        /* 相手の色を求める */
        //opponent = reverseColor( color );
        opponent = COLOR.WHITE;

        /* その石を置いたことにより、隣の相手の石が取れるなら自殺手ではない */
        if( x > 1 ){
          /* 隣は相手？ */
          if( board[y,x-1] != player ){
	    /* マークのクリア */
	    ClearCheckBoard();
	    /* 相手の石は囲まれているか？ */
            rtnVal = DoCheckRemoveStone( opponent, x-1, y );
	    /* 相手の石を取れるので自殺手ではない */
            if(rtnVal){
	      /* 盤を元に戻す */
	      board[y,x] = COLOR.EMPTY;
              return false;
            }
          }
        }
        if( y > 1 ){
          /* 隣は相手？ */
          if( board[y-1,x] != player ){
	    /* マークのクリア */
	    ClearCheckBoard();
	    /* 相手の石は囲まれているか？ */
            rtnVal = DoCheckRemoveStone( opponent, x, y-1 );
	    /* 相手の石を取れるので自殺手ではない */
            if( rtnVal){
	      /* 盤を元に戻す */
	      board[y,x] = COLOR.EMPTY;
              return false;
            }
          }
        }
        if( x < (WIDTH-2) ){
          /* 隣は相手？ */
          if( board[y,x+1] != player ){
	    /* マークのクリア */
	    ClearCheckBoard();
	    /* 相手の石は囲まれているか？ */
            rtnVal = DoCheckRemoveStone( opponent, x+1, y );
	    /* 相手の石を取れるので自殺手ではない */
            if( rtnVal){
	      /* 盤を元に戻す */
	      board[y,x] = COLOR.EMPTY;
              return false;
            }
          }
        }
        if( y < (WIDTH-2) ){
          /* 隣は相手？ */
          if( board[y+1,x] != player ){
	    /* マークのクリア */
	    ClearCheckBoard();
	    /* 相手の石は囲まれているか？ */
            rtnVal = DoCheckRemoveStone( opponent, x, y+1 );
	    /* 相手の石を取れるので自殺手ではない */
            if( rtnVal){
	      /* 盤を元に戻す */
	      board[y,x] = COLOR.EMPTY;
              return false;
            }
          }
        }

        /* 盤を元に戻す */
        board[y,x] = COLOR.EMPTY;

        /* 相手の石を取れないなら自殺手 */
        return true;

      }else{

        /* 盤を元に戻す */
        board[y,x] = COLOR.EMPTY;

        /* 囲まれていないので自殺手ではない */
        return false;
      }
    }


    /*------------------------------------------------------------------*/
    /* チェック用の碁盤をクリア                                         */
    /*------------------------------------------------------------------*/
    void ClearCheckBoard()
    {
      int x, y;

      for( y=1; y < (WIDTH-1); y++ ){
        for( x=1; x < (WIDTH-1); x++ ){
          checkBoard[y,x] = false;
        }
      }
    }

    /*------------------------------------------------------------------*/
    /* 座標(x,y)にあるcolor石が相手に囲まれているか調べる               */
    /*------------------------------------------------------------------*/
    /* 空点があればFALSEを返し、空点がなければTRUEを返す */
    bool DoCheckRemoveStone( COLOR player, int x, int y )
    {
      bool rtn;

      /* その場所は既に調べた点ならおしまい */  
      if( checkBoard[y,x]){
        return true;
      }
  
      /* 調べたことをマークする */
      checkBoard[y,x] = true;

      /* 何も置かれていないならばおしまい */
      if( board[y,x] == COLOR.EMPTY ){
        return false;
      }

      /* 同じ色の石ならばその石の隣も調べる */  
      if( board[y,x] == player ){
        /* その石の左(x-1,y)を調べる */
        if( x > 1 ){
          rtn = DoCheckRemoveStone( player, x-1, y );
          if( rtn == false ){
	    return false;
          }
        }
        /* その石の上(x,y-1)を調べる */
        if( y > 1 ){
          rtn = DoCheckRemoveStone( player, x, y-1 );
          if( rtn == false){
	    return false;
          }
        }
        /* その石の右(x+1,y)を調べる */
        if( x < (WIDTH-2) ){
          rtn = DoCheckRemoveStone( player, x+1, y );
          if( rtn == false ){
	    return false;
          }
        }
        /* その石の下(x,y+1)を調べる */
        if( y < (WIDTH-2) ){
          rtn = DoCheckRemoveStone( player, x, y+1 );
          if( rtn == false ){
	    return false;
          }
        }
      }

      /* 相手の色の石があった */  
      return true;
    }

    /*------------------------------------------------------------------*/
    /* 勝敗の判定                                                       */
    /* このプログラムでは地を数えていません                             */
    /* アゲハマの多い方を勝ちと判定しています                           */
    /*------------------------------------------------------------------*/
    
    void CountScore()
    {
      int black_score;
      int white_score;

      /* 碁盤上の地を数えるべきところだが省略 */
      black_score = 0;
      white_score = 0;

      /* アゲハマを加える */
      black_score += black_prisoner;
      white_score += white_prisoner;

      /* 黒－白を返す */
      return( black_score - white_score );
    }

    /*------------------------------------------------------------------*/
    /* 碁盤に石を置く                                                   */
    /*------------------------------------------------------------------*/
    
    void SetStone( COLOR color, int x, int y )
    {
      int prisonerN;    /* 取り除かれた石の数（上） */
      int prisonerE;    /* 取り除かれた石の数（右） */
      int prisonerS;    /* 取り除かれた石の数（下） */
      int prisonerW;    /* 取り除かれた石の数（左） */
      int prisonerAll;  /* 取り除かれた石の総数 */
      bool koFlag;       /* 劫かどうか */

      /* 座標(x,y)に石を置く */
      board[y,x] = color;

      /* 置いた石の隣に同じ色の石はあるか？ */
      if( board[y+1,x] != color &&
          board[y-1,x] != color &&
          board[y,x+1] != color &&
          board[y,x-1] != color ){
        /* 同じ色の石がないならば劫かもしれない */
        koFlag = true;
      }else{
        /* 同じ色の石があるならば劫ではない */
        koFlag = false;
      }

      /* 取り除かれた石の数 */
      prisonerN = prisonerE = prisonerS = prisonerW = 0;

      /* 置いた石の周囲の相手の石が死んでいれば碁盤から取り除く */
      if( y > 1 ){
        prisonerN = RemoveStone( color, x, y-1 );
      }
      if( x > 1 ){
        prisonerW = RemoveStone( color, x-1, y );
      }
      if( y < (WIDTH-2) ){
        prisonerS = RemoveStone( color, x, y+1 );
      }
      if( x < (WIDTH-2) ){
        prisonerE = RemoveStone( color, x+1, y );
      }

      /* 取り除かれた石の総数 */
      prisonerAll = prisonerN + prisonerE + prisonerS + prisonerW;

      /* 置いた石の隣に同じ色の石がなく、取り除かれた石も１つならば劫 */
      if( koFlag == true && prisonerAll == 1 ){
        /* 劫の発生した手数を覚える */
        ko_num = move;
        /* 劫の座標を覚える */
        if( prisonerE == 1 ){
          /* 取り除かれた石が右 */
          ko_x = x+1;
          ko_y = y;
        }else if( prisonerS == 1 ){
          /* 取り除かれた石が下 */
          ko_x = x;
          ko_y = y+1;
        }else if( prisonerW == 1 ){
          /* 取り除かれた石が左 */
          ko_x = x-1;
          ko_y = y;
        }else if( prisonerN == 1 ){
          /* 取り除かれた石が上 */
          ko_x = x;
          ko_y = y-1;
        }
      }

      /* アゲハマの更新 */
      if( prisonerAll > 0 ){
        if( color == COLOR.BLACK ){
          black_prisoner += prisonerAll;
        }else if( color == COLOR.WHITE ){
          white_prisoner += prisonerAll;
        }
      }
    }

    /*------------------------------------------------------------------*/
    /* 座標(x,y)の石が死んでいれば碁盤から取り除く                      */
    /*------------------------------------------------------------------*/
    /* 碁盤から取り除かれた石数 */
    int RemoveStone( COLOR color, int x, int y )
    {
      int prisoner;  /* 取り除かれた石数 */

      /* 置いた石と同じ色なら取らない */
      if( board[y,x] == color ){
        return( 0 );
      }

      /* 空点なら取らない */
      if( board[y,x] == COLOR.EMPTY ){
        return( 0 );
      }

      /* マークのクリア */
      ClearCheckBoard();

      /* 囲まれているなら取る */
      if( DoCheckRemoveStone( board[y,x], x, y ) == true ){
        prisoner = DoRemoveStone( board[y,x], x, y, 0 );
        return( prisoner );
      }

      return( 0 );
    }

    /*------------------------------------------------------------------*/
    /* 座標(x,y)のcolor石を碁盤から取り除き、取った石の数を返す         */
    /*------------------------------------------------------------------*/
    int DoRemoveStone( COLOR color, int x, int y, int prisoner )
    {
      /* 取り除かれる石と同じ色ならば石を取る */
      if( board[y,x] == player){

        /* 取った石の数を１つ増やす */
        prisoner++;

        /* その座標に空点を置く */
        board[y,x] = COLOR.EMPTY;

        /* 左を調べる */
        if( x > 1 ){
          prisoner = DoRemoveStone( color, x-1, y, prisoner );
        }
        /* 上を調べる */
        if( y > 1 ){
          prisoner = DoRemoveStone( color, x, y-1, prisoner );
        }
        /* 右を調べる */
        if( x < (WIDTH-2) ){
          prisoner = DoRemoveStone( color, x+1, y, prisoner );
        }
        /* 下を調べる */
        if( y < (WIDTH-2) ){
          prisoner = DoRemoveStone( color, x, y+1, prisoner );
        }
      }

      /* 取った石の数を返す */
      return prisoner;
    }
}