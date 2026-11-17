using Microsoft.Extensions.Configuration;
using Npgsql;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

//Get connection String from the json file
string connString = config.GetConnectionString("Postgres")!;

//Defines which database to connect to
await using var dataSource = NpgsqlDataSource.Create(connString);

/*
ExecuteNonQueryAsync: executes SQL which doesn't return
any results, typically INSERT, UPDATE or DELETE statements. 
Returns the number of rows affected.
ExecuteScalarAsync: executes SQL which returns a single, 
scalar value.
ExecuteReaderAsync: execute SQL which returns a full resultset.
Returns an NpgsqlDataReader which can be used to access the 
resultset.
 */

//READ
//Creates a command
await using (var cmd = dataSource.CreateCommand(
    "SELECT id, name, course, grade FROM students ORDER BY id"))
//Executes the command
await using (var reader = await cmd.ExecuteReaderAsync())
{
    //Reads each line of data
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"{reader.GetInt32(0),-3} {reader.GetString(1),-15} {reader.GetString(2),-25} {(reader.IsDBNull(3) ? null : reader.GetDecimal(3))}");
    }
}

//INSERT
await using (var cmd = dataSource.CreateCommand(
    "INSERT INTO students (name, course, grade) VALUES (@name, @course, @grade) RETURNING id"))
{
    cmd.Parameters.AddWithValue("name", "Joaquin Herrera");
    cmd.Parameters.AddWithValue("course", "BS Data Science");
    cmd.Parameters.AddWithValue("grade", 1.50m);

    /*
     Parameters =
    {
        new("name", NpgsqlDbType.Varchar, 100) { Value = "Joaquin Herrera" },
        new("course", NpgsqlDbType.Varchar, 100) { Value = "BS Data Science" },
        new("grade", NpgsqlDbType.Numeric) { Value = 1.50m }
    }
     */

    int newID = (int)(await cmd.ExecuteScalarAsync());
    Console.WriteLine($"Inserted student with id {newID}");
}

//UPDATE
await using (var cmd = dataSource.CreateCommand(
     "UPDATE students SET grade = @grade WHERE name = @name"))
{
    cmd.Parameters.AddWithValue("grade", 1.25m);
    cmd.Parameters.AddWithValue("name", "Joaquin Herrera");

    int rows = await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"Rows updated/affected: {rows}");
}

//READ AGAIN
await using (var cmd = dataSource.CreateCommand(
    "SELECT id, name, course, grade FROM students ORDER BY id"))
await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"{reader.GetInt32(0),-3} {reader.GetString(1),-15} {reader.GetString(2),-25} {(reader.IsDBNull(3) ? null : reader.GetDecimal(3))}");
    }
}