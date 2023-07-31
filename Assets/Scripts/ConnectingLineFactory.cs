using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目と目とを繋げる線を生成する
/// </summary>
public class ConnectingLineFactory : Singleton<ConnectingLineFactory>
{
    /// <summary>
    /// 線のプレハブ
    /// </summary>
    [SerializeField]
    private ConnectingLine linePrefab;
    /// <summary>
    /// 線の親オブジェクト
    /// </summary>
    public Transform lineParent;

    /// <summary>
    /// 盤面全体に線を生成する
    /// </summary>
    public void GenerateConnectingLineInstance()
    {
        // リスト初期化
        ConnectingLine.Lines = new List<ConnectingLine>();

        // 外側でない目に対し、外側ではなく、かつまだ引いていない隣の目に向かって線を引く
        foreach (BoardCross board in BoardCross.Field)
        {
            // 外側なら何もしない
            if (board.IsOut)
            {
                continue;
            }
            foreach (BoardCross neighborhood in board.Neighborhood8)
            {
                // 外側なら何もしない
                if (neighborhood.IsOut)
                {
                    continue;
                }

                // もう線が引かれてるなら何もしない
                if (ConnectingLine.Find(board, neighborhood) != null)
                {
                    continue;
                }

                // インスタンス生成
                var instance = Instantiate(linePrefab, lineParent);
                instance.Initialize(board, neighborhood);
            }
        }
    }
}
