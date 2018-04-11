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