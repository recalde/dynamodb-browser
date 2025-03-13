# ASP.NET 9.0 DynamoDB Browser

## Conversation Log

### User Prompt 1:
```
I have a c sharp application running in a docker container on OpenShift right now that has a AWS shared credential file mounted to ./aws/credentials that has a default profile that can access a Dynamo database. Can you write me an ASP.net page that will use an AWS client created using AWS_SHARED_CREDENTIALS_FILE environment variable and "default" profile. The app should list all of the Dynamo database tables in a drop-down box and it should render an HTML table using data tables library to format it as sortable and pageable but it should only return 5,000 records from each table but it should include all of the columns that are on the table by default that was selected in the drop down box. All of the logic for this should be on an ASP.net page using .net 9.0 minimal, only give me the page code and it should be minimal.
```

### Assistant Response 1:
```csharp
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (HttpContext context) =>
{
    var credentials = new StoredProfileAWSCredentials("default");
    using var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);

    var tableList = await client.ListTablesAsync();
    var tables = tableList.TableNames;

    var html = $@"
        <html>
        <head>
            <script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>
            <link rel='stylesheet' href='https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css'>
            <script src='https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js'></script>
            <script>
                $(document).ready(function() {{
                    $('#tableSelect').change(function() {{
                        $.getJSON('/data?table=' + $(this).val(), function(data) {{
                            var headers = Object.keys(data[0] || {{}});
                            var thead = '<tr>' + headers.map(h => '<th>' + h + '</th>').join('') + '</tr>';
                            var tbody = data.map(row => '<tr>' + headers.map(h => '<td>' + (row[h] || '') + '</td>').join('') + '</tr>').join('');
                            $('#dataTable').html('<thead>' + thead + '</thead><tbody>' + tbody + '</tbody>').DataTable();
                        }});
                    }});
                }});
            </script>
        </head>
        <body>
            <select id='tableSelect'>
                <option>Select a Table</option>
                {string.Join("", tables.Select(t => $"<option value='{t}'>{t}</option>"))}
            </select>
            <table id='dataTable' class='display'></table>
        </body>
        </html>";

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.MapGet("/data", async ([FromQuery] string table) =>
{
    if (string.IsNullOrEmpty(table)) return Results.Json(new { error = "Table not specified" });

    var credentials = new StoredProfileAWSCredentials("default");
    using var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);

    var dynamoTable = Table.LoadTable(client, table);
    var scanFilter = new ScanFilter();
    var search = dynamoTable.Scan(scanFilter);
    
    var items = new List<Dictionary<string, object>>();
    int count = 0;
    
    while (!search.IsDone && count < 5000)
    {
        var batch = await search.GetNextSetAsync();
        foreach (var item in batch)
        {
            items.Add(item.ToAttributeMap().ToDictionary(k => k.Key, v => v.Value));
            count++;
            if (count >= 5000) break;
        }
    }

    return Results.Json(items);
});

app.Run();
```

---

### User Prompt 2:
```
Okay this is rad and I'm really impressed but it needs to be a little bit more human readable unless sophisticated can you make the HTML rendering happen on an ASP.net razr page file. You can also give me a code behind file and if you think it's appropriate give me a controller class for any JSON stuff, put the script in an @script block
```

### Assistant Response 2:

#### **1. Razor Page - `Index.cshtml`**
```razor
@page
@model IndexModel
@{
    ViewData["Title"] = "DynamoDB Browser";
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>@ViewData["Title"]</title>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css">
    <script src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>
</head>
<body>

    <h2>Select a DynamoDB Table</h2>
    <select id="tableSelect">
        <option value="">-- Select a Table --</option>
        @foreach (var table in Model.TableNames)
        {
            <option value="@table">@table</option>
        }
    </select>

    <table id="dataTable" class="display">
        <thead></thead>
        <tbody></tbody>
    </table>

    @section Scripts {
        <script>
            $(document).ready(function () {
                $('#tableSelect').change(function () {
                    let tableName = $(this).val();
                    if (!tableName) return;

                    $.getJSON('/api/dynamo/data?table=' + tableName, function (data) {
                        if (data.error) {
                            alert(data.error);
                            return;
                        }

                        let headers = Object.keys(data[0] || {});
                        let thead = '<tr>' + headers.map(h => `<th>${h}</th>`).join('') + '</tr>';
                        let tbody = data.map(row => '<tr>' + headers.map(h => `<td>${row[h] || ''}</td>`).join('') + '</tr>').join('');

                        $('#dataTable').html('<thead>' + thead + '</thead><tbody>' + tbody + '</tbody>').DataTable();
                    });
                });
            });
        </script>
    }
</body>
</html>
```
