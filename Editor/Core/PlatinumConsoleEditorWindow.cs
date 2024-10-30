using UnityEditor;
using UnityEngine.UIElements;

namespace CobaPlatinum.DebugTools.PlatinumConsole
{
    public class PlatinumConsoleEditorWindow : EditorWindow
    {
        [MenuItem("Coba Platinum/Debug Tools/Platinum Console")]
        public static void OpenEditorWindow()
        {
            PlatinumConsoleEditorWindow window = GetWindow<PlatinumConsoleEditorWindow>();
            window.titleContent = new UnityEngine.GUIContent("Platinum Console");
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.cobaplatinum.devtools/Resources/UI Documents/PlatinumConsoleEditorWindow.uxml");

            VisualElement tree = visualTree.Instantiate();
            tree.style.height = Length.Percent(100);
            root.Add(tree);
        }
    }
}
