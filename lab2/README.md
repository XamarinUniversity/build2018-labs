# Build My First Xamarin App

In this walk-through, we will modify a Xamarin.Forms application to utilize a few Azure services. We will be working with three specific services:

1. [Azure App Services for Mobile Apps](https://azure.microsoft.com/en-us/services/app-service/mobile/)
2. [Azure Speech to Text cognitive services](https://docs.microsoft.com/en-us/azure/cognitive-services/speech/home)
3. [Azure App Services Mobile App Offline Synchronization](https://docs.microsoft.com/en-us/azure/app-service-mobile/app-service-mobile-ios-get-started-offline-data)

We will start with an existing application which was built with Visual Studio. It has target projects for iOS, Android, Windows (UWP), and macOS. The instructions here are primarily oriented towards Visual Studio for Windows, however you can also use Visual Studio for Mac to complete this walk-through.

> **Note** This lab assumes you are running Visual Studio on Windows 10 Fall Creators Update. If you are using an older version of Windows, you might be unable to run the UWP version of the application. In this case, you can skip those sections and concentrate on either iOS and/or Android.

## What you will learn
We will be focusing on integrating in Azure services into the app structure - particularly the services built specifically for Mobile Apps. At the end of the walk-through, you will have a good understanding of how to leverage these services in your own apps, as well as a starting point to explore other Azure services.

The app we will be working with is a community chat application named **My Circle**. It's a very simple application which allows a group of people to communicate in a public forum. Initially, it will work with local data only (mostly generated randomly), our goal will be to connect this app to Azure and provide a global, cloud-based data store so the app becomes a community connected application.

![My Circle running on UWP](media/image1.png) 

We will add this support in three stages:

1. [Part 1](#Part-One-add-support-for-Azure) - We will add the initial support to connect our app to Azure App Services and store our data in the cloud.

2. **Part 2** - We will add support for Azure Speech to Text cognitive services.

3. **Part 3** - We will finish our app by adding support for offline data synchronization so the app continues to work properly when the network is unavailable.

## Download the code

You can download the code [here](https://github.com/XamarinUniversity/build2018-labs/archive/master.zip) as a .zip file, or clone the repo with the following command-line:

```shell
git clone https://github.com/XamarinUniversity/build2018-labs.git
```

There are two labs - you want to work with **Lab 2** for this walk-through.

## Explore the Starter Solution

Let's start by exploring the starter solution. In the materials associated with this walk-through, you will find several folders:

| Folder    | Purpose |
|-----------|---------|
| **start** | The starting lab solution. |
| **part1** | The completed solution for Part 1 |
| **part2** | The completed solution for Part 2 |
| **part3** | The completed solution for Part 3 |

> It is recommended that you _copy_ each folder to another local location, preferably one with a short path to avoid any path restrictions on your development system.




## Part One: add support for Azure

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
