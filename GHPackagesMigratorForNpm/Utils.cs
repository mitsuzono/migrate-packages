using System.Text;
using System.Text.Json.Nodes;

namespace GHPackagesMigratorForNpm
{
    public static class Utils
    {
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
            + $"        \"test\": \"echo \\\"Error: no test specified\\\" && exit 1\"" // TODO エスケープして代入
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
            + $"      \"readme\": \"\"," // TODO エスケープして代入
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
