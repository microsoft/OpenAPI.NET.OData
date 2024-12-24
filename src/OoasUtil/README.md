# OoasUtil Command

## Name

`OoasUtil.exe` - General executable application used used to create [Open Api 3.0](https://swagger.io/specification/) document based on OData Edm input.

## Synopsis

```C#

OoasUtil.exe [--help|-h] [--version|-v] [Options]
```

## Description

`OoasUtil.exe` is a tool used to convert CSDL to Open API document.

## Options

### [--json|-j]

Output the "JSON" format Open API document;

### [--yaml|-y]

Output the "YAML" format Open API document;

### [--keyassegment|-k]

Output the document using key-as-segment style URLs.;

### [--derivedtypesreferencesforresponses|-drs]

Output the document to expect all derived types in responses.;

### [--derivedtypesreferencesforrequestbody|-drq]

Output the document to expect all derived types in request bodies.;

### [--enablepagination|-p]

Output the document to expose pagination for collections.;

### [--enableunqualifiedcall|-u]

Output the document to use unqualified calls for bound operations.;

### [--disableschemaexamples|-x]

Output the document without examples in the schema.;

### [--yaml|-y]

Output the "YAML" format Open API document;

### [--specversion|-s int]

Indicate which version, either 2 or 3, of the OpenApi specification to output. Only 2 or 3 are supported;

### [--input|-i file]

Indicate to where to get CSDL, from file or from Uri.

### [--output|-o file]

Indicate to output file name.


## Examples

`OoasUtil.exe -j -k -drs -drq -p -u -s 3 -i http://services.odata.org/TrippinRESTierService -o trip.json`

The content of `trip.json` is similar at https://github.com/xuzhg/OData.OpenAPI/blob/main/Microsoft.OData.OpenAPI/Microsoft.OData.OpenAPI.Tests/Resources/TripService.OpenApi.json

# Alternative Tool - Hidi

This OoasUtil Command tool is currently not actively maintained, and an alternative command line tool, Hidi, is available for use in converting CSDL to OpenAPI. You can find the link to its README [here](https://github.com/microsoft/OpenAPI.NET/blob/vnext/src/Microsoft.OpenApi.Hidi/readme.md) which includes setup instructions.

