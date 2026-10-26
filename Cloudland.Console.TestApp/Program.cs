// اتصال به دیتابیس

using Cloudland.APM.Utils.DQL;
using Microsoft.Data.SqlClient;

var connection =
    new SqlConnection(
        "Server=mssql.maysaservice.ir;Database=Test;User Id=sa;Password=Kabinet95##;Integrated Security=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
var metaProvider = new DatabaseMetadataProvider(connection);
metaProvider.LoadMetadata();

var tables = metaProvider.Tables;

// پردازش پرس و جوی محاوره‌ای فارسی
var parser = new QueryTemplateParser(tables);
var queryDto = parser.Parse("لیست همه تست");

var builder = new QueryBuilderEngine(tables);
var sql = builder.Build(queryDto);

var executor = new QueryExecutor(connection);
var results = executor.Execute(sql);

// نمایش نتایج
foreach (var row in results)
{
    // نمایش داده
}