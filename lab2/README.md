# Connect your mobile app to the cloud with Azure App Services

### Estimated completion time: 60-75 minutes

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

1. [Part 1](#part-1-add-support-for-azure) - We will add the initial support to connect our app to Azure App Services and store our data in the cloud.

2. [Part 2](#part-2-add-speech-services) - We will add support for Azure Speech to Text cognitive services.

3. [Part 3](#part-3-add-offline-sync) - We will finish our app by adding support for offline data synchronization so the app continues to work properly when the network is unavailable.

## Download the code

You can download the code [here](https://github.com/XamarinUniversity/build2018-labs/archive/master.zip) as a .zip file, or clone the repo with the following command-line:

```shell
git clone https://github.com/XamarinUniversity/build2018-labs.git
```

> **Note:** There are two labs in this Github repository, make sure you are working with the contents of the **lab2** folder for this walk-through.

## Explore the Starter Solution

Let's start by exploring the starter solution. In the materials associated with this walk-through, you will find several folders:

| Folder    | Purpose |
|-----------|---------|
| **start** | The starting lab solution. |
| **part1** | The completed solution for Part 1 |
| **part2** | The completed solution for Part 2 |
| **part3** | The completed solution for Part 3 |

> It is recommended that you _copy_ each folder to another local location, preferably one with a short path to avoid any path restrictions on your development system.

1. Open the **MyCircle.sln** solution in the **start** folder.

2. There are five projects in this solution:
	- **MyCircle** - a .NET Standard shared-code project where all of our work will be done.
	- **MyCircle.Android** - the Xamarin.Android host which runs the app on the Android platform.
	- **MyCircle.iOS** - the Xamarin.iOS host which runs the app on the iPhone and iPad.
	- **MyCircle.Mac** - the Xamarin.Mac host which runs the app on macOS.
	- **MyCircle.UWP** - the Universal Windows host which runs the app on Windows 10.

We will be concentrating on the first project. This is a .NET Standard library which has all the shared code and UI definitions for the app.

![MyCircle project](media/image2.png)

### Data Folder

The **Data** folder holds model objects - these are underlying data objects which represent some state in our app. 

The only model object we have defined is the `CircleMessage` which is used to represent a single message sent or received from the app. It holds the Author, Color, and Text for a single message.

### Services Folder

The **Services** folder holds global services used by the application to perform some work.

| Service | Description |
|---------|-------------|
| `InMemoryRepository` | This is a sample in-memory implementation of the `IAsyncMessageRepository` interface. It is used in the starter to provide the storage for all the `CircleMessage` objects. We will replace this class in **part1** with a repository that connects to Azure. |
| `LoginService` | This service provides the ability for a user to login and logout of the application. |
| `UserInfo` | This service stores the currently logged on user in persistent storage using the built-in Xamarin.Forms properties collection. |

### ViewModels Folder

The app relies on the [Model-View-ViewModel design pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm) to provide bindable view-centric data objects for the UI to display. The **ViewModels** folder defines each of our bindable ViewModels.

| ViewModel | Description |
|-----------|-------------|
| `BasePageViewModel` | A base class for `MainViewModel` and `DetailsViewModel` which have some shared functionality. In particular, this class supports adding a new `CircleMessage` to the system and providing a collection of `CircleMessageViewModel` objects for the UI to display. |
| `CircleMessageViewModel` | A bindable wrapper around the `CircleMessage` data model. |
| `DetailsViewModel` | The ViewModel for the `DetailsPage` |
| `LoginViewModel` | The ViewModel for the `LoginPage` |
| `MainViewModel` | The ViewModel for the `MainPage` |
| `ProfileColorViewModel` | A ViewModel to hold a `Color` and current selection used in the `LoginPage` |
| `SpeechTranslatorViewModel` | A ViewModel for the `SpeechTranslatorPage` |

### XAML User Interface definitions

The app uses [XAML](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/xaml/xaml-basics/) to construct and display the user interface for each page. Each XAML file has a corresponding code-behind (**xam.cs**) file, which may have a little connecting logic in it, particularly for navigation. However, most of the programmed logic is contained in the [ViewModels Folder](#viewmodels-folder).

| XAML | Description |
|------|-------------|
| **App.xaml** | The main `Application` for the app - this is a singleton which is created as part of the app launch sequence and is the starting point for the app. |
| **DetailsPage.xaml** | This page displays the thread details for a conversation. At the top is the starting point, and then each response is displayed in a scrollable list. |
| **LoginPage.xaml** | This page is the first displayed UI in the app when the app has not been launched before. It allows the user to enter their name and select a color for their messages. You can also reach this app by tapping the Logout button on the main page. |
| **MainPage.xaml** | This page is the primary page which displays all the "root" conversations - these are starting points for any conversation thread. You can tap on a specific message to display the details. |
| **MessagesView.xaml** | This is a `ContentView` which is shared between the **MainPage.xaml** and **DetailsPage.xaml** to display a list of messages and a "New Message" `Entry` UI widget. |
| **SpeechTranslatorPage.xaml** | This is a starting point for the UI related to **part2** of the lab. It is incomplete and will be utilized as part of that step. |

## Run the starter application

1. Build the application in Visual Studio.

2. Select one of the supported platforms and set it as the Startup Project by right-clicking on the project in the Solution Explorer and selecting *Set as Startup*. Alternatively, you can use the platform drop-down in the toolbar which is next to the **Play** button.

![Select a Platform](media/image3.png)

> **Note:** The UWP app is only available if you are running on Windows, and the Mac app is only available if you are running on macOS. In addition, you must have a Mac to run the iOS project.

3. Run the application by clicking the **Play** button in the toolbar, or through the menu. Initially, you will be shown the **LoginPage**:

![Login Page](media/image4.png)

4. Type your name and select a color. Then tap the Circle icon at the bottom to enter the app.

5. You will now be presented with a series of messages.

![Main Page](media/image1.png)

6. The Toolbar at the top has two icons:

![Toolbar](media/image5.png)

7. Tap the "Refresh" icon to see new messages. Alternatively, on iOS and Android, you can use the "Pull to refresh" gesture. These new messages are generated internally by the app for testing.

8. You can add your own message by typing it into the `Entry` at the bottom of the page and pressing **ENTER** or **RETURN**.

9. Finally, when there is a response to a root message here, you will see a text marker indicating that replies are available:

![Replies are available](media/image6.png)

10. You can tap on that message to see the details, or to enter your own response.

## Create the Azure Mobile App Service

Let's start by creating an Azure Mobile App Service that our mobile app can use to store data in the cloud. There is an existing version of this service located at https://build2018mycircle.azurewebsites.net, however it likely won't be around forever, so you can use these steps to create your own version of the service. 

In addition, the source code for the pre-supplied service is located in this Github repo [here](https://github.com/XamarinUniversity/build2018-labs/tree/master/lab2/azure-service).

If you want to use the pre-supplied service, you can skip to [Part One](#part-1-add-support-for-azure) where we will add support for Azure to the app.

> **Note**: at this time, this part of the lab can only be done on Visual Studio for Windows. Once you have the service, you can use Visual Studio for Mac; alternatively, you can create the service using the [Azure Portal](https://portal.azure.com).

### Create the Azure Mobile App Service solution

1. Open Visual Studio for Windows.

2. Use the **File** > **New** > **Project...** menu option to open the New Project wizard

3. Select the **Visual C#** > **Web** > **ASP.NET Web Application (.NET Framework)** template.

![New Web App](media/image7.png)

4. Name the solution **build2018mycircle**.

5. Click **OK** to start the wizard.


### Set the web app options

1. Select **Azure Mobile App** from the set of choices.

2. Leave all other options as their defaults and click **OK** to create the web app.

![Web Options](media/image8.png)

### Change the default model to CircleMessage

The wizard created a default data object (**TodoItem**) and controller (**TodoItemController**) to manage a table. You can either create a new controller + data item, or rename this one since we aren't going to use it. We'll choose the latter in this walk-through.

1. Expand the **DataObjects** folder in the project.

2. Locate the **TodoItem.cs** source file and open it.

3. Right-click on the class name and select **Rename**. Type **CircleMessage** as the new class name.

4. Rename the file in the Solution Explorer to match the new class name. An easy way to do this in VS for Windows is to use the **Quick Actions and Refactorings...** menu (accessible through a right-click on the class, or **CTRL+.**)

5. Change the properties in the class definition to match our mobile app version of the definition:

```csharp
public class CircleMessage : EntityData
{
    public bool IsRoot { get; set; }
    public string ThreadId { get; set; }
    public string Author { get; set; }
    public string Text { get; set; }
    public string Color { get; set; }
}
```

### Rename the TodoItemController

1. Expand the **Controllers** folder in the Solution.

2. Open the **TodoItemController.cs** file.

3. Rename the class and file (using the same steps as above) to **CircleMessageController**.

4. Go through the class and replace all text references of "Todo" with "CircleMessage". This mostly affects comments, but there are a few method names which are changed too. You can do a global rename within the class if you like - these methods aren't referenced in code anywhere but are instead dispatched through the ASP.NET MVC framework.

### Rename the DbSet accessor

1. Expand the **Models** folder in the Solution.

2. Locate the **MobileServiceContext.cs** file and open it.

3. In this file, locate the **TodoItems** property and rename it to **CircleMessageItems**.

### Fix the MobileServiceInitializer

1. Expand the **App_Start** folder in the Solution.

2. Locate the **Startup.MobileApp.cs** file and open it.

3. At the bottom you will find a class named `MobileServiceInitializer` with a `Seed` method. This method is used to initialize an empty database with an initial dataset.

4. You can either replace the code with some default `CircleMessage` objects (just follow the same code pattern given for the `TodoItem` objects), or remove the method altogether if you don't want any initial data in the database.

### Publish the service to Azure

1. Build the service to make sure everything is renamed correctly.

2. Right-click on the project in the Solution Explorer and select **Publish**.

3. Select **App Service** from the side-bar, and **Create New** on the right side.

4. Click **Create Profile** to start the service creation. This will load your available subscriptions in Azure. You will need to have an account (a free one is fine) and it will prompt you to create one or to login if you are not currently logged in.

5. Give the app a name - this will become your URL which the mobile app will need to know.

6. Pick the subscription group, resource group, and hosting plan. A free plan is fine since this won't receive much traffic.

7. On the right side, click the **Create a SQL Database** link.

8. Select or create a SQL Server to host your database - you will need the admin username and password. Again, if you decide to create a DB, a free plan is fine for this.

9. Click **OK** to add the database configuration.

10. Click **Create** to publish the service - this will take a few minutes. When it's finished, your app will be running in Azure using the following URL:

```
https://<APPNAME>.azurewebsites.net
```

## Part 1: add support for Azure

In this first part, we will replace our test `InMemoryRepository` class with an Azure version that connects to an existing Azure Mobile App Service.

> **Note:** Our service is located at https://build2018mycircle.azurewebsites.net, if you used the prior instructions to create and publish your own service, make sure to use _that_ service URL instead

### Add client-support for Azure Mobile App Services

1. Right-click on the root node of the Solution Explorer and select **Manage NuGet Packages for Solution**.

> **Note**: on Visual Studio for Mac you cannot control NuGet packages at the solution level but have to add the packages to each project separately. Do the same steps but on each project node instead.

2. Use the **Browse** feature and search for the **Microsoft.Azure.Mobile.Client** NuGet package.

3. Add the package to each project in the solution - on Visual Studio for Windows you can do this in one step, on macOS you will need to do it one at a time.

![NuGet on Windows](media/image9.png)

### Examine the IAsyncMessageRepository interface

1. Our message repository is currently local and defined by the interface `IAsyncMessageRepository`. Locate that source file in the **MyCircle** shared-code project and open it.

2. The interface looks like this. Notice that all the methods are asynchronous and return `Task`s. Since we are going to be talking to a network-based DB in the cloud, this sort of interface makes perfect sense.

```csharp
public interface IAsyncMessageRepository
{
    Task AddAsync(CircleMessage message);
    Task<IEnumerable<CircleMessage>> GetRootsAsync();
    Task<long> GetDetailCountAsync(string id);
    Task<IEnumerable<CircleMessage>> GetDetailsAsync(string id);
}
```

| Method | Purpose |
|--------|---------|
| `AddAsync` | Add a new `CircleMessage` to the database. |
| `GetRootsAsync` | Retrieve all root (main) conversations - these are the ones added on the Main screen and they might, or might not have children. |
| `GetDetailCountAsync` | Return the number of _child_ messages associated with the specified root message id. |
| `GetDetailsAsync` | Returns all the child messages (threads) for the given root message id. |

> **Note:** If you like, you can look at a concrete implementation of this interface which manages an in-memory collection of `CircleMessages` in the **Services/InMemoryRepository.cs** file.

### Add a new AzureMessageRepository class

1. Add a new C# class into the **Services** folder. Name it **AzureMessageRepository.cs**

2. Make the class `public` and `sealed`. We don't plan to have anyone derive from the class and this can provide a (small) performance benefit when accessing the interface methods since the compiler knows there's no virtual dispatch required.

### Add support for Azure

This is where the meat of the Azure connectivity will be added. We will create a `MobileServiceClient` object which will give us access to our Azure service. This is really just a thing wrapper around `HttpClient` and it uses HTTP and REST protocols to work with the server-side code.

Each `TableController` you create on the server is mapped to a `IMobileServiceTable<T>` which lets you perform CRUD operations on the server table.

1. Add a `string` constant to the class to define where our Azure service is located. Name it **AzureServiceUrl** and set the value to be your Azure service URL:

```csharp
const string AzureServiceUrl = "https://build2018mycircle.azurewebsites.net";
```

2. Add a private field of type `Microsoft.WindowsAzure.MobileServices.MobileServiceClient` and name it **client**. This is our client accessor class to our Azure service.

3. Add a private field of type `Microsoft.WindowsAzure.MobileServices.IMobileServiceTable<CircleMessage>` and name it messages. This will hold the retrieved messages from Azure.

4. Add a public, default (no-argument) constructor to the class.

5. In the constructor, create a new `MobileServiceClient`, passing it your `AzureServiceUrl` and assign it to the **client** field.

6. After the **client** creation, call the method `GetTable<CircleMessage>()` to retrieve the server-side representation of `CircleMessages` from Azure and assign the result to the **messages** field.

```csharp
public sealed class AzureMessageRepository
{
    const string AzureServiceUrl = "https://build2018mycircle.azurewebsites.net";

    MobileServiceClient client;
    IMobileServiceTable<CircleMessage> messages;

    public AzureMessageRepository()
    {
        client = new MobileServiceClient(AzureServiceUrl);
        messages = client.GetTable<CircleMessage>();
    }
}
```

### Implement IAsyncMessageRepository

1. Have the class implement the `IAsyncMessageRepository` interface. You can use the built-in refactoring support to provide stubbed out implementations for the required methods or type them all yourself using the [interface definition above](#examine-the-iasyncmessagerepository-interface).

All the methods will be implemented using the **messages** `IMobileServiceTable<CircleMessage>` field.

### Implement the Add method

1. Use `InsertAsync` on the **messages** field to add the passed `CircleMessage`. 

```csharp
public Task AddAsync(CircleMessage message)
{
    return messages.InsertAsync(message);
}
```

### Implement GetRootsAsync

The `IMobileServiceTable` interface supports some basic LINQ capabilities which are turned into OData `$filter` queries that are executed on the server side. We can use these to filter our data so we are only returning the specific data we need vs. the entire table every time.

1. Use LINQ to add a `Where` clause that only looks for root messages (`IsRoot == true`), and where the returned data is in descending order on the `CreatedDate` property.

2. Use the extension method `ToEnumerableAsync` to execute the query on the server and return the results. You can use `async` and `await` to make the code easy to work with as shown below.

```csharp
public async Task<IEnumerable<CircleMessage>> GetRootsAsync()
{
    return await messages.Where(cm => cm.IsRoot)
        .OrderByDescending(cm => cm.CreatedDate)
        .ToEnumerableAsync();
}
```

### Implement GetDetailsAsync

1. You can implement `GetDetailsAsync` in the same way - except the `Where` clause should look for all `ThreadId` properties matching the passed **id** parameter.

2. It should be ordered by the `CreatedDate` property in _ascending_ order.

3. Use the same `ToEnumerableAsync` extension method to execute the query and return the `Task` with results.

### Implement GetDetailCountAsync

1. Use the same basic query you created for `GetDetailsAsync`, except add a test to your `Where` clause to exclude root messages so we only see the children (`IsRoot == false`).

2. Before calling `ToEnumerableAsync`, add a call to `IncludeTotalCount` into the fluent calls - this will return the total count of matching objects.

3. Call `ToEnumerableAsync` and cast the result to `IQueryResultEnumerable<CircleMessage>` - this has a `TotalCount` property you can return from the method.

4. An optimization you can make here is to append `ConfigureAwait(false)` to the fluent calls so we reuse the worker thread vs. coming back to the UI thread on the `await` call. This is optional but shown below.

```csharp
public async Task<long> GetDetailCountAsync(string id)
{
    var result = await messages
                   .Where(cm => cm.ThreadId == id && !cm.IsRoot)
                   .IncludeTotalCount().ToEnumerableAsync()
                   .ConfigureAwait(false) as IQueryResultEnumerable<CircleMessage>;
    return result.TotalCount;
}
```

### Replace the created repository

1. Open the **App.xaml.cs** file in the **MyCircle** shared-code project.

2. Locate the static property holding the `IAsyncMessageRepository` instance.

3. Replace the implementation with a new `AzureMessageRepository`.

```csharp
public static IAsyncMessageRepository Repository = new AzureMessageRepository();
```

4. Build the solution and cleanup any compilation errors.

### Fixup the data model

You could run the app now, but it will have a runtime failure. The problem is that our `CircleMessage` definition doesn't _quite_ match the server implementation. There are two things we need to do to it.

1. The server should be assigning the primary key (`Id`) and creation date. Currently, our code is doing that in the `CircleMessage` constructor. That code needs to be removed.

2. The server uses the name `CreatedAt` to hold the creation date as a `DateTimeOffset?` - this is a _hardcoded_ field in Azure and our client must conform either by changing the property name, or applying a `JsonProperty` attribute to change the network representation.

3. Make the necessary changes - make sure to use the built-in class rename feature to rename the `CreatedDate` field to `CreatedTo` - so you catch all usages. In addition, make sure to change the field type from `DateTime` to `DateTimeOffset?`.

Here's the correct definition of our `CircleMessage`:

```csharp
public class CircleMessage
{
	...
    public DateTimeOffset? CreatedAt { get; set; }

	...
    public CircleMessage(string parentId = null)
    {
        ThreadId = parentId ?? Guid.NewGuid().ToString();
    }
```

### Run the app

1. Run the application on your platform of choice. You should no longer see the test data, but should now be seeing data from the cloud.

2. If you insert a message, other copies of the app should be able to refresh and see your messages!

> **Note** Given that this is a shared data source, be family friendly please!

3. You can respond to messages.

4. If you close the app and reopen it, you should still see your messages.

## Part 2: Add Speech Services

Now that we have our app talking to Azure, let's utilize another Azure service to give some more features to the app - translating speech into text.

We will be using two NuGet based plug-ins for this:

1. [Xam.Plugin.SimpleAudioRecorder](https://www.nuget.org/packages/Xam.Plugin.SimpleAudioRecorder/0.3.0-beta) to record audio on the device in a cross-platform fashion. This component is open source and you can check out the implementation [here](https://github.com/adrianstevens/Xamarin-Plugins).

2. [Plugin.SpeechToText](https://www.nuget.org/packages/Plugin.SpeechToText/0.1.0-beta) to call Azure and translate the recorded speech to text using the [Bing Speech API](https://azure.microsoft.com/en-us/services/cognitive-services/speech/). Note that while the Azure service supports multiple languages, this specific component only works for spoken English.

We are using this component to access the Bing Speech to Text service only for convenience since it's mostly boiler plate code, but it's useful to look at the technique used. If you want to just jump adding the speech support to the app, you can [skip directly to that step](#add-the-required-nuget-packages).

### Working with Bing Speech to Text service

The Bing Speech to Text service is a REST-based web service that takes a media stream, passed in the body of the request, and returns the textual representation of the spoken words.

There is a .NET SDK which can be used for desktop applications, however it relies on some C++ code which makes it non-portable to mobile platforms.

There is also a .NET-based service library - but this is intended only for server-side use as it allows for long-authenticated requests.

Mobile apps are recommended to use the REST service directly which is located at https://speech.platform.bing.com/speech/recognition/.

There are two steps involved in translating a media file.

1. Get an authentication token.
2. Submit a translation request.

#### Get an API key

All the cognitive services (vision, speech, search, etc.) use shared API keys ("secrets") to validate access to the service. You must request a key in order to gain access to each service you want to use.

There are two ways to get an API key. You can get the Free/Trial key which allows for 5k transactions at 20/minute. Or you can tie the key to an Azure subscription for higher load.

In this case, we'll use the free/trial key, but you can log into the [Azure Portal](https://portal.azure.com) to request a full paid key.

1. Open a browser and go to https://azure.microsoft.com/en-us/try/cognitive-services/

2. Select **Speech** as the API.

3. Click **Get API Key** on the Bing Speech API entry.

4. Agree to the terms and login to a Microsoft account to assign the free key.

5. It will then take you to the Cognitive Services API page, scroll down to fine the **Bing Speech API**. It will contain two API keys (primary/secondary). Copy down the primary key.

![API Key page](media/image10.png)

#### Authenticate to the Cognitive service API

The Bing Speech to Text REST API uses an authentication endpoint located at https://api.cognitive.microsoft.com/sts/v1.0/issueToken to issue temporary access tokens valid for 10 minutes. 

You get a token by calling the authentication endpoint, passing the API key as a header value for the `Ocp-Apim-Subscription-Key` key. The returned data will either be an error, or a valid authentication token to access the service - valid for 10 minutes.

Here's an example of getting the access token given the API key obtained from the Azure portal:

```csharp
async Task<string> GetAuthenticationToken(string apiKey)
{
	HttpClient client = new HttpClient();
	httpClient.DefaultRequestHeaders.Add(
		 "Ocp-Apim-Subscription-Key", apiKey);

	var uriBuilder = new UriBuilder("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");

    var result = await httpClient.PostAsync(
    	uriBuilder.Uri.AbsoluteUri, null);
    return await result.Content.ReadAsStringAsync();
}
```

#### Translating Speech to Text

The Speech to Text API takes a **WAV** formatted file (only PCM Mono 16k encoding is supported) and translates it to a string.

You send a `POST` request to https://speech.platform.bing.com/speech/recognition/dictation/cognitiveservices/v1 with a few query string parameters to indicate the format and language to expect.

The request will be denied unless a valid bearer access token (obtained above) is passed in the HTTP Authorization header.

The API returns a JSON object with the following format (**Note**: we are using a JSON.net attribute to make sure the network name for the object is "result"):

```csharp
[JsonObject("result")]
public class SpeechToTextResult
{
    public string RecognitionStatus { get; set; }
    public string DisplayText { get; set; }
    public string Offset { get; set; }
    public string Duration { get; set; }
}
```

Here's a simple example of calling the REST API to translate a **.wav** file passed in as a `Stream` along with an auth token obtained using the code above.

```csharp
async Task<SpeechToTextResult> SendRequestAsync(Stream mediaStream, string authToken)
{
	string url = "https://speech.platform.bing.com/speech/recognition/dictation/cognitiveservices/v1" + 
		"?language=en-us&format=simple";

	HttpClient client = new HttpClient();
	client.DefaultRequestHeaders.Authorization = 
	  new AuthenticationHeaderValue("Bearer", authToken);

	// Define the media we want to translate.
	var content = new StreamContent(mediaStream);
	content.Headers.TryAddWithoutValidation(
		"Content-Type", 
		"audio/wav; codec=\audio/pcm\"; samplerate=16000");

	var response = await httpClient.PostAsync(url, content);
	var jsonText = await response.Content.ReadAsStringAsync();

	return JsonConvert.DeserializeObject<SpeechToTextResult>(jsonText);
}
```

You can then get the text from the `DisplayText` property of the returning object.

### Add the required NuGet packages




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
