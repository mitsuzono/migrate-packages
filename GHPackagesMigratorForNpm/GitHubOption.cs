namespace GHPackagesMigratorForNpm
{
    public class GitHubOption
    {
        public string SourceOrg { get; set; }
        public string SourceRepo { get; set; }
        public string SourcePat { get; set; }
        public string TargetOrg { get; set; }
        public string TargetRepo { get; set; }
        public string TargetPat { get; set; }
    }
}
