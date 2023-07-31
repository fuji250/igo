using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : Singleton<AudioController>
{
    /// <summary>
    /// パチンと打つ音
    /// </summary>
    public AudioClip soundPutStone;
    /// <summary>
    /// 石を取った時の音
    /// </summary>
    public AudioClip soundRemoveStone;
    /// <summary>
    /// 石を置けない時の音
    /// カーソルを動かしたときの音
    /// </summary>
    public AudioClip soundError;

    /// <summary>
    /// このシーンのオーディオソース
    /// </summary>
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }
    /// <summary>
    /// 音を出す（エイリアス）
    /// </summary>
    /// <param name="clip"></param>
    public void Play(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    /// <summary>
    /// 石を打つ音
    /// </summary>
    public void PlayPutStone()
    {
        Play(soundPutStone);
    }
    /// <summary>
    /// 石を取る音
    /// </summary>
    public void PlayRemoveStone()
    {
        Play(soundRemoveStone);
    }
    /// <summary>
    /// エラーが出た時の音
    /// </summary>
    public void PlayError()
    {
        Play(soundError);
    }
}