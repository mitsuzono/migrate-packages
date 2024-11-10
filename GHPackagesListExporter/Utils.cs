using System.Text;
using System.Text.Json;

namespace GHPackagesListExporter
{
    public static class Utils
    {
        public static async Task<IList<PackagesPayload.Package>> GetPackages(string org, string packageType, string pat)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            using var response = await httpClient.GetAsync($"https://api.github.com/orgs/{org}/packages?package_type={packageType}");
            var content = await response.Content.ReadAsStringAsync();
            var packages = JsonSerializer.Deserialize<PackagesPayload>($"{{\"packages\":{content}}}");
            return packages?.packages;
        }

        public static async Task<IList<PackageVersionsPayload.PackageVersion>> GetPackageVersions(string org, string packageType, string packageName, string pat)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            using var response = await httpClient.GetAsync($"https://api.github.com/orgs/{org}/packages/{packageType}/{packageName}/versions");
            var content = await response.Content.ReadAsStringAsync();
            var versions = JsonSerializer.Deserialize<PackageVersionsPayload>($"{{\"versions\":{content}}}");
            return versions?.versions.OrderBy(v => v.created_at).ToList();
        }

        public static async Task OutputCsv(string path, IList<PackageVersionsPayload.PackageVersion> versions)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            foreach (var version in versions)
            {
                Console.WriteLine($"{version.name} {version.created_at}");
                var tags = version.GetTags();

                if (!tags.Any())
                {
                    await writer.WriteLineAsync($"{version.id},,{version.name},{version.created_at},COMMIT_ID,.");
                    continue;
                }

                foreach (var tag in tags)
                {
                    Console.WriteLine($"  {tag}");
                    await writer.WriteLineAsync($"{version.id},{tag},{version.name},{version.created_at},COMMIT_ID,.");
                }
            }
        }
    }
}
