using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームの流れを管理
/// </summary>
public class GameController : Singleton<GameController>
{
    /// <summary>
    /// 縦方向のマス数   
    /// </summary>
    [SerializeField, Range(2, 16)] private int VerticalNum = 9;
    /// <summary>
    /// 横方向のマス目
    /// </summary>
    [SerializeField, Range(2, 16)] private int HorizontalNum = 9;

    /// <summary>
    /// Lineを制御するため
    /// </summary>
    [SerializeField] LineController m_LineController;

    /// <summary>
    /// デモ映像を流すか(trueで、かつマス目が9×9なら流す）
    /// </summary>
    [SerializeField] private bool playVideo = false;

    /// <summary>
    /// 盤面上の1つの目のプレハブ
    /// </summary>
    public GameObject BoardCrossPrefab;
    /// <summary>
    /// 目の親オブジェクト
    /// </summary>
    public Transform BoardCrossParent;

    /// <summary>
    /// カメラ情報
    /// </summary>
    private RaycastHit hit;

    /// <summary>
    /// 自プレイヤーの使う石の色
    /// </summary>
    public BoardCross.Status playerColor = BoardCross.Status.White;
    /// <summary>
    /// 相手プレイヤーの使う石の色
    /// </summary>
    public BoardCross.Status opponentColor
    {
        get
        {
            switch (playerColor)
            {
                default:
                case BoardCross.Status.White:
                    return BoardCross.Status.Black;
                case BoardCross.Status.Black:
                    return BoardCross.Status.White;
            }
        }
    }
    /// <summary>
    /// 相手の石の数
    /// </summary>
    public int OpponentStonesCount
    {
        get
        {
            if (opponentColor == BoardCross.Status.Black)
            {
                return BoardCross.BlackField.Count;
            }
            if (opponentColor == BoardCross.Status.White)
            {
                return BoardCross.WhiteField.Count;
            }
            return 0;
        }
    }
    /// <summary>
    /// 相手が置ける石の最大値
    /// これを過ぎるとゲームオーバー
    /// </summary>
    public int maxOpponentStonesCount = 50;

    /// <summary>
    /// 最後の手を打ってからの時間
    /// </summary>
    [HideInInspector]
    public float timeFromLastMove = 0f;
    [SerializeField]
    private float maxTimeFromLastMove = 60f;

    /// <summary>
    /// ゲームの種類
    /// </summary>
    public enum GameType
    {
        PlayWithComputer,   // コンピュータと対局
        Demo                // デモ
    }
    /// <summary>
    /// 現在のゲームの種類
    /// </summary>
    public GameType currentGameType = GameType.PlayWithComputer;

    protected override void Awake()
    {
        base.Awake();

        // 指定されたマス目に調整する
        BoardCross.BOARD_SIZE_V = VerticalNum + 2;
        BoardCross.BOARD_SIZE_H = HorizontalNum + 2;

        // 盤面生成
        for (int i=0; i<BoardCross.BOARD_SIZE_H; i++)
        {
            for (int j=0; j<BoardCross.BOARD_SIZE_V; j++)
            {
                GameObject go = Instantiate(BoardCrossPrefab, BoardCrossParent);
                BoardCross board = go.GetComponent<BoardCross>() ?? go.AddComponent<BoardCross>();
                board.Initialize(i, j);
            }
        }
        //盤面の線を指定されたマス目に合わせる
        m_LineController.AdjustLines(HorizontalNum, VerticalNum);

        // 線生成
        ConnectingLineFactory.Instance.GenerateConnectingLineInstance();
    }

