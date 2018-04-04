# Couchbase Document Expiry Setter

Sets document expiry for all documents in specified Couchbase buckets.

This tool was created to resolve some disk space issues due to documents added to Couchbase by the https://github.com/OrleansContrib/OrleansCouchbaseProvider having no expiry value set. 

Currently the same expiry value is set for all specified buckets but could be easily extended to set this per bucket or document type.

The tool will hit the Couchbase REST API and request the number of document ids specified using the batch size parameter. It will then set the expiry value for all returned documents. This process will repeat until all documents have been updated. 

## Parameters

This is a command line tool that takes the following parameters:

-h, --host         Required. The hostname of the Couchbase server

-b, --buckets      Required. The Couchbase bucket(s) to process, eg: bucket1:password1,bucket2:password2

-u, --username     Required. Couchbase username

-p, --password     Required. Password for the specified Couchbase username

-e, --expiry       Required. The number of minutes to set as expiry time for each document

-v, --viewname     Required. The Couchbase map/reduce view used to get document ids

-s, --batchsize    The number of documents to request from Couchbase REST service for processing (default = 2000)

-a, --apiport      The port the Couchbase REST API responds to (default = 8092)

--help             Display this help screen.

--version          Display version information.

## Couchbase requirements

Map/reduce views are used to get the list of documents to update.

To get a list of documents with no expiry value set, you can create a Couchbase map/reduce view with the following code in the map section:

```javascript
function (doc, meta) {
  if(meta.expiration === 0) emit(null, null);
}
```

To get a list of all document ids, put this code in the map section:

```javascript
function (doc, meta) {
  emit(null, null);
}
```

## Disclaimer

This has been tested with Couchbase 4.6.2-3905 Enterprise Edition (build-3905) only.
