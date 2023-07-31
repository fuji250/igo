using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 盤上の目1つを表す
/// </summary>
public class BoardCross: MonoBehaviour, IComparable<BoardCross>
{
    #region 座標等
    /// <summary>
    /// 盤の1辺の長さ9+端の分2
    /// </summary>
    public static int BOARD_SIZE_V = 11;
    public static int BOARD_SIZE_H = 11;
    /// <summary>
    /// x座標 1～9
    /// </summary>
    public int X
    {
        private set; get;
    }
    /// <summary>
    /// z座標 1～9
    /// </summary>
    public int Z
    {
        private set; get;
    }
    /// <summary>
    /// (x, z)
    /// </summary>
    public Vector2 coordinate
    {
        get
        {
            return new Vector2(X, Z);
        }
    }
    /// <summary>
    /// 天元の座標
    /// </summary>
    public static Vector2 CenterCoordinate
    {
        set; get;
    }
    /// <summary>
    /// この目が碁盤の外にあるかどうか
    /// </summary>
    public bool IsOut
    {
        get
        {
            return X < 1 || BOARD_SIZE_H - 2 < X || Z < 1 || BOARD_SIZE_V - 2 < Z;
        }
    }
    /// <summary>
    /// 比較演算子（ソートに使用）
    /// 左下が弱く、右上が強い
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(BoardCross other)
    {
        if (other == null) return 1;
        return (X + Z) - (other.X + other.Z);
    }
    #endregion

    #region 目の状態等
    /// <summary>
    /// 現在の目の状態
    /// </summary>
    public enum Status
    {
        None,   // 何も置かれていない
        Black,  // 黒石が置かれている
        White,  // 白石が置かれている
        Out,    // 盤面の外
    }
    private Status boardStatus;
    /// <summary>
    /// 現在の目の状態
    /// </summary>
    public Status BoardStatus
    {
        get
        {
            return boardStatus;
        }
        set
        {
            boardStatus = value;

            // ビジュアルも反映させる
            UpdateBoardVis(value);

            //BlackStone.localScale = Vector3.one;
            //WhiteStone.localScale = Vector3.one;
        }
    }
    /// <summary>
    /// 現在置かれている石と対立する石の色
    /// </summary>
    public Status OpponentStatus
    {
        get
        {
            if (BoardStatus == Status.Black) return Status.White;
            else if (BoardStatus == Status.White) return Status.Black;
            else
            {
                return Status.None;
            }
        }
    }
    /// <summary>
    /// この目に乗る黒石
    /// </summary>
    [SerializeField]
    private Transform BlackStone;
    /// <summary>
    /// この目に乗る白石
    /// </summary>
    [SerializeField]
    private Transform WhiteStone;
    /// <summary>
    /// 黒石が消える時のエフェクト
    /// </summary>
    [SerializeField]
    private ParticleSystem blackParticle;
    /// <summary>
    /// 白石が消える時のエフェクト
    /// </summary>
    [SerializeField]
    private ParticleSystem whiteParticle;
    /// <summary>
    /// 石の見た目を示すSpriteRenderer
    /// </summary>
    [SerializeField]
    private List<SpriteRenderer> StoneVis;
    #endregion

    #region 目の寿命等
    /// <summary>
    /// 石が置かれてから消えるまでの時間（秒）
    /// 寿命概念を消すと線引きがうまく行くので一旦十分大きな値に飛ばすことにする
    /// </summary>
    public static float LifeSpan
    {
        set; get;
    } = 10f;
    /// <summary>
    /// 石が置かれてからの経過時間（秒）
    /// </summary>
    private float stoneAge;
    /// <summary>
    /// 石が置かれてからの経過時間（秒）
    /// Updateで毎フレーム加算する
    /// </summary>
    public float StoneAge
    {
        private set
        {
            stoneAge = value;

            // 寿命に応じて石の大きさを変えようと思ったけど見づらいのでやめた
            //BlackStone.localScale = Vector3.one * (1f - stoneAge / LifeSpan);
            //WhiteStone.localScale = Vector3.one * (1f - stoneAge / LifeSpan);

            // 寿命が近づいたら点滅させる
            if (LifeSpan - 4f < stoneAge && currentBlinkStone == null)
            {
                currentBlinkStone = StartCoroutine(BlinkStone());
                //Debug.Log($"BoardCross.StoneAge: startcoroutine");
            }

            // 寿命を超えた場合は石を取り除く
            if (LifeSpan < stoneAge)
            {
                stoneAge = 0f;
                ActivateEffect();
                BoardStatus = IsOut ? Status.Out : Status.None;

                StopCoroutine(BlinkStone());
            }
        }
        get
        {
            return stoneAge;
        }
    }
    #endregion

