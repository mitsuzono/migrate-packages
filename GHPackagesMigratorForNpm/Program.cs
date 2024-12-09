using GHPackagesMigratorForNpm;

var sourceOrg = args[0]; //"YOUR_SOURCE_ORG_NAME";
var sourceRepo = args[1]; //"YOUR_SOURCE_REPO_NAME";
var sourcePat = args[2]; //"ghp_xxx";
var targetOrg = args[3]; //"YOUR_TARGET_ORG_NAME";
var targetRepo = args[4]; //"YOUR_TARGET_REPO_NAME";
var targetPat = args[5]; //"ghp_xxx";

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
