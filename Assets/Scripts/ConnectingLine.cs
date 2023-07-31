using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目と目とを繋げる線
/// </summary>
public class ConnectingLine : MonoBehaviour
{
    /// <summary>
    /// 盤面全体の線
    /// </summary>
    public static List<ConnectingLine> Lines = new List<ConnectingLine>();
    /// <summary>
    /// 線
    /// </summary>
    private LineRenderer lineRenderer;
    /// <summary>
    /// 両端の目
    /// </summary>
    private BoardCross[] Ends;
    /// <summary>
    /// 線を見えるようにするか
    /// </summary>
    private bool isVisible = false;
    /// <summary>
    /// 線を見えるようにするか
    /// </summary>
    public bool IsVisible
    {
        set
        {
            isVisible = value;
            lineRenderer.enabled = value;
        }
        get
        {
            return isVisible;
        }
    }
    /// <summary>
    /// 指定した両端の目にある線を取り出す
    /// </summary>
    /// <param name="endA"></param>
    /// <param name="endB"></param>
    /// <returns></returns>
    public static ConnectingLine Find(BoardCross endA, BoardCross endB)
    {
        // 両端が等しい線は存在しない
        if (endA == endB)
        {
            return null;
        }

        foreach (ConnectingLine line in Lines)
        {
            if (line.Ends[0] == endA || line.Ends[1] == endA)
            {
                if (line.Ends[0] == endB || line.Ends[1] == endB)
                {
                    return line;
                }
            }
        }

        // 探してもなかった
        return null;
    }
    /// <summary>
    /// 指定した目に接続された線を全て取り出す
    /// </summary>
    /// <param name="endA"></param>
    /// <returns></returns>
    public static List<ConnectingLine> Find(BoardCross endA)
    {
        List<ConnectingLine> result = new List<ConnectingLine>();
        foreach (ConnectingLine line in Lines)
        {
            if (line.Ends[0] == endA || line.Ends[1] == endA)
            {
                result.Add(line);
            }
        }
        return result;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="endA">一方の端</param>
    /// <param name="endB">もう一方の端</param>
    public void Initialize(BoardCross endA, BoardCross endB)
    {
        lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

        // endA < endB となるようソート
        if (endA.CompareTo(endB) > 0)
        {
            BoardCross tmp = endA;
            endA = endB;
            endB = tmp;
        }

        // 配列に格納
        Ends = new BoardCross[2] { endA, endB };

        // 目の中央になるように移動
        transform.position = (endA.transform.position + endB.transform.position) / 2f;

        // LineRendererの両端の座標を設定
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { endA.transform.position - transform.position, endB.transform.position - transform.position });

        // 見えないようにする
        IsVisible = false;

        // 自身をリストに格納
        if (!Lines.Contains(this))
        {
            Lines.Add(this);
        }
    }
}