    #region 目の相対関係等
    /// <summary>
    /// 盤面
    /// </summary>
    public static List<BoardCross> Field;
    /// <summary>
    /// 特定の座標のBoardCrossを取得する
    /// </summary>
    /// <param name="x">x座標 1～9</param>
    /// <param name="z">z座標 1～9</param>
    /// <returns></returns>
    public static BoardCross Find(int x, int z)
    {
        foreach (BoardCross c in Field)
        {
            if (c.X == x && c.Z == z)
            {
                return c;
            }
        }
        Debug.LogError($"BoardCross: ({x}, {z}) is not found! Field.count = {Field.Count}");
        return null;
    }
    /// <summary>
    /// 特定の座標のBoardCrossを取得する
    /// </summary>
    /// <param name="coordinate"></param>
    /// <returns></returns>
    public static BoardCross Find(Vector2 coordinate)
    {
        return Find((int)coordinate.x, (int)coordinate.y);
    }
    /// <summary>
    /// この目の上側
    /// </summary>
    public BoardCross Up
    {
        get
        {
            return Find(X, Z + 1);
        }
    }
    /// <summary>
    /// この目の下側
    /// </summary>
    public BoardCross Down
    {
        get
        {
            return Find(X, Z - 1);
        }
    }
    /// <summary>
    /// この目の右側
    /// </summary>
    public BoardCross Right
    {
        get
        {
            return Find(X + 1, Z);
        }
    }
    /// <summary>
    /// この目の左側
    /// </summary>
    public BoardCross Left
    {
        get
        {
            return Find(X - 1, Z);
        }
    }
    /// <summary>
    /// この目の右上側
    /// </summary>
    public BoardCross UpRight
    {
        get
        {
            return Find(X + 1, Z + 1);
        }
    }
    /// <summary>
    /// この目の左上側
    /// </summary>
    public BoardCross UpLeft
    {
        get
        {
            return Find(X - 1, Z + 1);
        }
    }
    /// <summary>
    /// この目の右下側
    /// </summary>
    public BoardCross DownRight
    {
        get
        {
            return Find(X + 1, Z - 1);
        }
    }
    /// <summary>
    /// この目の左下側
    /// </summary>
    public BoardCross DownLeft
    {
        get
        {
            return Find(X - 1, Z - 1);
        }
    }
    /// <summary>
    /// 隣接する縦横4つの目
    /// </summary>
    public BoardCross[] Neighborhood4
    {
        get
        {
            return new BoardCross[4] { Up, Down, Right, Left };
        }
    }
    /// <summary>
    /// 隣接する縦横斜め8つの目
    /// </summary>
    public BoardCross[] Neighborhood8
    {
        get
        {
            return new BoardCross[8] { Left, UpLeft, Up, UpRight, Right, DownRight, Down, DownLeft, };
        }
    }
    /// <summary>
    /// 隣接する8つの目の中で、味方の色の石が置いてある目の集合
    /// </summary>
    public List<BoardCross> AllyNeighborhood8
    {
        get
        {
            List<BoardCross> result = new List<BoardCross>();
            foreach (BoardCross board in Neighborhood8)
            {
                if (board.BoardStatus == BoardStatus)
                {
                    result.Add(board);
                }
            }
            return result;
        }
    }
    /// <summary>
    /// 特定の状態の目の個数を数える
    /// </summary>
    /// <param name="status">数えたい目の状態</param>
    /// <returns></returns>
    private static List<BoardCross> Count(Status status)
    {
        List<BoardCross> result = new List<BoardCross>();
        foreach (BoardCross board in Field)
        {
            if (board.BoardStatus == status)
            {
                result.Add(board);
            }
        }
        return result;
    }
    /// <summary>
    /// Fieldのうち、黒石がある場所
    /// </summary>
    public static List<BoardCross> BlackField
    {
        get
        {
            return Count(Status.Black);
        }
    }
    /// <summary>
    /// Fieldのうち、白石がある場所
    /// </summary>
    public static List<BoardCross> WhiteField
    {
        get
        {
            return Count(Status.White);
        }
    }
    /// <summary>
    /// Fieldのうち、何も置かれていない場所
    /// </summary>
    public static List<BoardCross> EmptyField
    {
        get
        {
            return Count(Status.None);
        }
    }
    /// <summary>
    /// Fieldの外側
    /// </summary>
    public static List<BoardCross> OutField
    {
        get
        {
            return Count(Status.Out);
        }
    }
    /// <summary>
    /// この目と連結関係にある目のリスト
    /// 隣接していて、かつ同じ状態（空、黒、白）の目を返す
    /// </summary>
    public List<BoardCross> ConnectedBoardCross
    {
        get
        {
            // 返り値
            List<BoardCross> result = new List<BoardCross>();

            // 無限ループ対策
            int count = 0;

            // 候補のキュー
            Queue<BoardCross> Candidates = new Queue<BoardCross>();
            Candidates.Enqueue(this);

            // オリジナルの状態
            Status originalStatus = BoardStatus;

            // 捜査フラグを解除する
            ClearAlreadyCheckedFlag();

            while(Candidates.Count > 0)
            {
                // 新しく候補を取り出す
                var candidate = Candidates.Dequeue();
                
                // 外側なら何もしない
                if (candidate.IsOut)
                {
                    continue;
                }

                // もう調べていたら何もしない
                if (candidate.isCheckedSieged)
                {
                    continue;
                }
                candidate.isCheckedSieged = true;

                // 元の色と同色でないなら何もしない
                if (candidate.BoardStatus != originalStatus)
                {
                    continue;
                }

                // もうリストに格納されているなら何もしない
                if (result.Contains(candidate))
                {
                    continue;
                }

                // リストに格納
                result.Add(candidate);

                // 候補に隣接する目を候補に追加
                foreach (BoardCross neighborhood in candidate.Neighborhood4)
                {
                    if (!neighborhood.IsOut)
                    {
                        Candidates.Enqueue(neighborhood);
                    }
                }

                // 無限ループ対策
                count++;
                if (count > 200)
                {
                    Debug.LogError("BoarCross.ConnectedBoardCross: Too many iteration!");
                    return result;
                }
            }

            return result;
        }
    }
    /// <summary>
    /// この石の呼吸点
    /// </summary>
    /// <param name="connected">連結した石のリスト</param>
    public static List<BoardCross> BreathPoint(List<BoardCross> connected)
    {
        // 石が置かれた目が渡されていなければ何もしない
        if (connected[0].BoardStatus == Status.None || connected[0].boardStatus == Status.Out)
        {
            return new List<BoardCross>();
        }

        List<BoardCross> result = new List<BoardCross>();
        foreach (BoardCross board in connected)
        {
            // 連結した石と隣接する目で、空状態の目を探す
            foreach (BoardCross neighborhood in board.Neighborhood4)
            {
                if (neighborhood.BoardStatus == Status.None)
                {
                    result.Add(neighborhood);
                }
            }
        }
        return result;
    }
    #endregion

