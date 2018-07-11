// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;

/// <summary>
/// Editor utility class to help manage the different builds of the project.
/// </summary>
public class Builder
{
    private static readonly BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    private static readonly BuildOptions buildOptions = BuildOptions.None;

    [MenuItem("Window/Master Server Kit/Build master server", false, 100)]
    public static void BuildMasterServer()
    {
        PlayerSettings.virtualRealitySupported = false;
        var levels = new string[] { "Assets/MasterServerKit/Demo/Scenes/MasterServer/MasterServer.unity" };
        BuildPipeline.BuildPlayer(levels, "Builds/MasterServer.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build zone server", false, 100)]
    public static void BuildZoneServer()
    {
        PlayerSettings.virtualRealitySupported = false;
        var levels = new string[] { "Assets/MasterServerKit/Demo/Scenes/ZoneServer/ZoneServer.unity" };
        BuildPipeline.BuildPlayer(levels, "Builds/ZoneServer.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build game server", false, 100)]
	public static void BuildGameServer()
	{
		var levels = new string[] {
			"Assets/MasterServerKit/Demo/Scenes/GameServer/GameServer.unity",
			"Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_Game.unity"
		};
		BuildPipeline.BuildPlayer(levels, "Builds/GameServer.exe", buildTarget, buildOptions);
	}

    [MenuItem("Window/Master Server Kit/Build game client", false, 100)]
    public static void BuildGameClient()
    {
        var levels = new string[] {
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_Start.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_Login.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_Lobby.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_Game.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_AlertDialog.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_ChatDialog.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_GamePasswordDialog.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_LoadingDialog.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_LoginDialog.unity",
            "Assets/MasterServerKit/Demo/Scenes/GameClient/GameClient_RegisterDialog.unity"
        };
        BuildPipeline.BuildPlayer(levels, "Builds/GameClient.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build all", false, 50)]
    public static void BuildAll()
    {
        BuildMasterServer();
        BuildZoneServer();
        BuildGameServer();
        BuildGameClient();
    }
}