using UnityEditor;

public class BundleCreate
{
    [MenuItem("AssetBundle/Build", false, 1)]
    static void BuildBundles() {
        string path = EditorUtility.SaveFolderPanel("Save Bundle", "", "");
        if (path.Length != 0) {
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}
