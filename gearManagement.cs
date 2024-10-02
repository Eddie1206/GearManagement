using System.Collections;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.Entity;
namespace gear;

//关于资源管理部分后期考虑使用using

class GearManagement
{
    //private Dictionary<string, int> gearInventory = new();      
    private const string connectionString = "Data Source=gearManagement.db;Version=3";
    private SQLiteConnection connection;

    public GearManagement()
    {
        //connection = new SQLiteConnection(connectionString);
        string databasePath = "gearManagement.db";
        if (!File.Exists(databasePath))
        {
            CreateDatabase(databasePath);
        }
        connection = new SQLiteConnection(connectionString);
    } 

    private void CreateDatabase(string databasePath)
    {
        try
        {
            SQLiteConnection.CreateFile(databasePath);
            Console.WriteLine($"Database created at {databasePath}.");
            using (SQLiteConnection tempConnection = new SQLiteConnection($"Data Source={databasePath}"))
            {
                tempConnection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS GearInventory(ID INTEGER PRIMARY KEY AUTOINCREMENT, Model TEXT, Quantity INTEGER)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, tempConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating database: {ex.Message}");
        }
    }
    public int AddGear(string model, int quantity)      //添加齿轮，成功返回最新数量，错误返回-1错误码
    {
        int state;
        try
        {
            connection.Open();
            if (quantity >= 0)
            {
                string checkQuery = $"SELECT Quantity FROM GearInventory WHERE Model = '{model}'";
                SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection);
                object result = checkCommand.ExecuteScalar();
                int existingQuantity = result == null ? 0 : Convert.ToInt32(result);
                int newQuantity = existingQuantity + quantity;
                string updateQuery = $"UPDATE GearInventory SET Quantity = {newQuantity} WHERE Model = '{model}'";

                if (existingQuantity == 0 && result == null)
                {
                    updateQuery = $"INSERT INTO GearInventory (Model, Quantity) VALUES ('{model}', {quantity})";
                }
                SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                command.ExecuteNonQuery();
                state =  newQuantity;
            }
            else
            {
                state = -1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding gear: {ex.Message}");
            state =  -1;
        }
        finally
        {
            connection.Close();
            
        }
        return state;
    }

    public int RemoveGear(string model, int quantity)      //如果可行减去齿轮数量，否则返回-1错误码
    {
        try
        {
            connection.Open();
            string checkQuery = $"SELECT Quantity FROM GearInventory WHERE Model = '{model}'";
            SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection);
            object result = checkCommand.ExecuteScalar();

            if (result == null || Convert.ToInt32(result) < quantity || quantity < 0)
            {
                return -1;
            }
            int newQuantity = Convert.ToInt32(result) - quantity;
            string updateQuery = $"UPDATE GearInventory SET Quantity = {newQuantity} WHERE Model = '{model}'";
            SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
            command.ExecuteNonQuery();
            return newQuantity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing gear: {ex.Message}");
            return -1;
        }
        finally
        {
            connection.Close();
        }
    }

    public int GetGearQuantity(string model)        //通过型号获得数量
    {
        try 
        {
            connection.Open();
            string query = $"SELECT Quantity FROM GearInventory WHERE Model = '{model}'";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            object result = command.ExecuteScalar();
            return result == null ? 0 : Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting gear quantity: {ex.Message}");
            return 0;
        }
        finally
        {
            connection.Close();
        }
    }

    public void DisplayInventory()      //打印库存
    {
        try
        {
            connection.Open();
            string query = "SELECT Model, Quantity FROM GearInventory";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            Console.WriteLine("Model\t\tQuantity");
            string horizontalLine = new('-', 50);
            Console.WriteLine(horizontalLine);
            while (reader.Read())
            {
                string model = reader.GetString(0);
                int quantity = reader.GetInt32(1);
                Console.WriteLine($"{model}\t\t{quantity}");
            }
            Console.WriteLine();
            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying inventory: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }

    public int AddItem(string model)        //增加齿轮型号，返回：0添加正常，1已存在，-1执行异常
    {
        int state = 0;
        try
        {
            connection.Open();
            string checkQuery = $"SELECT Quantity FROM GearInventory WHERE Model = '{model}'";
            SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection);
            object result = checkCommand.ExecuteScalar();
            if (result == null)
            {
                string updateQuery = $"INSERT INTO GearInventory (Model, Quantity) VALUES ('{model}', 0)";
                SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                command.ExecuteNonQuery();
                state = 0;
            }
            else
            {
                state = 1;
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding Item: {ex.Message}");
            state = -1;
        }
        finally
        {
            connection.Close();
        }
        return state;
    }

    public int DeleteItem(string model)     //删除齿轮型号，返回：0正常，-1异常
    {
        int state = 0;
        try
        {
            connection.Open();
            string deleteQuery = $"DELETE FROM GearInventory WHERE Model = '{model}'";
            using (SQLiteCommand command = new(deleteQuery, connection))
            {
                state = command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting: {ex.Message}");
            state = -1;
        }
        finally
        {
            connection.Close();
        }
        return state;
    }
}