    /// <summary>
    /// 囲い石の際にチェック済みかどうか調べるのに使う
    /// </summary>
    private bool isCheckedSieged = false;

    /// <summary>
    /// この目から伸びている線
    /// </summary>
    public List<ConnectingLine> ConnectingLines
    {
        get
        {
            return ConnectingLine.Find(this);
        }
    }

    private void Update()
    {
        // 石が置かれてからの時間を加算
        //if (BoardStatus == Status.Black || BoardStatus == Status.White)
        // プレイヤーの石なら寿命を設ける
        if (BoardStatus == GameController.Instance.playerColor)
        {
            StoneAge += Time.deltaTime;
        }
        else
        {
            StoneAge = 0f;
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void Initialize(int x, int z)
    {
        // 変数初期化
        X = x;
        Z = z;

        // 位置初期化
        CenterCoordinate = Vector2.zero;
        TranslateBoard(x, z);

        // 石をリセットする
        BoardStatus = IsOut ? Status.Out : Status.None;

        // 盤面Fieldがnullの場合に盤面を生成
        if (Field == null)
        {
            Field = new List<BoardCross>();
        }
        // 盤面に自身を追加
        if (!Field.Contains(this))
        {
            Field.Add(this);
        }

        // 見た目情報を取得
        //StoneVis = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));
        SetVisActive();
    }
    /// <summary>
    /// 石を全て取り除く
    /// </summary>
    public static void ClearStoneAll()
    {
        foreach (BoardCross board in Field)
        {
            board.ActivateEffect();
            board.BoardStatus = board.IsOut ? Status.Out : Status.None;
        }
    }
    /// <summary>
    /// 碁盤上の所定座標に移動する
    /// </summary>
    /// <param name="pos">(1, 9)×(1, 9)</param>
    private void TranslateBoard(Vector2 coordinate)
    {
        TranslateBoard((int)coordinate.x, (int)coordinate.y);
    }
    /// <summary>
    /// 碁盤上の所定座標に移動する
    /// </summary>
    /// <param name="x">x座標 1～9</param>
    /// <param name="z">z座標 1～9</param>
    private void TranslateBoard(int x, int z)
    {
        transform.position = new Vector3(x, 0f, z) + new Vector3(CenterCoordinate.x, 0f, CenterCoordinate.y) - new Vector3(5f, 0f, 5f);
    }
    /// <summary>
    /// 盤面のビジュアル（空、黒石、白石）を更新する
    /// </summary>
    /// <param name="nextStatus"></param>
    private void UpdateBoardVis(Status nextStatus)
    {
        // 石の色の更新
        BlackStone.gameObject.SetActive(nextStatus == Status.Black);
        WhiteStone.gameObject.SetActive(nextStatus == Status.White);

        // 石が置かれていないのに線があるなら消す
        if (nextStatus != Status.Black && nextStatus != Status.White)
        {
            foreach (ConnectingLine line in ConnectingLines)
            {
                line.IsVisible = false;
            }
        }

        // SpriteRendererはオンにする
        SetVisActive();

        // 点滅フラグを切る
        currentBlinkStone = null;
    }
    /// <summary>
    /// 調べる場所にある石が相手に囲まれているか調べる
    /// </summary>
    /// <param name="thisBoardCross">最初に調べる場所</param>
    /// <returns></returns>
    public bool IsSieged()
    {
        // チェック履歴をクリアする
        ClearAlreadyCheckedFlag();

        // 石が置かれていないなら何もしない
        if (BoardStatus != Status.Black && BoardStatus != Status.White)
        {
            return false;
        }

        // この石に連結した石を全て取り出す
        var connected = ConnectedBoardCross;

        // この連結した石の呼吸点を全て取り出す
        var breath = BreathPoint(connected);

        // 呼吸点の有無
        return breath.Count == 0;
    }
    /// <summary>
    /// 置いた石の上下左右に相手の囲われた石があるなら取る
    /// </summary>
    /// <param name="playerColor"></param>
    /// <returns>取った石があった目</returns>
    public List<BoardCross> RemoveStoneAll(Status playerColor)
    {
        List<BoardCross> result = new List<BoardCross>();
        result.AddRange(Up.RemoveStone(playerColor));
        result.AddRange(Down.RemoveStone(playerColor));
        result.AddRange(Right.RemoveStone(playerColor));
        result.AddRange(Left.RemoveStone(playerColor));

        return result;
    }
    /// <summary>
    /// この目の石が死んでいれば碁盤から取り除く
    /// </summary>
    /// <param name="playerColor">自分が置いた石の色</param>
    /// <returns></returns>
    public List<BoardCross> RemoveStone(Status playerColor)
    {
        // 外側なら何もしない
        if (IsOut)
        {
            return new List<BoardCross>();
        }

        // 置いた石と同じ色なら取らない
        if (BoardStatus == playerColor)
        {
            return new List<BoardCross>();
        }

        // 追加する前にリセット
        SiegingLineFactory.Instance.ResetSiegedBoardCross();

        // 囲まれてないなら何もしない
        if (!IsSieged())
        {
            return new List<BoardCross>();
        }

        // この石と連結された石を全て取得
        var result = ConnectedBoardCross;

        // 返り値になっている目の石を取り除く
        foreach (BoardCross board in result)
        {
            board.BoardStatus = Status.None;
            SiegingLineFactory.Instance.AddSiegedBoardCross(board);
        }

        // 線を生成する（石の色により出る線の色を場合分け）
        SiegingLineFactory.Instance.GenerateSiegingLineInstance(playerColor == GameController.Instance.playerColor);

        return result;
    }
    /// <summary>
    /// 自殺手かどうか調べる
    /// </summary>
    /// <param name="nextMove">次に置きたい手</param>
    /// <returns></returns>
    public bool IsSuicide(Status nextMove)
    {
        // 仮に石を置く
        BoardStatus = nextMove;

        // 仮に置いた石が相手に囲まれているならば自殺手の可能性あり
        if (IsSieged())
        {
            // その石を置いたことにより、隣の相手の石が取れるなら自殺手ではない
            // 隣は相手？
            foreach (BoardCross neighborhood in Neighborhood4)
            {
                if (neighborhood.BoardStatus == OpponentStatus)
                {
                    // 相手のが囲まれているなら自殺手ではない
                    if (neighborhood.IsSieged())
                    {
                        BoardStatus = Status.None;
                        return false;
                    }
                }
            }

            // 盤を元に戻す
            BoardStatus = Status.None;

            // 相手の石を取れないなら自殺手
            return true;
        }
        else
        {
            BoardStatus = Status.None;

            // 囲まれていないので自殺手ではない
            return false;
        }
    }
    /// <summary>
    /// 合法手かどうか調べる
    /// </summary>
    /// <param name="nextMove">次に置きたい石の色</param>
    /// <returns>合法手である</returns>
    public bool IsLegalMove(Status nextMove)
    {
        // 空点でないと置けない
        if (boardStatus != Status.None)
        {
            return false;
        }

        // 一手前にコウを取られていたら置けない
        // 履歴作るのちょっと面倒くさい
        // どうせ石を取った石も同時に無くなるならコウは存在しなくなるので省略

        // 自殺手なら置けない
        if (IsSuicide(nextMove))
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// 自身が相手の石に接している時、その相手の石が隣り合っていれば、それらを線で結ぶ
    /// </summary>
    public void ActivateOpponentLine()
    {
        // 隣り合う相手の石または自分自身の入ったリストを取得
        // 自分が空状態の場合は、隣り合う自分と相手の石を全て取得
        List<BoardCross> Siegings = SiegingBoardCross(ConnectedBoardCross);
        foreach (BoardCross sieging in Siegings)
        {
            // 相手の石でない（自分自身である）なら何もしない
            if (sieging.BoardStatus != OpponentStatus)
            {
                continue;
            }

            foreach (BoardCross neighborhood in sieging.Neighborhood8)
            {
                if (Siegings.Contains(neighborhood) && neighborhood.BoardStatus == OpponentStatus)
                {
                    ConnectingLine line = ConnectingLine.Find(sieging, neighborhood);
                    line.IsVisible = true;
                }
            }
        }
    }
    
    /// <summary>
    /// 囲い石の際のチェック用の変数をクリアする
    /// </summary>
    public static void ClearAlreadyCheckedFlag()
    {
        foreach (BoardCross board in Field)
        {
            board.isCheckedSieged = false;
        }
    }

    /// <summary>
    /// 引数の目を囲むように配置された石のリストを返す
    /// </summary>
    /// <param name="sieged"></param>
    /// <returns></returns>
    public static List<BoardCross> SiegingBoardCross(List<BoardCross> sieged)
    {
        var result = new List<BoardCross>();
        foreach (BoardCross board in sieged)
        {
            // 囲われた石の周りで捜査
            foreach (BoardCross neighborhood in board.Neighborhood4)
            {
                // もうリストに格納されているなら何もしない
                if (result.Contains(neighborhood))
                {
                    continue;
                }

                // 相手の石があったら追加
                // 囲われた石が既に取られていて空の状態になっている場合は、黒も白もカウントする
                if ((board.BoardStatus == Status.None && (neighborhood.BoardStatus == Status.Black || neighborhood.BoardStatus == Status.White)) ||
                    (board.BoardStatus == Status.Black && neighborhood.boardStatus == Status.White) ||
                    (board.BoardStatus == Status.White && neighborhood.boardStatus == Status.Black))
                {
                    result.Add(neighborhood);
                }

                // 角だったらboard自身を追加
                if (neighborhood.BoardStatus == Status.Out && !result.Contains(board))
                {
                    result.Add(board);
                }
            }
        }
        return result;
    }
    /// <summary>
    /// 輪を描くような石の列のリストが一筆書きになるようにリストをソートする
    /// </summary>
    /// <param name="list">輪を描くような石の列</param>
    public static List<BoardCross> SortOneStroke(List<BoardCross> list)
    {
        var result = new List<BoardCross>();

        // チェック用のフラグをクリア
        ClearAlreadyCheckedFlag();

        // 代表点を一つ取り出す
        list.Sort();
        // 左下
        BoardCross next = list[0];
        result.Add(next);

        // 何かの拍子に無限ループしちゃった時の対策
        int count = 0;

        // resutlの大きさがlistのサイズと同じになるまで続ける
        while (result.Count < list.Count)
        {
            for (int i = 0; i < next.Neighborhood8.Length; i++)
            {
                // まだ捜査しておらず、かつlistに含まれる隣の目について、リストへの追加を行う
                if (list.Contains(next.Neighborhood8[i]) && !next.Neighborhood8[i].isCheckedSieged)
                {
                    next.Neighborhood8[i].isCheckedSieged = true;

                    result.Add(next.Neighborhood8[i]);

                    // 次はその目を中心にして捜査する
                    next = next.Neighborhood8[i];

                    break;
                }
            }

            count++;

            // ある程度countが大きくなりすぎたらエラー吐いて強制終了
            if (count == 100)
            {
                Debug.LogError($"BoardCross.SortOneStroke: Too much iteration!");
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 石が消える時のエフェクトを出す
    /// </summary>
    public void ActivateEffect()
    {
        // 石が置かれていないなら何もしない
        if (BoardStatus != Status.Black && BoardStatus != Status.White)
        {
            return;
        }

        // パーティクルを指定する
        ParticleSystem particle;
        if (BoardStatus == Status.Black)
        {
            particle = blackParticle;
        }
        else
        {
            particle = whiteParticle;
        }

        // パーティクルを有効化する
        particle.Play();
    }

    /// <summary>
    /// 石の見た目を見せるかどうか
    /// </summary>
    /// <param name="isActive"></param>
    public void SetVisActive(bool isActive=true)
    {
        foreach (SpriteRenderer sprite in StoneVis)
        {
            sprite.enabled = isActive;
        }
    }

    /// <summary>
    /// 現在回っているコルーチン
    /// </summary>
    private Coroutine currentBlinkStone;
    /// <summary>
    /// 石を点滅させる
    /// 全部で4sec
    /// </summary>
    /// <returns></returns>
    public IEnumerator BlinkStone()
    {
        // 最初の2秒：1Hz
        SetVisActive(true);
        yield return new WaitForSeconds(0.5f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.5f);
        SetVisActive(true);
        yield return new WaitForSeconds(0.5f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.5f);

        // 次の1秒：2Hz
        SetVisActive(true);
        yield return new WaitForSeconds(0.25f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.25f);
        SetVisActive(true);
        yield return new WaitForSeconds(0.25f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.25f);

        // 最後の1秒：4Hz
        SetVisActive(true);
        yield return new WaitForSeconds(0.125f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.125f);

        SetVisActive(true);
        yield return new WaitForSeconds(0.125f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.125f);

        SetVisActive(true);
        yield return new WaitForSeconds(0.125f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.125f);

        SetVisActive(true);
        yield return new WaitForSeconds(0.125f);
        SetVisActive(false);
        yield return new WaitForSeconds(0.125f);
    }
}
