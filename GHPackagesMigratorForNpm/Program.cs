using GHPackagesMigratorForNpm;

var sourceOrg = args[0]; //"YOUR_SOURCE_ORG_NAME";
var packageName = args[1]; //"YOUR_PACKAGE_NAME";
var pat = args[2]; //"ghp_xxx";
var targetOrg = args[3]; //"YOUR_TARGET_ORG_NAME";

// Get all package versions
var packagesNode = await Utils.GetNpmPackageVersionsAsync(sourceOrg, packageName, pat);

foreach (var item in packagesNode["packages"]!["versions"]!.AsObject())
{
    Console.WriteLine($"version: {item!.Key}");
    var fileName = $"{sourceOrg}_{packageName}_{item.Key}.tgz";

    // Download tarball
    await Utils.DownloadTarballAsync(item.Value["dist"]!["tarball"]!.GetValue<string>(), pat, fileName);

    // to base64
    var bytes = File.ReadAllBytes(fileName);
    var base64 = Convert.ToBase64String(bytes);

    // Upload to npm
    var repoUrl = item.Value["repository"]!["url"]!.GetValue<string>().Substring(4);
    var putContent = Utils.CreatePutNpmPackagePayload(
        targetOrg,
        packageName,
        item.Key,
        repoUrl,
        item.Value["main"]!.GetValue<string>(),
        item.Value["scripts"]!["test"]!.GetValue<string>(),
        item.Value["author"]!["name"]!.GetValue<string>(),
        item.Value["license"]!.GetValue<string>(),
        item.Value["readme"]!.GetValue<string>(),
        item.Value["gitHead"]!.GetValue<string>(),
        item.Value["_nodeVersion"]!.GetValue<string>(),
        item.Value["_npmVersion"]!.GetValue<string>(),
        item.Value["dist"]!["integrity"]!.GetValue<string>(),
        item.Value["dist"]!["shasum"]!.GetValue<string>(),
        base64,
        bytes.Length.ToString());
    Console.WriteLine($"putContent: {putContent}");
    await Utils.PutNpmPackageAsync(targetOrg, packageName, item.Key, pat, putContent);
}
