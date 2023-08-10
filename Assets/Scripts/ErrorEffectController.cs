using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorEffectController : MonoBehaviour
{
    public float fadeSpeed = 0.001f;        // 透明度が変わるスピード
    float alfa;   // Materialの色
    
    public bool isFadeOut = false;  // フェードアウト処理の開始、完了を管理
    public bool isFadeIn = true;   // フェードイン処理の開始、完了を管理
    
     private Renderer fadeMaterial;        // Materialにアクセスする容器

     private Renderer child;
    
    // Start is called before the first frame update
    void Start()
    {
        fadeMaterial = GetComponent<Renderer>();
        alfa = 0;
        fadeMaterial.material.color = new Color(fadeMaterial.material.color.r, fadeMaterial.material.color.g, fadeMaterial.material.color.b, alfa);

        child = transform.GetChild(0).gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isFadeIn)
        {
            alfa += fadeSpeed;         // 不透明度を下げる
            SetAlpha ();               // 変更した透明度を反映する

            if (alfa >= 0.5f)
            {
                isFadeIn = false;
                isFadeOut = true;
            }
        }

        if (isFadeOut)
        {
            alfa -= fadeSpeed;         // 不透明度を下げる
            SetAlpha ();

            if (alfa <= 0)
            {
                isFadeOut = false;
                
                Destroy(gameObject);
            }
        }
    }
    
    void StartFadeOut()
    {
        alfa -= fadeSpeed;         // 不透明度を下げる
        SetAlpha ();               // 変更した透明度を反映する
        if(alfa <= 0)              // 完全に透明になったら処理を抜ける
        {             
            isFadeOut = false;     // boolのチェックが外れる
        }
    }
    void StartFadeIn(){
        alfa += fadeSpeed;          // 不透明度を徐々に上げる
        SetAlpha ();                // 変更した不透明度を反映する
        if(alfa >= 1)               // 完全に不透明になったら処理を抜ける
        {                    
            isFadeIn = false;       // boolのチェックが外れる
            StartFadeOut();
        }
    }
    void SetAlpha()
    {
        fadeMaterial.material.color = new Color(fadeMaterial.material.color.r, fadeMaterial.material.color.g, fadeMaterial.material.color.b, alfa);
        child.material.color = new Color(fadeMaterial.material.color.r, fadeMaterial.material.color.g, fadeMaterial.material.color.b, alfa);
        // 変更した不透明度を含むMaterialのカラーを反映する
    }
}
