using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    /// <summary>
    /// �c���̐e�I�u�W�F�N�g
    /// </summary>
    [SerializeField] Transform m_VerticalParent;

    /// <summary>
    /// �����̐e�I�u�W�F�N�g
    /// </summary>
    [SerializeField] Transform m_HorizontalParent;

    /// <summary>
    /// ��}�X������̑傫��
    /// </summary>
    private float m_BlockSize = 0.125f;

    /// <summary>
    /// �ő�}�X��
    /// </summary>
    private int m_MaxBlockNum = 16;

    /// <summary>
    /// Line��񊈐�����������Z�������肷��
    /// </summary>
    /// <param name="p_hLength">�Ֆʂ̉��̃}�X��</param>
    /// <param name="p_vLength">�Ֆʂ̏c�̃}�X��</param>
    public void AdjustLines(int p_hLength, int p_vLength)
    {
        //�����ɂ���
        for(int x = 0;x < m_MaxBlockNum; x++)
        {
            //���������g��Ȃ�Line��������
            if(x >= p_hLength)
            {
                //�񊈐�������
                m_VerticalParent.GetChild(x).gameObject.SetActive(false);
            }

            //�g��Line��������
            //�w�肳�ꂽ�}�X���ɍ��킹�Ē����𒲐�����
            m_HorizontalParent.GetChild(x).localScale = new Vector3(m_BlockSize * (p_hLength-1), 1, 1);
        }

        //�c���ɂ���
        for(int y = 0;y < m_MaxBlockNum;y++)
        {
            //���������g��Ȃ�Line��������
            if (y >= p_vLength)
            {
                //�񊈐�������
                m_HorizontalParent.GetChild(y).gameObject.SetActive(false);
            }

            //�g��Line��������
            //�w�肳�ꂽ�}�X���ɍ��킹�Ē����𒲐�����
            m_VerticalParent.GetChild(y).localScale = new Vector3(m_BlockSize * (p_vLength-1), 1, 1);
        }
    }
}
