using GHPackagesMigratorForNpm;
using Microsoft.Extensions.Configuration;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var option = config.GetSection("GitHub").Get<GitHubOption>();
if (option == null)
{
    Console.WriteLine("GitHubOption(appsettings.json) is not configured");
    return;
}
var sourceOrg = option.SourceOrg;
var sourceRepo = option.SourceRepo;
var sourcePat = option.SourcePat;
var targetOrg = option.TargetOrg;
var targetRepo = option.TargetRepo;
var targetPat = option.TargetPat;

// Get package names linked to sourceRepo
var packageNames = await Utils.GetLinkedPackageNamesAsync(sourceOrg, sourceRepo, sourcePat);

foreach (var packageName in packageNames)
{
    // Get all package versions
    var packagesNode = await Utils.GetNpmPackageVersionsAsync(sourceOrg, packageName, sourcePat);

    var sortedTimes = packagesNode["packages"]!["time"]!.AsObject().OrderBy(r => r.Value!.GetValue<DateTime>());
    foreach (var item in sortedTimes)
    {
        Console.WriteLine($"version: {item!.Key}");
        var versionItem = packagesNode["packages"]!["versions"]!.AsObject().Where(r => r.Key == item.Key).FirstOrDefault();
        var fileName = $"{sourceOrg}_{packageName}_{versionItem.Key}.tgz";

        // Download tarball
        await Utils.DownloadTarballAsync(versionItem.Value["dist"]!["tarball"]!.GetValue<string>(), sourcePat, fileName);

        // to base64
        var bytes = File.ReadAllBytes(fileName);
        var base64 = Convert.ToBase64String(bytes);

        // Upload to npm
        var putContent = Utils.CreatePutNpmPackagePayload(
            targetOrg,
            packageName,
            versionItem.Key,
            targetRepo,
            versionItem.Value["main"]?.GetValue<string>() ?? "",
            versionItem.Value["scripts"]?["test"]?.GetValue<string>() ?? "",
            versionItem.Value["author"]?["name"]?.GetValue<string>() ?? "",
            versionItem.Value["license"]?.GetValue<string>() ?? "",
            versionItem.Value["readme"]?.GetValue<string>() ?? "",
            versionItem.Value["gitHead"]?.GetValue<string>() ?? "",
            versionItem.Value["_nodeVersion"]?.GetValue<string>() ?? "",
            versionItem.Value["_npmVersion"]?.GetValue<string>() ?? "",
            versionItem.Value["dist"]?["integrity"]?.GetValue<string>() ?? "",
            versionItem.Value["dist"]?["shasum"]?.GetValue<string>() ?? "",
            base64,
            bytes.Length.ToString());
        Console.WriteLine($"putContent: {putContent}");
        await Utils.PutNpmPackageAsync(targetOrg, packageName, versionItem.Key, targetPat, putContent);
    }
}
