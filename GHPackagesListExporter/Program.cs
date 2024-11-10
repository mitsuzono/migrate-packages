using GHPackagesListExporter;

var org = args[0]; //"YOUR_ORG_NAME";
var pat = args[1]; //"ghp_xxx";

var packageTypes = new List<string> { "container", "npm", "nuget" };

foreach (var packageType in packageTypes)
{
    var packages = await Utils.GetPackages(org, packageType, pat);
    foreach (var package in packages)
    {
        var versions = await Utils.GetPackageVersions(org, packageType, package.name, pat);
        await Utils.OutputCsv($"{packageType}_{package.name}.csv", versions);
    }
}
