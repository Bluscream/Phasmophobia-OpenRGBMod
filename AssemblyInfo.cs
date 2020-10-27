using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(MelonLoaderMod.BuildInfo.Description)]
[assembly: AssemblyDescription(MelonLoaderMod.BuildInfo.Description)]
[assembly: AssemblyCompany(MelonLoaderMod.BuildInfo.Company)]
[assembly: AssemblyProduct(MelonLoaderMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + MelonLoaderMod.BuildInfo.Author)]
[assembly: AssemblyTrademark(MelonLoaderMod.BuildInfo.Company)]
[assembly: AssemblyVersion(MelonLoaderMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(MelonLoaderMod.BuildInfo.Version)]
[assembly: MelonInfo(typeof(MelonLoaderMod.OpenRGBMod), MelonLoaderMod.BuildInfo.Name, MelonLoaderMod.BuildInfo.Version, MelonLoaderMod.BuildInfo.Author, MelonLoaderMod.BuildInfo.DownloadLink)]


// Create and Setup a MelonGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonGameAttribute is found or any of the Values for any MelonGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonMame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Kinetic Games", "Phasmophobia")]