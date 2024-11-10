namespace GHPackagesListExporter
{
    public class PackageVersionsPayload
    {
        public IList<PackageVersion> versions { get; set; }

        public class PackageVersion
        {
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string package_html_url { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string html_url { get; set; }
            public Metadata metadata { get; set; }
            public IList<string> GetTags()
                => metadata?.container?.tags ?? new List<string>();

            public class Metadata
            {
                public string package_type { get; set; }
                public Container container { get; set; }

                public class Container
                {
                    public IList<string> tags { get; set; }
                }
            }
        }
    }
}
