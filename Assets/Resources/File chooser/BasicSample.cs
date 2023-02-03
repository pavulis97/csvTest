using System.Collections;
using UnityEngine;
using SFB;

public class BasicSample : MonoBehaviour {
    private string _path;

    void OnGUI() {
        var guiScale = new Vector3(Screen.width / 800.0f, Screen.height / 600.0f, 1.0f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, guiScale);

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();

        // Open File Samples

        if (GUILayout.Button("Open Video File")) {
            
            saveVideoFilePath(StandaloneFileBrowser.OpenFilePanel("Open Video File", "", "", false));

            //filePath.pathOfFile = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        }
        GUILayout.Space(5);

        if (GUILayout.Button("Open CSV File")) {
            
            saveCSVfilePath(StandaloneFileBrowser.OpenFilePanel("Open CSV File", "", "", false));

            //filePath.pathOfFile = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        }


        GUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.Label(_path);
        GUILayout.EndHorizontal();
    }

    public void saveVideoFilePath(string[] paths) {
        if (paths.Length == 0) {
            return;
        }

        _path = "";
        foreach (var p in paths) {
            _path += p + "\n";
            filePath.pathOfVideoFile = p;
        }
    }

    public void saveCSVfilePath(string[] paths) {
        if (paths.Length == 0) {
            return;
        }

        _path = "";
        foreach (var p in paths) {
            _path += p + "\n";
            filePath.pathOfCSVFile = p;
        }
    }    

    public void WriteResult(string path) {
        _path = path;
    }
}

