using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// デモ用の石を打つ
/// </summary>
public class DemoController : Singleton<DemoController>
{
    private List<DemoNextMove> initialMoveRecord;
    /// <summary>
    /// 棋譜
    /// </summary>
    public Queue<DemoNextMove> MoveRecord;
    /// <summary>
    /// 次の手
    /// </summary>
    private DemoNextMove nextMove;
    /// <summary>
    /// 次の手で打つ石の色
    /// </summary>
    public BoardCross.Status NextColor
    {
        get
        {
            return nextMove.playerColor;
        }
    }
    /// <summary>
    /// 最後の手を打ってからの時間
    /// </summary>
    public float TimeFromLastMove
    {
        private set; get;
    } = 0f;
    /// <summary>
    /// 次の手を打つまでのクールダウン時間が経ったかどうか
    /// </summary>
    public bool IsCooledDown
    {
        get
        {
            return nextMove.timeToThisMove < TimeFromLastMove;
        }
    }

    [SerializeField]
    private string moveRecordPath = "";


    protected override void Awake()
    {
        base.Awake();

        InitializeMoveRecord();
    }

    private void Update()
    {
        TimeFromLastMove += Time.deltaTime;
    }

    /// <summary>
    /// 棋譜を初期化する
    /// </summary>
    public void InitializeMoveRecord()
    {
        // 初期化
        MoveRecord = new Queue<DemoNextMove>();

        var data = XmlManager.Instance.LoadFromPath(moveRecordPath);

        initialMoveRecord = data.List;
        Debug.Log($"DemoController.InitializeMoveRecord: MoveRecord.Count = {initialMoveRecord.Count}");
        // キュー生成
        foreach (DemoNextMove next in initialMoveRecord)
        {
            MoveRecord.Enqueue(next);
        }

        // 初期値にセットする
        nextMove = MoveRecord.Dequeue();

        TimeFromLastMove = 0f;
    }

    /// <summary>
    /// 次の一手を決める
    /// </summary>
    /// <returns></returns>
    public BoardCross Move()
    {
        // 時間リセット
        TimeFromLastMove = 0f;

        // キューから次の一手を出す
        nextMove = MoveRecord.Dequeue();

        // 返り値を見つける
        BoardCross result = BoardCross.Find(nextMove.x, nextMove.z);

        return result;
    }
}

/// <summary>
/// 次の手
/// デモ用
/// </summary>
public class DemoNextMove
{
    /// <summary>
    /// 次に打つ石の色
    /// </summary>
    [XmlElement("color")]
    public BoardCross.Status playerColor;
    /// <summary>
    /// この手で打つ目のx座標
    /// </summary>
    [XmlElement("x")]
    public int x;
    /// <summary>
    /// この手で打つ目のz座標
    /// </summary>
    [XmlElement("z")]
    public int z;
    /// <summary>
    /// この手を打つまでの時間
    /// </summary>
    [XmlElement("time")]
    public float timeToThisMove;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="playerColor"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="time"></param>
    public DemoNextMove(BoardCross.Status playerColor, int x, int z, float time)
    {
        this.playerColor = playerColor;
        this.x = x;
        this.z = z;
        this.timeToThisMove = time;
    }
    public DemoNextMove()
    {

    }
}
[XmlRoot("demoXmlData")]
public class DemoXmlData
{
    /// <summary>
    /// demoNextMoveを格納した配列
    /// </summary>
    [XmlArray("list")]
    [XmlArrayItem("nextMove")]
    public List<DemoNextMove> List
    {
        set; get;
    }
}
