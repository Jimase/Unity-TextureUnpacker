#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;

namespace TextureUnpacker
{
    public static class TextureExporter
    {
        public static IEnumerator ExportSprites(PlistFile plist, Texture2D texture, string exportPath, bool clipSprite)
        {
            string textureDir = Path.GetDirectoryName(exportPath);
            // 路径不存在时
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            int total = plist.frames.Count;
            for (int i = 0; i < total; i++)
            {
                PlistFrame frame = plist.frames[i];
                //EditorUtility.DisplayProgressBar("Exporting", frame.name, (float)i / total);

                Texture2D sprite = CreateSpriteTexture(texture, frame, clipSprite);
                SaveTextureToPNG(sprite, Path.Combine(exportPath, frame.name));
                yield return null;
            }

            //EditorUtility.ClearProgressBar();
            //AssetDatabase.Refresh();
        }

        private static Texture2D CreateSpriteTexture(Texture2D source, PlistFrame frame, bool clipSprite)
        {
            // 计算目标纹理尺寸
            int width = clipSprite ? frame.frame.width : frame.sourceSize.x;
            int height = clipSprite ? frame.frame.height : frame.sourceSize.y;

            // 源纹理边界验证
            if (frame.frame.x < 0 
                || frame.frame.y < 0
                || frame.frame.x + frame.frame.width > source.width
                || frame.frame.y + frame.frame.height > source.height)
            {
                Debug.LogError($"Frame {frame.name} is out of bounds in the source texture.");
                return null;
            }

            // 获取并处理像素数据
            Color[] pixels = frame.rotated ? GetRotatedPixels(source, frame, width, height) 
                : GetStraightPixels(source, frame);

            // 创建并返回最终纹理
            Texture2D sprite = new Texture2D(width, height, TextureFormat.RGBA32, false);
            sprite.SetPixels(pixels);
            sprite.Apply();
            return sprite;
        }

        private static Color[] GetRotatedPixels(Texture2D source, PlistFrame frame, int targetWidth, int targetHeight)
        {
            // 调整Y坐标偏移量
            int yOffset = frame.frame.width - frame.frame.height;
            int yPos = frame.frame.y - yOffset;

            // 获取旋转前的像素并执行旋转
            Color[] rawPixels = source.GetPixels(
                frame.frame.x,
                yPos,
                frame.frame.height,
                frame.frame.width
            );
    
            return RotatePixels(rawPixels, targetHeight, targetWidth);
        }

        private static Color[] GetStraightPixels(Texture2D source, PlistFrame frame)
        {
            return source.GetPixels(
                frame.frame.x,
                frame.frame.y,
                frame.frame.width,
                frame.frame.height
            );
        }


        private static Color[] RotatePixels(Color[] src, int srcWidth, int srcHeight)
        {
            Color[] dst = new Color[srcWidth * srcHeight];
            for (int x = 0; x < srcWidth; x++)
            {
                for (int y = 0; y < srcHeight; y++)
                {
                    int srcIndex = x + y * srcWidth;
                    int dstIndex = (srcHeight - y - 1) + x * srcHeight; // 修正索引计算
                    dst[dstIndex] = src[srcIndex];
                }
            }
            return dst;
        }

        private static void SaveTextureToPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
#endif