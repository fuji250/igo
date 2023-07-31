using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 石を取る時に出現する紫の線を生成するクラス
/// </summary>
public class SiegingLineFactory : Singleton<SiegingLineFactory>
{
    /// <summary>
    /// プレイヤーが相手の石を取った時に生成するプレハブ
    /// </summary>
    public SiegingLine linePrefab;
    /// <summary>
    /// 相手がプレイヤーの石を取った時に生成するプレハブ
    /// </summary>
    public SiegingLine linePrefabOpponent;
    /// <summary>
    /// 囲われた目たち
    /// </summary>
    private List<BoardCross> SiegedBoardCross = new List<BoardCross>();

    #region 石を取った時の線
    /// <summary>
    /// インスタンスを生成する
    /// 事前に囲われている目をAddSiegedBoardCrossで全て追加しておくこと
    /// </summary>
    public void GenerateSiegingLineInstance(bool isPlayer = true)
    {
        // インスタンスを生成して囲われた目のリストを代入
        SiegingLine lineInstance = Instantiate(isPlayer ?linePrefab : linePrefabOpponent, transform);
        lineInstance.Initialize(SiegedBoardCross);
        lineInstance.FadeOut();

        // リスト削除
        ResetSiegedBoardCross();
    }
    /// <summary>
    /// 囲われている目を追加する
    /// </summary>
    /// <param name="sieged"></param>
    public void AddSiegedBoardCross(BoardCross sieged)
    {
        // 足す
        SiegedBoardCross.Add(sieged);
    }
    /// <summary>
    /// 全ての要素を削除する
    /// </summary>
    public void ResetSiegedBoardCross()
    {
        SiegedBoardCross.RemoveAll((board) => true);
    }
    #endregion
}