    private void Update()
    {
        // コンピュータと対局
        if (currentGameType == GameType.PlayWithComputer)
        {
            // マウスクリック
            if (Input.GetMouseButtonDown(0))
            {
                // マウスのポジションを取得してRayに代入
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // マウスのポジションからRayを投げて何かに当たったらhitに入れる
                if (Physics.Raycast(ray, out hit))
                {
                    // Boardレイヤーなら
                    BoardCross board = hit.collider.GetComponent<BoardCross>();
                    if (board != null)
                    {
                        // 合法手がどうか調べる 
                        if (board.IsLegalMove(playerColor))
                        {
                            // 石を置く
                            board.BoardStatus = playerColor;

                            // 石を打つ音
                            AudioController.Instance.PlayPutStone();

                            // 隣の石を取り除く
                            List<BoardCross> Prisoners = board.RemoveStoneAll(playerColor);

                            // 石を取った石も取り除く
                            List<BoardCross> SiegingStone = BoardCross.SiegingBoardCross(Prisoners);
                            foreach (BoardCross sieging in SiegingStone)
                            {
                                sieging.BoardStatus = BoardCross.Status.None;
                            }

                            // 取り除いた場合、音を鳴らす
                            if (Prisoners.Count > 0)
                            {
                                AudioController.Instance.PlayRemoveStone();

                                // コンピュータをちょっとだけ強くする
                                //OpponentController.Instance.SpanAverage -= 0.03f;
                            }

                            // 取り除かなかった場合、置いた石に接している石に対して、線が引けるなら引く
                            else
                            {
                                //board.ActivateOpponentLine();
                                foreach (BoardCross neighborhood in board.Neighborhood8)
                                {
                                    if (neighborhood.BoardStatus == board.OpponentStatus)
                                    {
                                        neighborhood.ActivateOpponentLine();
                                    }
                                }
                            }

                            // 最後の手を打った時間をリセット
                            timeFromLastMove = 0f;
                        }
                        else
                        {
                            // エラー音
                            AudioController.Instance.PlayError();
                        }
                    }
                }
            }

            // 相手（COM）の手
            if (OpponentController.Instance.IsCooledDown)
            {
                BoardCross board = OpponentController.Instance.Move(BoardCross.Field);
                if (board.IsLegalMove(opponentColor))
                {
                    // 石を置く
                    board.BoardStatus = opponentColor;

                    // 石を打つ音
                    //AudioController.Instance.PlayPutStone(); // 20221003 音がうるさいのでコメントアウト

                    // 隣の石を取り除く
                    List<BoardCross> Prisoners = board.RemoveStoneAll(opponentColor);

                    // 石を取った石も取り除く
                    List<BoardCross> SiegingStone = BoardCross.SiegingBoardCross(Prisoners);
                    foreach (BoardCross sieging in SiegingStone)
                    {
                        sieging.BoardStatus = BoardCross.Status.None;
                    }

                    // 音を鳴らす
                    if (Prisoners.Count > 0)
                    {
                        AudioController.Instance.PlayRemoveStone();
                    }

                    // 取り除かなかった場合、置いた石に線が引けるなら引く
                    else
                    {
                        board.ActivateOpponentLine();
                    }
                }
            }

            // 最後に打った手から一定時間経った場合は、リセットしてデモ状態に移行する
            if (maxTimeFromLastMove < timeFromLastMove && BoardCross.Field.Count == 121 && playVideo)
            {
                BoardCross.ClearStoneAll();
                OpponentController.Instance.ResetSpanAverage();
                DemoController.Instance.InitializeMoveRecord();
                currentGameType = GameType.Demo;
            }
        }

        // デモ映像（マス目が9×9のときのみできるようにする）
        else if (currentGameType == GameType.Demo && BoardCross.Field.Count == 121 && playVideo)
        {
            // 棋譜がこれ以上ない場合はリセットして棋譜を再初期化する
            if (DemoController.Instance.MoveRecord.Count <= 0)
            {
                Debug.Log($"GameController.Update: Demo MoveRecord does not have next move. Restart.");
                BoardCross.ClearStoneAll();
                DemoController.Instance.InitializeMoveRecord();
            }

            if (DemoController.Instance.IsCooledDown)
            {
                // 次の一手を決める
                BoardCross nextBoard = DemoController.Instance.Move();
                BoardCross.Status nextColor = DemoController.Instance.NextColor;
                nextBoard.BoardStatus = nextColor;

                // 石を置く場合
                if (nextColor == BoardCross.Status.Black || nextColor == BoardCross.Status.White)
                {
                    // 石を打つ音
                    AudioController.Instance.PlayPutStone();

                    // 隣の石を取り除く
                    List<BoardCross> Prisoners = nextBoard.RemoveStoneAll(nextColor);

                    // 石を取った石も取り除く
                    List<BoardCross> SiegingStone = BoardCross.SiegingBoardCross(Prisoners);
                    foreach (BoardCross sieging in SiegingStone)
                    {
                        sieging.BoardStatus = BoardCross.Status.None;
                    }

                    // 音を鳴らす
                    if (Prisoners.Count > 0)
                    {
                        AudioController.Instance.PlayRemoveStone();
                    }

                    // 取り除かなかった場合、置いた石に線が引けるなら引く
                    else
                    {
                        //nextBoard.ActivateOpponentLine();
                    }
                }
            }

            // マウスクリックをした場合はリセットしてゲーム状態に移行する
            if (Input.GetMouseButtonDown(0))
            {
                BoardCross.ClearStoneAll();
                OpponentController.Instance.ResetSpanAverage();
                currentGameType = GameType.PlayWithComputer;
                timeFromLastMove = 0f;
            }
        }

        // 相手の石が一定数を超えた場合はゲームオーバーになりリセットする
        if (maxOpponentStonesCount <= OpponentStonesCount)
        {
            BoardCross.ClearStoneAll();
            OpponentController.Instance.ResetSpanAverage();
        }

        // 時間更新
        timeFromLastMove += Time.deltaTime;
    }

    // デバッグ用
    int windowId = 0;
    Rect windowRect = new Rect(0, 0, 250, 200);
    private void OnGUI()
    {
        windowRect = GUI.Window(windowId, windowRect, (Id) =>
        {
            // ゲームタイプ表示
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Game Type:");
            GUILayout.FlexibleSpace();
            GUILayout.Label(currentGameType.ToString());
            GUILayout.EndHorizontal();

            // プレイヤーの色を変更
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Player:");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(playerColor.ToString()))
            {
                if (playerColor == BoardCross.Status.White)
                {
                    playerColor = BoardCross.Status.Black;
                }
                else if (playerColor == BoardCross.Status.Black)
                {
                    playerColor = BoardCross.Status.White;
                }
            }
            GUILayout.EndHorizontal();

            // 盤面リセット
            if (GUILayout.Button("Reset Board"))
            {
                BoardCross.ClearStoneAll();
            }

            // 最後の手を打ってからの時間表示
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time from Last Move:");
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{timeFromLastMove:0.00} sec");
            GUILayout.EndHorizontal();

            // 相手（COM）のクールダウン時間
            GUILayout.BeginHorizontal();
            GUILayout.Label("COM Span:");
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{OpponentController.Instance.SpanAverage:0.00} sec");
            GUILayout.EndHorizontal();
            OpponentController.Instance.SpanAverage = GUILayout.HorizontalSlider(OpponentController.Instance.SpanAverage, 0f, 10f);

            // 移動可
            GUI.DragWindow();
        }, "Game Controller (Debug)");
    }
}