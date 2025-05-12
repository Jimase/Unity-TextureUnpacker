#if UNITY_EDITOR
using System.Xml;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace TextureUnpacker
{
    public class PlistPraser
    {
        public PlistFile plistFile = new PlistFile();
        
        public void Load(string path)        
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode root = doc.DocumentElement.SelectSingleNode("dict");
            Dictionary<string, object> dict = ParseDict(root);
            ParsePlistData(dict);
        }
    
        private Dictionary<string, object> ParseDict(XmlNode node)
        {
            var dict = new Dictionary<string, object>();
            XmlNodeList children = node.ChildNodes;
            
            for (int i = 0; i < children.Count; i += 2)
            {
                string key = children[i].InnerText;
                XmlNode valueNode = children[i + 1];
                
                if (valueNode.Name == "dict")
                {
                    dict.Add(key, ParseDict(valueNode));
                }
                else if (valueNode.Name == "array")
                {
                    dict.Add(key, ParseArray(valueNode));
                }
                else
                {
                    dict.Add(key, ParseValue(valueNode));
                }
            }
            return dict;
        }
        

        private List<object> ParseArray(XmlNode node)
        {
            var list = new List<object>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "dict")
                {
                    list.Add(ParseDict(child));
                }
                else
                {
                    list.Add(ParseValue(child));
                }
            }
            return list;
        }

        private object ParseValue(XmlNode node)
        {
            switch (node.Name)
            {
                case "string":
                    return node.InnerText;
                case "integer":
                    return int.Parse(node.InnerText);
                case "real":
                    return float.Parse(node.InnerText);
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return node.InnerText;
            }
        }

    private void ParsePlistData(Dictionary<string, object> data)
    {
        // 设置默认的 textureSize
        plistFile.textureSize = new Vector2Int(1024, 1024);

        // 检查 frames 是否存在且为字典
        if (data.ContainsKey("frames") && data["frames"] is Dictionary<string, object> framesDict)
        {
            foreach (var kvp in framesDict)
            {
                if (kvp.Value is Dictionary<string, object> frameData)
                {
                    PlistFrame frame = new PlistFrame
                    {
                        name = kvp.Key
                    };

                    // 解析 rotated 字段
                    if (frameData.ContainsKey("rotated"))
                    {
                        string rotatedValue = frameData["rotated"].ToString().ToLower();
                        frame.rotated = rotatedValue == "true";
                    }
                    else
                    {
                        Debug.LogError($"Missing rotated field for frame: {kvp.Key}");
                        continue;
                    }

                    // 解析 frame 字段
                    if (frameData.ContainsKey("frame") && frameData["frame"] is string frameStr)
                    {
                        frame.frame = ParseRectInt(frameStr);
                    }
                    else
                    {
                        Debug.LogError($"Missing or invalid frame field for frame: {kvp.Key}");
                        continue;
                    }

                    // 解析 sourceSize 字段
                    if (frameData.ContainsKey("sourceSize") && frameData["sourceSize"] is string sourceSizeStr)
                    {
                        frame.sourceSize = ParseVector2Int(sourceSizeStr);
                    }
                    else
                    {
                        Debug.LogError($"Missing or invalid sourceSize field for frame: {kvp.Key}");
                        continue;
                    }

                    // 解析 offset 字段
                    if (frameData.ContainsKey("offset") && frameData["offset"] is string offsetStr)
                    {
                        frame.offset = ParseVector2Int(offsetStr);
                    }
                    else
                    {
                        Debug.LogError($"Missing or invalid offset field for frame: {kvp.Key}");
                        continue;
                    }

                    // 坐标系转换（TexturePacker Y轴原点在左上，Unity在左下）
                    frame.frame.y = plistFile.textureSize.y - frame.frame.y - frame.frame.height;
                    plistFile.frames.Add(frame);
                }
                else
                {
                    Debug.LogError($"Invalid frame data format for key: {kvp.Key}");
                }
            }
        }
        else
        {
            Debug.LogError("Frames not found or invalid format in plist file.");
        }
    }



        private RectInt ParseRectInt(string str)
        {
            List<int> values = StringToIntList(str);
            return new RectInt(values[0], values[1], values[2], values[3]);
        }

        private Vector2Int ParseVector2Int(string str)
        {
            List<int> values = StringToIntList(str);
            return new Vector2Int(values[0], values[1]);
        }

        private List<int> StringToIntList(string str)
        {
            string clean = str.Replace("{", "").Replace("}", "").Replace("(", "").Replace(")", "");
            string[] parts = clean.Split(',');
            List<int> list = new List<int>();
            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int num))
                    list.Add(num);
            }
            return list;
        }
    }
}
#endif