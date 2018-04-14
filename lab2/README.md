# Part One: add support for Azure

1. Add Microsoft.Azure.Mobile.Client NuGet to all projects.
2. Create AzureMessageRepository.cs
3. Implement IMessageRepository
4. Add MobileServiceClient field and initialize with "https://build2018mycircle.azurewebsites.net"
5. Add `IMobileServiceTable<CircleMessage> messages;`
6. Initialize messages with `client.GetTable<CircleMessage>();`

7. Modify CircleMessage data structure
	a) Rename `CreatedDate` to `CreatedAt` and make `DateTimeOffset?`
	b) Remove setter for `Id` and `CreatedAt` in ctor

8. Implement interface
9. Replace repository in `App.xaml.cs`
10. Run the app and see server data.


1. Add Xam.Plugin.SimpleAudioRecorder (pre-release)
2. Add Xam.Plugin.SpeechToText (myget)
   - need to add feed: https://www.myget.org/F/speech-to-text/api/v2
   - Key: fb3c4e67a81242f794cb56ebb279271d
3. Add code to SpeechTranslatorViewModel.cs
4. Add permissions to UWP project (appxmanifest - Capabilities - Microphone)
5. Add permission to iOS info.plist (hand edit)
	<key>NSMicrophoneUsageDescription</key>
  	<string>Record my voice for Speech to Text</string>
6. Add permissions to Android Manifest
	- MODIFY_AUDIO_SETTINGS, RECORD_AUDIO, READ_EXTERNAL_STORAGE & WRITE_EXTERNAL_STORAGE
7. Add Plugin.Permissions component to all projects.
8. Add Android permissions pieces to MainActivity
	- CrossCurrentActivity.Current.Activity = this; // OnCreate
	- OnRequestPermissions
9. Add code to request mic permission into ViewMOdel
10. Call method from OnStartRecording

```
private async Task<bool> RequestAudioPermissions()
{
    try
    {
        var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);

        if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Microphone))
        {
            var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Microphone);
            if (results.ContainsKey(Permission.Microphone))
                status = results[Permission.Microphone];

            if (status == PermissionStatus.Granted)
            {
                return true;
            }
        }
    }
    catch
    {

    }
    return false;
}
```

## Part3

1. Add Microsoft.Azure.Mobile.Client.SQLiteStore to all projects
2. Add VC2015 runmtime lib to UWP app
2. Need a parameterless ctor on CircleMessage!
3. Add InitializeTableAsync, InitializeOfflineStorageAsync, and SynchronizeAsync to Azure repo
4. Call Synchronize in each method.
..
5. Add Xam.Plugin.Connectivity to all projects
6. Update SynchronizeAsync to check connectivity .. fun test: use if (true) to exit early and show that it's cached offline with no sync.
..
7. Add pushChanges to Synchronize and set to false when using GetXXX methods.
..
7. Add ResolveConflictAsync method
8. Add Version field to CircleMessage
9. Implement IEquatable<CircleMessage> on CircleMessage .. Test Id, Text, Color, ThreadId.
..
10. Add PurgeOldRecordsAsync, call at end of InitializeTableAsync

> CAN UNINSTALL APP TO FORCE A FULL CACHE CLEAR!
