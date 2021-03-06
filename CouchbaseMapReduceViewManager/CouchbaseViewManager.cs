﻿namespace CouchbaseMapReduceViewManager
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Couchbase;

    public class CouchbaseViewManager
    {
        private Options Options { get; }

        public CouchbaseViewManager(Options options)
        {
            Options = options;

            ClusterHelper.Initialize(options.BuildClientConfiguration());
        }

        public async Task AddViewsAsync()
        {
            var buckets = Options.GetBucketDetails();

            foreach (var b in buckets)
            {
                using (var bucket = ClusterHelper.GetBucket(b.Key, b.Value))
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    var viewDefinition = BuildViewDefinition();

                    using (var bucketManager = bucket.CreateManager(Options.UserName, Options.Password))
                    {
                        var existing = await bucketManager.GetDesignDocumentAsync(Options.ViewName).ConfigureAwait(false);
                        if (existing.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Bucket '{b.Key}' already has a mapreduce view named '{Options.ViewName}' - skipping...");
                            Console.ResetColor();
                            continue;
                        }

                        var result = await bucketManager.InsertDesignDocumentAsync(Options.ViewName, viewDefinition).ConfigureAwait(false);
                        if (!result.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error inserting view '{Options.ViewName}' for bucket '{b.Key}': {(result.Exception != null ? result.Exception.Message : result.Message)}");
                            Console.ResetColor();
                        }
                    }

                    sw.Stop();

                    Console.WriteLine();
                    Console.WriteLine($"Processing bucket '{b.Key}' took {sw.Elapsed.TotalSeconds} seconds");
                }
            }
        }

        private string BuildViewDefinition()
        {
            if (string.IsNullOrWhiteSpace(Options.ViewReduceContent))
            {
                return "{\"views\":{\"" + Options.ViewName + "\":{\"map\":\"" + Options.ViewMapContent + "\"}}}";
            }

            return "{\"views\":{\"" + Options.ViewName + "\":{\"map\":\"" + Options.ViewMapContent + "\",\"reduce\":\"" + Options.ViewReduceContent + "\"}}}";
        }
    }
}
