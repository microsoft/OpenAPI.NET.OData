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

### [--input|-i file]

Indicate to where to get CSDL, from file or from Uri.

### [--output|-o file]

Indicate to output file name.


## Examples

`OoasUtil.exe -j -i http://services.odata.org/TrippinRESTierService -o trip.json`

The content of `trip.json` is similiar at https://github.com/xuzhg/OData.OpenAPI/blob/master/Microsoft.OData.OpenAPI/Microsoft.OData.OpenAPI.Tests/Resources/TripService.OpenApi.json

