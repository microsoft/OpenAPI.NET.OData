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

### [--specversion|-s int]

Indicate which version, either 2 or 3, of the OpenApi specification to output. Only 2 or 3 are supported;

### [--yaml|-y]

Output the "YAML" format Open API document;

### [--input|-i file]

Indicate to where to get CSDL, from file or from Uri.

### [--output|-o file]

Indicate to output file name.


## Examples

`OoasUtil.exe -j -s 3 -i http://services.odata.org/TrippinRESTierService -o trip.json`

The content of `trip.json` is similar at https://github.com/xuzhg/OData.OpenAPI/blob/master/Microsoft.OData.OpenAPI/Microsoft.OData.OpenAPI.Tests/Resources/TripService.OpenApi.json

