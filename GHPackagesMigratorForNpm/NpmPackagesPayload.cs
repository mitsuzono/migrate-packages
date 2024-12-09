﻿namespace GHPackagesMigratorForNpm
{
    public class NpmPackagesPayload
    {
        public IList<NpmPackage> packages { get; set; }

        public class NpmPackage
        {
            public int id { get; set; }
            public string name { get; set; }
            public string package_type { get; set; }
            public Owner owner { get; set; }
            public int version_count { get; set; }
            public string visibility { get; set; }
            public string url { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public Repository repository { get; set; }
            public string html_url { get; set; }

            public class Owner
            {
                public string login { get; set; }
                public int id { get; set; }
                public string node_id { get; set; }
                public string avatar_url { get; set; }
                public string gravatar_id { get; set; }
                public string url { get; set; }
                public string html_url { get; set; }
                public string followers_url { get; set; }
                public string following_url { get; set; }
                public string gists_url { get; set; }
                public string starred_url { get; set; }
                public string subscriptions_url { get; set; }
                public string organizations_url { get; set; }
                public string repos_url { get; set; }
                public string events_url { get; set; }
                public string received_events_url { get; set; }
                public string type { get; set; }
                public string user_view_type { get; set; }
                public bool site_admin { get; set; }
            }

            public class Repository
            {
                public int id { get; set; }
                public string node_id { get; set; }
                public string name { get; set; }
                public string full_name { get; set; }
                public bool _private { get; set; }
                public Owner owner { get; set; }
                public string html_url { get; set; }
                public object description { get; set; }
                public bool fork { get; set; }
                public string url { get; set; }
                public string forks_url { get; set; }
                public string keys_url { get; set; }
                public string collaborators_url { get; set; }
                public string teams_url { get; set; }
                public string hooks_url { get; set; }
                public string issue_events_url { get; set; }
                public string events_url { get; set; }
                public string assignees_url { get; set; }
                public string branches_url { get; set; }
                public string tags_url { get; set; }
                public string blobs_url { get; set; }
                public string git_tags_url { get; set; }
                public string git_refs_url { get; set; }
                public string trees_url { get; set; }
                public string statuses_url { get; set; }
                public string languages_url { get; set; }
                public string stargazers_url { get; set; }
                public string contributors_url { get; set; }
                public string subscribers_url { get; set; }
                public string subscription_url { get; set; }
                public string commits_url { get; set; }
                public string git_commits_url { get; set; }
                public string comments_url { get; set; }
                public string issue_comment_url { get; set; }
                public string contents_url { get; set; }
                public string compare_url { get; set; }
                public string merges_url { get; set; }
                public string archive_url { get; set; }
                public string downloads_url { get; set; }
                public string issues_url { get; set; }
                public string pulls_url { get; set; }
                public string milestones_url { get; set; }
                public string notifications_url { get; set; }
                public string labels_url { get; set; }
                public string releases_url { get; set; }
                public string deployments_url { get; set; }
            }
        }
    }
}
