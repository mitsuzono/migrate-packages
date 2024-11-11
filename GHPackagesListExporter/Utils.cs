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
            var url = $"https://api.github.com/orgs/{org}/packages?package_type={packageType}&per_page=100";
            Console.WriteLine($"Getting packages from {url}");
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get packages: {response.StatusCode}");
                return new List<PackagesPayload.Package>();
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content) || !content.StartsWith('['))
            {
                Console.WriteLine("No packages found");
                return new List<PackagesPayload.Package>();
            }

            var packages = JsonSerializer.Deserialize<PackagesPayload>($"{{\"packages\":{content}}}");
            return packages?.packages ?? new List<PackagesPayload.Package>();
        }

        public static async Task<IList<PackageVersionsPayload.PackageVersion>> GetPackageVersions(string org, string packageType, string packageName, string pat)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            var url = $"https://api.github.com/orgs/{org}/packages/{packageType}/{packageName}/versions?per_page=100";
            Console.WriteLine($"Getting package versions from {url}");
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get package versions: {response.StatusCode}");
                return new List<PackageVersionsPayload.PackageVersion>();
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content) || !content.StartsWith('['))
            {
                Console.WriteLine("No package versions found");
                return new List<PackageVersionsPayload.PackageVersion>();
            }
            var versions = JsonSerializer.Deserialize<PackageVersionsPayload>($"{{\"versions\":{content}}}");
            return versions?.versions.OrderBy(v => v.created_at).ToList() ?? new List<PackageVersionsPayload.PackageVersion>();
        }

        public static async Task OutputCsv(string path, IList<PackageVersionsPayload.PackageVersion> versions)
        {
            Console.WriteLine($"Writing to file {path}");
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
