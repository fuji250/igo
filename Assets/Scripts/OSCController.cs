using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uOSC;


public class OSCController : MonoBehaviour
{
    // Start is called before the first frame update
    //public Camera cam;

    //ParticleSystem�^��ϐ�ps�Ő錾���܂��B
    //public GameObject ps;
    //GameObject�^�ŕϐ�obj��錾���܂��B
    //GameObject obj;
    //�}�E�X�ŃN���b�N���ꂽ�ʒu���i�[����܂��B
    //private Vector3 mousePosition;
    private Vector3 sensorPosition;
    public GameObject originObject; //オリジナルのオブジェクト
    public GameController gameController;

    void Start()
    {
        var server = GetComponent<uOscServer>();
        server.onDataReceived.AddListener(OnDataReceived);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDataReceived(Message message)
    {
        //        Debug.Log(message.ToString());

        if (message.address == "/pos")
        {
            /*
            if (float.Parse(message.values[0].ToString()) > 1.5 || float.Parse(message.values[0].ToString()) < -0.7
                || float.Parse(message.values[1].ToString()) > 2.6 || float.Parse(message.values[1].ToString()) < 0.2)
            {
                return;
            }
            */
            float X = float.Parse(message.values[0].ToString());
            float Z = float.Parse(message.values[1].ToString());
            //Debug.Log(X);

            float a, b, c, d, e, f, g, h;
            a = -6.19314351f;
            b = -1.16391593f;
            c = 0.16527548f;
            d =  -1.13293551f;
            e = 6.32312651f;
            f = -5.2667029f;
            g = -0.01740763f;
            h = -0.02746715f;

            float HomoX = a * X + b * Z + c;
            float HomoZ = d * X + e * Z + f;
            float HomoY = g * X + h * Z + 1;

            //Debug.Log(HomoY);


            X = HomoX / HomoY;
            Z = HomoZ / HomoY;


            //ここでunityの座標に変換されているのが理想(xz座標)

            Debug.Log(X + ", " + Z);
            //�}�E�X�J�[�\���̈ʒu���擾�B
            //sensorPosition = new Vector3(X, 0, Z);
           // Debug.Log(sensorPosition);

            //Instantiate(originObject,sensorPosition , Quaternion.identity);

            Vector3 vec3 = new Vector3(0, 8, 0);
            Vector3 dir = new Vector3(X, -8, Z);

            Ray ray = new Ray(vec3, dir);
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 1f, false);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit)) // Rayを投射
            {
                //Debug.Log(2525);
                // Boardレイヤーなら
                    BoardCross board = hit.collider.GetComponent<BoardCross>();
                    if (board != null)
                    {
                        // 合法手がどうか調べる 
                        if (board.IsLegalMove(GameController.Instance.playerColor))
                        {
                            // 石を置く
                            board.BoardStatus = GameController.Instance.playerColor;

                            // 石を打つ音
                            AudioController.Instance.PlayPutStone();

                            // 隣の石を取り除く
                            List<BoardCross> Prisoners = board.RemoveStoneAll(GameController.Instance.playerColor);

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
                            GameController.Instance.timeFromLastMove = 0f;
                        }
                        else
                        {
                            // エラー音
                            //AudioController.Instance.PlayError();
                        }
                    }
                /*if(hit.collider.CompareTag("Enemy")) // タグを比較
                {
                    Destroy(hit.collider.gameObject); // オブジェクトを破壊
                }*/
            }

            //Instantiate(ps, Camera.main.ScreenToWorldPoint(sensorPosition),
            //    Quaternion.identity);
            //instantiate(ps, sensorPosition, Quaternion.identity);

            //PosCheck(sensorPosition, X, Z);
            //gameController.CheckPosition(X, Z);
        }
        //var msg = message.address + ": ";

        //// arguments (object array)
        //foreach (var value in message.values)
        //{
        //    if (value is int) msg += (int)value;
        //    else if (value is float) msg += (float)value;
        //    else if (value is string) msg += (string)value;
        //    else if (value is bool) msg += (bool)value;
        //    else if (value is byte[]) msg += "byte[" + ((byte[])value).Length + "]";
        //}

        //Debug.Log(msg);
        
    }
/*
    public void PosCheck(Vector3 Pos, float X, float Z)
    {
        GameObject clickedGameObject = null;

        Vector3 vec3 = new Vector3(0, 8, 0);
        Vector3 dir = new Vector3(X, -8, Z);


        // �������΂�
        //Ray ray = Camera.main.ScreenPointToRay(Pos);
        Ray ray = new Ray(vec3, dir);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 1f, false);
        RaycastHit hit = new RaycastHit();

        // ���������ɓ���������
       if (Physics.Raycast(ray, out hit))
        {
            // �I�u�W�F�N�g���擾����
            clickedGameObject = hit.collider.gameObject;
            Debug.Log("switch clicked");
        }
        else
        {
            return;
        }

        // �X�C�b�`�������珈�����s���B
        if (clickedGameObject.layer == 0)
        {
            clickedGameObject.GetComponent<Image>().enabled = false;
            Debug.Log(clickedGameObject.layer);
        }
    }*/
}