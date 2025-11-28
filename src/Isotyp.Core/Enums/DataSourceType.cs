namespace Isotyp.Core.Enums;

/// <summary>
/// Types of data sources that can be connected.
/// </summary>
public enum DataSourceType
{
    /// <summary>
    /// SQL Server database.
    /// </summary>
    SqlServer = 0,

    /// <summary>
    /// PostgreSQL database.
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// MySQL database.
    /// </summary>
    MySql = 2,

    /// <summary>
    /// SQLite database.
    /// </summary>
    Sqlite = 3,

    /// <summary>
    /// CSV file.
    /// </summary>
    CsvFile = 4,

    /// <summary>
    /// JSON file.
    /// </summary>
    JsonFile = 5,

    /// <summary>
    /// REST API endpoint.
    /// </summary>
    RestApi = 6,

    /// <summary>
    /// Cloud storage (Azure Blob, S3, etc.).
    /// </summary>
    CloudStorage = 7
}
