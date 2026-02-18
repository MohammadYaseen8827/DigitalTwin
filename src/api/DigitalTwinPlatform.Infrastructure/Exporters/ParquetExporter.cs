
using DigitalTwinPlatform.Application.Abstractions.Services;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System.Collections;
using System.Reflection;

namespace DigitalTwinPlatform.Infrastructure.Exporters;

public class ParquetExporter : IExporter
{
    public async Task ExportAsync<T>(IEnumerable<T> data, string destinationPath, CancellationToken ct = default)
    {
        var dataList = data.ToList();
        if (dataList.Count == 0) return;

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        var fields = properties.Select(p => new DataField(p.Name, p.PropertyType)).ToArray();
        var schema = new ParquetSchema(fields);

        using var stream = File.Create(destinationPath);
        using var writer = await ParquetWriter.CreateAsync(schema, stream);

        using var groupWriter = writer.CreateRowGroup();
        foreach (var prop in properties)
        {
            var columnData = dataList.Select(d => prop.GetValue(d)).ToArray();
            
            // Handle specific types if needed, but Parquet.Net is fairly good at auto-handling
            var castedData = Array.CreateInstance(prop.PropertyType, dataList.Count);
            for (int i = 0; i < dataList.Count; i++)
            {
                castedData.SetValue(columnData[i], i);
            }

            var column = new DataColumn(fields.First(f => f.Name == prop.Name), castedData);
            await groupWriter.WriteColumnAsync(column);
        }
    }
}
