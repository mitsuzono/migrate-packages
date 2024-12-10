using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GHPackagesMigratorForNpm
{
    public static class Utils
    {
        public static async Task<IList<string>> GetLinkedPackageNamesAsync(string org, string repo, string pat)
        {
            var packages = await GetNpmPackagesAsync(org, pat);
            return packages
                .Where(p => p.repository.name == repo)
                .Select(p => p.name)
                .ToList();
        }

        private static async Task<IList<NpmPackagesPayload.NpmPackage>> GetNpmPackagesAsync(string org, string pat)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            var url = $"https://api.github.com/orgs/{org}/packages?package_type=npm&per_page=100";
            Console.WriteLine($"Getting npm packages from: {url}");
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get packages: {response.StatusCode}");
                return new List<NpmPackagesPayload.NpmPackage>();
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content) || !content.StartsWith('['))
            {
                Console.WriteLine("No packages found");
                return new List<NpmPackagesPayload.NpmPackage>();
            }

            var packages = JsonSerializer.Deserialize<NpmPackagesPayload>($"{{\"packages\":{content}}}");
            return packages?.packages ?? new List<NpmPackagesPayload.NpmPackage>();
        }

        public static async Task<JsonNode> GetNpmPackageVersionsAsync(string org, string packageName, string pat)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            var url = $"https://npm.pkg.github.com/@{org}/{packageName}";
            Console.WriteLine($"Getting packages from {url}");
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get packages: {response.StatusCode}");
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("No packages found");
                return default;
            }

            return JsonNode.Parse($"{{\"packages\":{content}}}");
        }

        public static async Task DownloadTarballAsync(string tarballUrl, string pat, string fileName)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            Console.WriteLine($"Downloading tarball from {tarballUrl}");
            using var response = await httpClient.GetAsync(tarballUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to download tarball: {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(fileName);
            await stream.CopyToAsync(fileStream);
        }

        public static async Task PutNpmPackageAsync(string org, string packageName, string base64, string pat, string contentJson)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "localhost");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
            var url = $"https://npm.pkg.github.com/@{org}/{packageName}";
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to upload to npm: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"errorResponse: {responseContent}");
                return;
            }
        }

        public static string CreatePutNpmPackagePayload(
            string org,
            string packageName,
            string version,
            string repo,
            string main,
            string test,
            string author,
            string license,
            string readme,
            string gitHead,
            string nodeVersion,
            string npmVersion,
            string integrity,
            string shasum,
            string encodedTarball,
            string tarballLength)
            => $"{{" 
            + $"  \"_id\": \"@{org}/{packageName}\","
            + $"  \"name\": \"@{org}/{packageName}\","
            + $"  \"description\": \"\","
            + $"  \"dist-tags\": {{"
            + $"    \"latest\": \"{version}\""
            + $"  }},"
            + $"  \"versions\": {{"
            + $"    \"{version}\": {{"
            + $"      \"name\": \"@{org}/{packageName}\","
            + $"      \"version\": \"{version}\","
            //+ $"      \"description\": \"\","
            + $"      \"main\": \"{main}\","
            + $"      \"scripts\": {{"
            + $"        \"test\": \"{test?.Replace("\"", "\\\"")}\""
            + $"      }},"
            + $"      \"author\": {{"
            + $"        \"name\": \"{author}\""
            + $"      }},"
            + $"      \"license\": \"{license}\","
            + $"      \"repository\": {{"
            + $"        \"type\": \"git\","
            + $"        \"url\": \"git+https://github.com/{org}/{repo}.git\""
            + $"      }},"
            + $"      \"publishConfig\": {{"
            + $"        \"registry\": \"https://npm.pkg.github.com\""
            + $"      }},"
            + $"      \"_id\": \"@{org}/{packageName}@{version}\","
            + $"      \"readme\": \"{readme?.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"")}\","
            //+ $"      \"readmeFilename\": \"\","
            + $"      \"gitHead\": \"{gitHead}\","
            + $"      \"bugs\": {{"
            + $"        \"url\": \"https://github.com/{org}/{repo}/issues\""
            + $"      }},"
            + $"      \"homepage\": \"https://github.com/{org}/{repo}#readme\","
            + $"      \"_nodeVersion\": \"{nodeVersion}\","
            + $"      \"_npmVersion\": \"{npmVersion}\","
            + $"      \"dist\": {{"
            + $"        \"integrity\": \"{integrity}\","
            + $"        \"shasum\": \"{shasum}\","
            + $"        \"tarball\": \"http://npm.pkg.github.com/@{org}/{packageName}/-/@{org}/{packageName}-{version}.tgz\""
            + $"      }}"
            + $"    }}"
            + $"  }},"
            + $"  \"access\": null,"
            + $"  \"_attachments\": {{"
            + $"    \"@{org}/{packageName}-{version}.tgz\": {{"
            + $"      \"content_type\": \"application/octet-stream\","
            + $"      \"data\": \"{encodedTarball}\","
            + $"      \"length\": {tarballLength}"
            + $"    }}"
            + $"  }}"
            + $"}}";
    }
}
