# SettingsProviders

This repository provides two custom implementations of the `SettingsProvider` class in the .NET Framework application settings architecture.
They can be used to make the built-in application of WinForms and WPF application settings portable and more configurable:

* **PortableSettingsProvider** saves app settings in an XML format similar to the built-in settings provider
* **PortableJsonSettingsProvider** saves app settings in JSON format, powered by Newtonsoft.Json

### Features

* Make application settings portable together with the app
* Configure the location and name of the settings file
* Easily plug into existing apps (just one line needed to make existing app settings portable)

## Usage

_Note:_ Change all occurrences of `PortableSettingsProvider` to `PortableJsonSettingsProvider` to use the JSON version.

1. Install the nuget package:  
	[`Install-Package PortableSettingsProvider`](https://www.nuget.org/packages/PortableSettingsProvider) or  
	[`Install-Package PortableJsonSettingsProvider`](https://www.nuget.org/packages/PortableJsonSettingsProvider)
2. Apply the provider to your settings:
	```
	PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
	```
	Make sure to adjust the name of the settings instance if it is not the default settings.  
	For an alternative approach, see [here](https://www.codeproject.com/Articles/1238550/Making-Application-Settings-Portable).
3. Optionally, you can set a different name and location for the settings file. 
	A full configuration for a WinForms application might look like this:
	
```csharp
static void Main()
{
	PortableSettingsProvider.SettingsFileName = "settings.config";
	PortableSettingsProvider.SettingsDirectory = "Some\\custom\\location";
	PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
	Application.Run(new Form1());
}
```

## More

For a detailed explanation of the implementation and more options,
visit the [related CodeProject article](https://www.codeproject.com/Articles/1238550/Making-Application-Settings-Portable).
The article also explains the difference between _roamed_ and _local_ settings as the settings providers support both.

## License

[BSD-3-Clause](LICENSE) License.
