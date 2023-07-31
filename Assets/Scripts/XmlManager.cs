using System;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

public class XmlManager : Singleton<XmlManager>
{
    /// <summary>
    /// xmlのファイルパスから読み込む場合。
    /// </summary>
    public DemoXmlData LoadFromPath(string i_path)
    {
        try
        {
            using (var fileStream = new FileStream(i_path, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(DemoXmlData));
                var xmlData = (DemoXmlData)serializer.Deserialize(fileStream);
                if (xmlData != null)
                {
                    // クラスに変換後はご自由に。

                    return xmlData;
                }
                return null;
            }
        }
        catch (System.Exception i_exception)
        {
            Debug.LogError($"{i_exception}");
            return null;
        }
    }

}