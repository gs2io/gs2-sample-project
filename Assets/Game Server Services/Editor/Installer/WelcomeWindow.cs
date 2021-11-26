/*
 * Copyright 2016 Game Server Services, Inc. or its affiliates. All Rights
 * Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gs2.Installer.LitJson;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Gs2.Installer
{
    enum Status
    {
        Present,
        AddPackage,
        Installing,
        InstallComplete,
    }

    public class WelcomeWindow : EditorWindow
    {
        private Status _status = Status.Present;
        
        [MenuItem ("Window/Game Server Services/インストーラー")]
        public static void Open ()
        {
            GetWindowWithRect<WelcomeWindow>(new Rect(0, 0, 700, 250), true, "GS2-Installer");
        }

        private bool IsImportedRegistry()
        {
            using(var stream = new StreamReader(@"Packages/manifest.json"))
            {
                var json = JsonMapper.ToObject(stream.ReadToEnd());
                if (json.Keys.Contains("scopedRegistries"))
                {
                    if (json["scopedRegistries"].IsArray)
                    {
                        for (var i = 0; i < json["scopedRegistries"].Count; i++)
                        {
                            var repository = json["scopedRegistries"][i];
                            for (var j = 0; j < repository["scopes"].Count; j++)
                            {
                                if (repository["scopes"][j].ToString() == "io.gs2")
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        
        async void OnGUI() {
            GUILayout.Label("Game Server Services SDK for Unity のインストールを開始します。");
            GUILayout.Label("");
            GUILayout.Label("インストールには Unity Package Manager を利用します。");
            GUILayout.Label("Unity Package Manager は Unity 2018 以降で追加された機能の為、それ以前のバージョンでは使用できません。");
            GUILayout.Label("");

            switch (_status)
            {
                case Status.Present:
                    if (GUILayout.Button("インストール"))
                    {
                        Repaint();

                        string newManifestJson = null;
                        if (!IsImportedRegistry())
                        {
                            using (var stream = new StreamReader(@"Packages/manifest.json"))
                            {
                                var json = JsonMapper.ToObject(stream.ReadToEnd());
                                if (!json.Keys.Contains("scopedRegistries"))
                                {
                                    json["scopedRegistries"] = JsonMapper.ToObject("[]");
                                }

                                json["scopedRegistries"].Add(JsonMapper.ToObject(
                                    "{" +
                                    "\"name\": \"Game Server Services CloudWeave\", " +
                                    "\"url\": \"https://upm.gs2.io/npm/\", " +
                                    "\"scopes\": [\"io.gs2\"]" +
                                    "}"
                                ));

                                newManifestJson = json.ToJson();
                            }
                        }

                        if (newManifestJson != null)
                        {
                            using (var wstream = new StreamWriter(@"Packages/manifest.json"))
                            {
                                wstream.Write(newManifestJson);
                            }
                        }
                        
                        _status = Status.AddPackage;
                    }
                    break;
                case Status.AddPackage:
                {
                    GUILayout.Button("インストール中… しばらくお待ちください。");
                    var request = Client.Search("io.gs2.unity.sdk");
                    while (request.Status == StatusCode.InProgress)
                    {
                        await Task.Delay(1000);
                    }
                    
                    _status = Status.Installing;
                    break;
                }
                case Status.Installing:
                {
                    GUILayout.Button("インストール中… しばらくお待ちください。");

                    {
                        var request = Client.Add("io.gs2.csharp.sdk");
                        while (request.Status == StatusCode.InProgress)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    {
                        var request = Client.Add("io.gs2.unity.sdk");
                        while (request.Status == StatusCode.InProgress)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    {
                        var request = Client.List();
                        while (request.Status == StatusCode.InProgress)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    {
                        var request = Client.List();
                        while (request.Status == StatusCode.InProgress)
                        {
                            await Task.Delay(1000);
                        }

                        if (request.Result.Count(item => item.name == "io.gs2.csharp.sdk") > 0 && 
                            request.Result.Count(item => item.name == "io.gs2.unity.sdk") > 0)
                        {
                            _status = Status.InstallComplete;
                        }
                    }
                    break;
                }
                case Status.InstallComplete:
                    GUILayout.Label("インストールが完了しました。");
                    GUILayout.Label("");
                    if (GUILayout.Button("閉じる"))
                    {
                        Close();
                    }
                    break;
            }

            Repaint();
        }
    }
}