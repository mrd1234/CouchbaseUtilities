﻿namespace CouchbaseDocumentExpirySetter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using CommandLine;
    using Couchbase.Configuration.Client;

    public class Options
    {
        private string BaseUrl { get; set; } = "http://{0}:{1}/{2}/_design/{3}/_view/{3}?connection_timeout=60000&limit={4}&skip={5}&stale=false";

        [Option('h', "host", Required = true, HelpText = "The hostname of the Couchbase server. Do not include a port or an exception will be thrown.")]
        public string HostName { get; set; }

        [Option('b', "buckets", Required = true, Separator = ',', HelpText = "The Couchbase bucket(s) to process, eg: bucket1:password1,bucket2:password2")]
        public IEnumerable<string> Buckets { get; set; }

        [Option('u', "username", Required = true, HelpText = "Couchbase username.")]
        public string UserName { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password for the specified Couchbase username.")]
        public string Password { get; set; }

        [Option('e', "expiry", Required = true, HelpText = "The number of minutes to set as expiry time for each document (use 0 if you want document to never expire).")]
        public long ExpiryMinutes { get; set; }

        [Option('v', "viewname", Required = true, HelpText = "The Couchbase map/reduce view used to get document ids.")]
        public string ViewName { get; set; }

        [Option('s', "batchsize", Required = false, HelpText = "The number of documents to request per call from Couchbase REST service for processing (default = 2000).")]
        public int BatchSize { get; set; } = 2000;

        [Option('a', "apiport", Required = false, HelpText = "The port the Couchbase REST API responds to (default = 8092).")]
        public int Port { get; set; } = 8092;

        [Option('l', "documentlimit", Required = false, HelpText = "The maximum number of documents to update. Default is all documents. This will override batchsize if the documentlimit value is lower that batchsize value.")]
        public int? DocumentLimit { get; set; }

        [Option('i', "showdetails", Required = false, HelpText = "Indicates the document id and ttl should be output to the console for each updated document.")]
        public bool ShowDetails { get; set; } = false;

        [Option('f', "logfile", Required = false, HelpText = "Full path and filename to log list of updated documents to.")]
        public string LogFile { get; set; }

        public LogManager LogManager { get; set; }

        public ClientConfiguration BuildClientConfiguration()
        {
            var config = new ClientConfiguration
            {
                ApiPort = Port,
                Servers = new List<Uri> { new Uri($"http://{HostName.Trim()}") }
            };

            config.BucketConfigs.Clear();

            foreach (var bucket in Buckets)
            {
                var parts = bucket.Split(':');
                config.BucketConfigs.Add(parts[0], new BucketConfiguration { BucketName = parts[0] });
            }

            return config;
        }

        public string BuildRestUrl(string bucketName, int skip)
        {
            return string.Format(BaseUrl, HostName.Trim(), Port, bucketName, ViewName.Trim(), BatchSize, skip);
        }

        public NetworkCredential GetCredentials()
        {
            return new NetworkCredential(UserName.Trim(), Password.Trim());
        }

        public Dictionary<string, string> GetBucketDetails()
        {
            return Buckets.Where(w => !string.IsNullOrWhiteSpace(w)).Select(bucket => bucket.Split(':')).ToDictionary(bucketDetails => bucketDetails[0].Trim(), bucketDetails => bucketDetails[1]);
        }
    }
}