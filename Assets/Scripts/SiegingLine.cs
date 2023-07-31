using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 石を消す時に出る線のインスタンス
/// </summary>
public class SiegingLine : MonoBehaviour
{
    /// <summary>
    /// 広がって消えるアニメーション
    /// </summary>
    private Animator animator;
    /// <summary>
    /// 消される石が置かれた目たち
    /// </summary>
    private List<BoardCross> SiegedBoardCross;
    /// <summary>
    /// 紫の線
    /// </summary>
    private LineRenderer lineRenderer;
    private List<BoardCross> SiegingBoardCross;
    /// <summary>
    /// 線の頂点となる部分
    /// </summary>
    private List<Vector3> LineVertexPositions;

    /// <summary>
    /// 初期化して紫の線を生成する
    /// </summary>
    /// <param name="sieged">囲われた石がある目</param>
    /// <returns>siegedを囲っている石がある目</returns>
    public List<BoardCross> Initialize(List<BoardCross> sieged)
    {
        // コピー生成
        SiegedBoardCross = new List<BoardCross>(sieged);

        // コンポーネント取得
        animator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

        // 頂点を選択
        SiegingBoardCross = BoardCross.SiegingBoardCross(SiegedBoardCross);

        // ソートして一筆書きにする
        SiegingBoardCross = BoardCross.SortOneStroke(SiegingBoardCross);

        // positionだけを取り出す
        LineVertexPositions = new List<Vector3>();
        foreach (BoardCross board in SiegingBoardCross)
        {
            LineVertexPositions.Add(board.transform.position);
        }

        // 位置を死んだ石の中心に指定
        transform.position = Vector3.zero;
        foreach (Vector3 pos in LineVertexPositions)
        {
            transform.position += pos;
        }
        transform.position /= LineVertexPositions.Count;

        // 頂点位置をこのインスタンスの移動に応じて修正
        for (int i=0; i<LineVertexPositions.Count; i++)
        {
            LineVertexPositions[i] -= transform.position;
        }

        // LineRendererの具体的な点を設定
        lineRenderer.positionCount = LineVertexPositions.Count;
        lineRenderer.SetPositions(LineVertexPositions.ToArray());

        return SiegingBoardCross;
    }
    /// <summary>
    /// 大きく・細くなって消える
    /// </summary>
    public void FadeOut()
    {
        animator.SetTrigger("Fade Out");
    }
    /// <summary>
    /// 自壊する
    /// </summary>
    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
