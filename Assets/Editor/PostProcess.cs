#if UNITY_IOS
 
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
 
public static class iOSOnPostProcessBuild
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
    {
        if (target == BuildTarget.iOS)
        {
            var plistPath = Path.Combine(pathToBuildProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
 
            plist.root.SetString("UIUserInterfaceStyle", "Dark");
 
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
 
#endif