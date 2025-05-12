using UnityEngine;
using System.Collections.Generic;
using System;

namespace TextureUnpacker
{
    // Unity 兼容的替代 System.Drawing 类型
    [Serializable]
    public struct Size
    {
        public int width;
        public int height;
        public Size(int w, int h) { width = w; height = h; }
    }

    [Serializable]
    public struct Point
    {
        public int x;
        public int y;
        public Point(int x, int y) { this.x = x; this.y = y; }
    }

    [Serializable]
    public struct Rectangle
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public Rectangle(int x, int y, int w, int h) { this.x = x; this.y = y; width = w; height = h; }
    }

    [Serializable]
    public class PlistMetadata
    {
        public int format;
        public string realTextureFileName;
        public Size size;
        public string smartupdate;
        public string textureFileName;
    }

    [System.Serializable]
    public class PlistFile
    {
        public Vector2Int textureSize;
        public List<PlistFrame> frames = new List<PlistFrame>();
    }

    [System.Serializable]
    public class PlistFrame
    {
        public string name;
        public bool rotated;
        public RectInt frame;
        public Vector2Int sourceSize;
        public Vector2Int offset;
    }
}