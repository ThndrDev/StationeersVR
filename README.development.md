# Setting up a development environment

## Software you will need
* Stationeers (obviously) with the VR Mod Installed. (It's recommend making a copy of the game folder to be used for development)
* [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
   - Make sure to choose Universal Platform and Unity Game Development at the install, in the workload options.
* [.NET 6 SDK](https://dotnet.microsoft.com/pt-br/download)
* [Unity](https://unity3d.com/get-unity/download) ([Details](#unity-install)) 
* [BepInExPack Stationeers](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). Probably already installed if you installed the mod.

## Setting up the environment
### Prerequisites
1. Make sure all the software above is installed.


### Checkout and configure the source
1. Open Visual Studio, and choose `Clone a Repository`, then enter this repo's checkout URL, e.g.
   https://github.com/ThndrDev/StationeersVR
2. Update the CommonDir to point to your Stationeers install:
    1. In the Visual Studio Solution Explorer, browse to `StationeersVR\StationeersVR`.
    2. Right-click `StationeersVR.csproj`, and Open With -> Source Code (Text) Editor. 
    3. Go to Edit -> Search and overwrite to update the reference paths to your Steam library folder containing Stationeers.:
    ``` From:
     G:\SteamLibrary\steamapps\common\StationeersVR\
    ```
    ``` To:
     <your stationeers folder location>
    ```   
    5. Save the file and close Visual Studio.

2. Publicitize the needed DLLs for the project.
   1. You need to use an [Assembly Publicizer](https://github.com/CabbageCrow/AssemblyPublicizer) for this. Download and unpack it.
   2. Go to the Stationeers Folder, drag the following files and drop into AssemblyPublicizer.exe
      1. Assembly-CSharp.dll
      2. SteamVR.dll
      3. Unity.XR.Management.dll
   3. The software will create a folder inside "rocketstation_Data\Managed" called "publicized_assemblies" with the 3 publicized DLLs.
   4. Rename the folder "publicized_assemblies" to "lib".
   4. Cut the "lib" folder and paste it inside the Mod project folder in "\StationeersVR\StationeersVRMod\". Should be in the folder you've checked the source out above.

3. Open Visual Studio and in the Solution Explorer, go to Dependencies -> Assemblies and make sure you don't have any broken dependencies.

### Build the Unity assets
1. In Unity Hub, go to Projects -> Add. 
2. Navigate to wherever you checked the source out above, and choose the `Unity\StationeersVR` folder.
4. Make sure to use the Unity version 2021.2.13f1. 
3. Click the newly added project to open it in Unity.
4. Go to File -> Build Settings, then click Build.
5. Navigate to the `\StationeersVR\Unity\StationeersVR\` folder inside your mod checkout.
6. Create a new subdirectory there named `build` and navigate into it.
7. Select that folder for the build. The Unity project's build output should appear in `\StationeersVR\Unity\StationeersVR\build`.
8. Close Unity.

## Build the mod
1. Open Visual Studio, and choose "Open a project or solution"
2. Navigate to the the mod source folder, then `\StationeersVR\StationeersVRMod\StationeersVR.sln`.
3. Make sure the release settings in the toolbar show: "Debug" and "Any CPU".
4. Click Build -> Build Solution.
    * The mod will be built and installed to your Stationeers directory. Check for the presence of `BepInEx\plugins\StationeersVR.dll`

## Enable Debug mode in Stationeers (optional but helpful)
* You'll have worse performance if you play with debug mode enabled, so make sure to use a separated copy of the game folder for this.
1. Copy the needed unity files to activate debug:
   1. Open Unity Hub. 
   2. Create a new 3D empty project using the same Unity version from Stationeers (2021.2.13f1)
   3. In the project, go to File -> Build Settings.
   4. Check the "Development Build" option and Build the project.
   5. Go to the directory where you've build the project, copy and overwrite the same files in the same directories in the game folder:
      1. MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll
      2. UnityPlayer.dll
      3. Copy the player.exe file, rename it to rocketstation.exe and overwrite the file in the game folder
   6. Open the file rocketstation_Data\boot.config with a text editor and add these two lines:
      ```player-connection-debug=1
         player-connection-project-name=Stationeers
      ```
   7. Close Unity, 
2. Enablle debug in Visual Studio:
   1. Open Visual Studio and open the project.
   2. In the Visual Studio interface, at the upper middle of the screeen, you'll see a play button with "Stationeers-Debug" written and a dropdown, click in the dropdown and choose "StationeersVR Debug Properties".
   3. In the "Starting profiles" Window, make sure you're editing "Stationeers-Debug" and then:
      1. Update the EXE path to point to the modified rocketstation.exe file.
      2. Check "Enable debug of native code".
      3. Close the Starting profiles window.
   4. In the "Visual Studio Solution Explorer", right click StationeersVR and choose Properties.
   5. Go to the "Create" tab, then scroll down to "Depuration Symbols" and make sure it's set to "PDB File, portable between platforms".
   6. Close the StationeersVR properties tab.
3. Test it:
   1. Add a breakpoint somewhere in the code that you want to debug.
   2. Press the play Stationeers-Debug, it should start the game imediatelly.
   3. Go to Debug > Attach to Unity Debugger. 
   4. The debug instance of the game should show up for you to attach. If not, hit refresh.
   4. The game should wait when the breakpoint is hit and the window focus should change to Visual Studio with the debug data. The game will wait until you press "Continue" in Visual Studio.
   
<hr>

## Installation details

### Unity Installation Details {#unity-install}
1. You'll need a [Unity ID](https://id.unity.com/account/new). This requires
   email verification and so forth, so best to get it out of the way first.
2. Download [Unity Hub](https://unity3d.com/get-unity/download), and log in.
3. Install Unity.2021.2.13f1. If it's not available in the interface, go to the
   [Unity Archive](https://unity3d.com/get-unity/download/archive), then choose the `Unity 2021.x` tab at the
   top, then `Unity.2021.2.13f1` and click the `Unity Hub` button.
