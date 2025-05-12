#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
namespace TextureUnpacker
{
    public class TextureUnpackerWindow : EditorWindow
    {
        // 配置路径和纹理路径
        private string configPath = "D:/Cache/test_apk/assets/Animations/binglongzidan.plist";
        private string texturePath = "D:/Cache/test_apk/assets/Animations/binglongzidan.png";
        private string exportPath = "D:/Cache/test_apk/assets/Animations/export";
        
        // 单选按钮状态
        private enum FileType { Plist, Atlas ,Others }
        private FileType fileType = FileType.Plist;
        
        // 复选框状态
        private bool showFrame = true;
        private bool clipSprite = false;
        
        // 预览纹理
        private Texture2D previewTexture;
        private Vector2 scrollPosition;
        
        private PlistPraser plistPraser = new PlistPraser();
        private Texture2D sourceTexture;
        
        
        // 初始化窗口
        [MenuItem("Tools/Texture Unpacker")]
        public static void ShowWindow()
        {
            GetWindow<TextureUnpackerWindow>("Texture Unpacker");
        }

        // GUI 绘制
        private void OnGUI()
        {
            DrawFileTypeGroup();
            DrawPathFields();
            DrawOptions();
            DrawPreview();
            DrawButtons();
        }

        // 文件类型单选组
        private void DrawFileTypeGroup()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("File Type", EditorStyles.boldLabel);
            fileType = (FileType)GUILayout.Toolbar((int)fileType, new[] { "Plist", "Atlas","Others" });
            EditorGUILayout.EndVertical();
        }

        // 路径输入字段
        private void DrawPathFields()
        {
            // 配置文件路径
            EditorGUILayout.BeginHorizontal();
            configPath = EditorGUILayout.TextField("Config Path", configPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Select Config", "", "plist,atlas");
                if (!string.IsNullOrEmpty(path)) configPath = path;
            }
            EditorGUILayout.EndHorizontal();

            // 纹理路径
            EditorGUILayout.BeginHorizontal();
            texturePath = EditorGUILayout.TextField("Texture Path", texturePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFilePanel("Select Texture", "", "png");
                if (!string.IsNullOrEmpty(path)) texturePath = path;
            }
            EditorGUILayout.EndHorizontal();
            
            // 期待导出的路径
            EditorGUILayout.BeginHorizontal();
            exportPath = EditorGUILayout.TextField("Export Path", exportPath);
            EditorGUILayout.EndHorizontal();
        }

        // 选项复选框
        private void DrawOptions()
        {
            showFrame = EditorGUILayout.Toggle("Show Frame", showFrame);
            clipSprite = EditorGUILayout.Toggle("Clip Sprite", clipSprite);
        }

        // 预览区域
        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            UnityEngine.Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(300), GUILayout.ExpandWidth(true));

            if (previewTexture != null)
            {
                float aspectRatio = (float)previewTexture.width / previewTexture.height;
                float previewWidth = Mathf.Min(previewRect.width, previewRect.height * aspectRatio);
                float previewHeight = previewWidth / aspectRatio;
                Rect textureRect = new Rect(
                    previewRect.x + (previewRect.width - previewWidth) * 0.5f,
                    previewRect.y + (previewRect.height - previewHeight) * 0.5f,
                    previewWidth,
                    previewHeight
                );
                GUI.DrawTexture(textureRect, previewTexture, ScaleMode.ScaleToFit);

                // 绘制帧的分割线
                if (showFrame && plistPraser.plistFile != null)
                {
                    foreach (var frame in plistPraser.plistFile.frames)
                    {
                        // 计算帧的位置和尺寸
                        float frameX = frame.frame.x * (textureRect.width / previewTexture.width);
                        float frameY = (previewTexture.height - frame.frame.y - frame.frame.height) * (textureRect.height / previewTexture.height);
                        float frameWidth = frame.frame.width * (textureRect.width / previewTexture.width);
                        float frameHeight = frame.frame.height * (textureRect.height / previewTexture.height);

                        // 如果帧是旋转的，交换宽度和高度
                        if (frame.rotated)
                        {
                            (frameWidth, frameHeight) = (frameHeight, frameWidth);
                        }

                        // 计算帧的矩形区域
                        Rect frameRect = new Rect(
                            textureRect.x + frameX,
                            textureRect.y + frameY,
                            frameWidth,
                            frameHeight
                        );

                        // 绘制帧的分割线
                        Handles.DrawSolidRectangleWithOutline(frameRect, Color.clear, Color.red);
                    }
                }
            }
            else
            {
                EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f));
                EditorGUI.LabelField(previewRect, "No Texture Loaded", EditorStyles.centeredGreyMiniLabel);
            }
        }



        // 操作按钮
        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open"))
            {
                LoadTexture();
                ParseConfig();
            }
            if (GUILayout.Button("Export"))
            {
                ExportTextures();
                OnExportClicked();
            }
            EditorGUILayout.EndHorizontal();
        }

        // 加载纹理、
        private void LoadTexture()
        {
            if (!File.Exists(texturePath)) return;
            byte[] data = File.ReadAllBytes(texturePath);
            sourceTexture = new Texture2D(2, 2); // 初始化sourceTexture
            sourceTexture.LoadImage(data);
    
            previewTexture = sourceTexture; // 预览和导出使用同一个纹理
            Repaint();
        }

        // 导出逻辑（需根据实际需求实现）
        private void ExportTextures()
        {
            // TODO: 实现具体的导出逻辑
            // Debug.Log("Exporting textures...");
        }

        // 处理拖拽事件
        private void OnDragUpdated()
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        private void OnDragPerform()
        {
            foreach (string path in DragAndDrop.paths)
            {
                if (path.EndsWith(".plist") || path.EndsWith(".atlas"))
                {
                    configPath = path;
                }
                else if (path.EndsWith(".png"))
                {
                    texturePath = path;
                }
            }
            Repaint();
        }
        
        private void ParseConfig()
        {
            if (fileType == FileType.Plist)
            {
                plistPraser.Load(configPath); // 假设ParsePlist是解析方法
            }
            else
            {
                // todo
                // 处理Atlas文件解析
            }
        }
        
        private void OnExportClicked()
        {
            if (string.IsNullOrEmpty(exportPath)) return;

            if (plistPraser.plistFile == null || sourceTexture == null)
            {
                EditorUtility.DisplayDialog("Error", "请先加载配置文件和纹理！", "OK");
                return;
            }

            EditorCoroutineUtility.StartCoroutine(
                TextureExporter.ExportSprites(
                    plistPraser.plistFile,
                    sourceTexture,
                    exportPath,
                    clipSprite
                ), this
            );
            // 启动协程
            // EditorCoroutine.Start(TextureExporter.ExportSprites(
            //     plistPraser.plistFile,
            //     sourceTexture,
            //     exportPath,
            //     clipSprite
            // ));
        }
    }
    
    // public static class EditorCoroutine
    // {
    //     private static IEnumerator _coroutine;
    //
    //     public static void Start(IEnumerator coroutine)
    //     {
    //         _coroutine = coroutine;
    //         EditorApplication.update += Update;
    //     }
    //
    //     private static void Update()
    //     {
    //         if (_coroutine == null || !_coroutine.MoveNext())
    //         {
    //             EditorApplication.update -= Update;
    //         }
    //     }
    // }
}
#endif