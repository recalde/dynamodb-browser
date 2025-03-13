using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    public List<string> TableNames { get; set; } = new();

    public async Task OnGetAsync()
    {
        var credentials = new StoredProfileAWSCredentials("default");
        using var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        
        var response = await client.ListTablesAsync();
        TableNames = response.TableNames;
    }
}