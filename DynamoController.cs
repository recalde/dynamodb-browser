using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/dynamo")]
[ApiController]
public class DynamoController : ControllerBase
{
    private readonly AmazonDynamoDBClient _client;

    public DynamoController()
    {
        var credentials = new StoredProfileAWSCredentials("default");
        _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetTableData([FromQuery] string table)
    {
        if (string.IsNullOrEmpty(table))
            return BadRequest(new { error = "Table not specified" });

        var dynamoTable = Table.LoadTable(_client, table);
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

        return Ok(items);
    }
}