using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相手（COM）の制御を行う
/// </summary>
public class OpponentController : Singleton<OpponentController>
{
    /// <summary>
    /// 次の手を打つ時の考え方
    /// </summary>
    public enum Protocol
    {
        Random,     // ランダム
        RandomPlus, // ランダムに毛が生えたやつ
    }
    /// <summary>
    /// 相手（COM）の考え方
    /// </summary>
    public Protocol OpponentProtocol;

    /// <summary>
    /// 打ってから次の手を打つまでの平均時間
    /// </summary>
    public float SpanAverage
    {
        set; get;
    } = 4f;
    /// <summary>
    /// SpanAverageの初期値
    /// </summary>
    private float firstSpanAverage;
    /// <summary>
    /// 打ってから次の手を打つまでの時間の分散
    /// </summary>
    public float SpanVariance
    {
        set; get;
    } = 1f;
    /// <summary>
    /// 打ってから次の手を打つまでの時間
    /// ボックス＝ミュラー法によって生成される正規分布を返す
    /// https://imagingsolution.net/program/csharp/normal-random/
    /// </summary>
    public float SpanGauss
    {
        get
        {
            float x = Random.Range(0f, 1f);
            float y = Random.Range(0f, 1f);
            return Mathf.Sqrt(SpanVariance) * Mathf.Sqrt(-2f * Mathf.Log(x)) * Mathf.Cos(2f * Mathf.PI * y) + SpanAverage;
        }
    }
    /// <summary>
    /// 最後の手を打ってからの時間
    /// </summary>
    public float TimeFromLastMove
    {
        private set; get;
    }
    /// <summary>
    /// 次の手が打つまでのクールダウン時間が経ったかどうか
    /// </summary>
    public bool IsCooledDown
    {
        get
        {
            return SpanGauss < TimeFromLastMove;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 初期化
        firstSpanAverage = SpanAverage;
    }
    private void Update()
    {
        TimeFromLastMove += Time.deltaTime;
    }

    /// <summary>
    /// 次の一手を決める
    /// </summary>
    /// <param name="field">場</param>
    /// <returns>打つ場所の目</returns>
    public BoardCross Move(List<BoardCross> field)
    {
        // 時間リセット
        TimeFromLastMove = 0f;

        // プロトコルに沿って次の一手を決める
        switch (OpponentProtocol)
        {
            case Protocol.Random:
            default:
                return MoveRandom(field);
            
        }
    }
    /// <summary>
    /// ランダムに次の一手を決める
    /// </summary>
    /// <param name="field">場</param>
    /// <returns>打つ場所の目</returns>
    private BoardCross MoveRandom(List<BoardCross> field)
    {
        // ランダムに開いている目を取り出す
        int rand = Random.Range(0, BoardCross.EmptyField.Count);
        BoardCross candidate = BoardCross.EmptyField[rand];

        return candidate;
    }
    /// <summary>
    /// ランダムに毛が生えたやつ
    /// 勝手に死なないようにする
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private BoardCross MoveRandomPlus(List<BoardCross> field)
    {
        // 無限ループ対策
        int count = 0;

        BoardCross result = MoveRandom(field);

        // 石の場所が決定されるまで実行
        while (true)
        {
            result = MoveRandom(field);

            foreach (BoardCross neighborhood in result.Neighborhood4)
            {
                // 自分自身の色でない石がある場合はOK
                if (neighborhood.BoardStatus != GameController.Instance.opponentColor || neighborhood.BoardStatus != BoardCross.Status.Out)
                {
                    return result;
                }
            }

            // 無限ループ対策
            count++;
            if (count > 200)
            {
                Debug.LogError($"OpponentController.MoveRandomPlus: Too many iteration!");
            }
            return result;
        }
    }
    /// <summary>
    /// SpanAverageをリセットする
    /// </summary>
    public void ResetSpanAverage()
    {
        SpanAverage = firstSpanAverage;
        TimeFromLastMove = 0f;
    }
}
