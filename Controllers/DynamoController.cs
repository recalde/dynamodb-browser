using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    [HttpGet("query")]
    public async Task<IActionResult> ExecuteQuery([FromQuery] string table, [FromQuery] string query)
    {
        if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(query))
            return BadRequest(new { error = "Table and query are required." });

        var dynamoTable = Table.LoadTable(_client, table);
        var scanFilter = new ScanFilter();

        // Parse multiple conditions (e.g., WHERE key = 'value' AND age > '30')
        var conditions = Regex.Matches(query, @"(\w+)\s*(=|>|<|>=|<=|CONTAINS)\s*'([^']+)'");
        if (conditions.Count == 0)
            return BadRequest(new { error = "Invalid query format. Example: SELECT * FROM table WHERE key = 'value' AND age > '30'" });

        foreach (Match condition in conditions)
        {
            string key = condition.Groups[1].Value;
            string op = condition.Groups[2].Value;
            string value = condition.Groups[3].Value;

            switch (op)
            {
                case "=":
                    scanFilter.AddCondition(key, ScanOperator.Equal, value);
                    break;
                case ">":
                    scanFilter.AddCondition(key, ScanOperator.GreaterThan, value);
                    break;
                case "<":
                    scanFilter.AddCondition(key, ScanOperator.LessThan, value);
                    break;
                case ">=":
                    scanFilter.AddCondition(key, ScanOperator.GreaterThanOrEqual, value);
                    break;
                case "<=":
                    scanFilter.AddCondition(key, ScanOperator.LessThanOrEqual, value);
                    break;
                case "CONTAINS":
                    scanFilter.AddCondition(key, ScanOperator.Contains, value);
                    break;
            }
        }

        var search = dynamoTable.Scan(scanFilter);
        var items = new List<Dictionary<string, object>>();

        while (!search.IsDone && items.Count < 5000)
        {
            items.AddRange(await search.GetNextSetAsync().ContinueWith(t => 
                t.Result.Select(item => item.ToAttributeMap().ToDictionary(k => k.Key, v => v.Value))
            ));
        }

        return Ok(items);
    }
}
