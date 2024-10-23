using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using gear;
using Microsoft.Data.Sqlite;


namespace motor;

public struct MotorItem
{
    public string model;
    List<GearInfo> gearInfos;
}

public struct GearInfo : IComparable<GearInfo>
{
    public string model;
    public int quantity;
    public GearInfo(string model, int quantity)
    {
        this.model = model;
        this.quantity = quantity;
    }
    public int CompareTo(GearInfo other)
    {
        int compareModel = this.model.CompareTo(other.model);
        if (compareModel != 0)
        {
            return compareModel;
        }
        else
        {
            return this.quantity.CompareTo(other.quantity);
        }
    }
}

public struct SingleRecipe : IComparable<SingleRecipe>
{
    public string model;
    public string gears;
    public string nums;

    public SingleRecipe(string model, string gears, string nums)
    {
        this.model = model;
        this.gears = gears;
        this.nums = nums;
    }

    public int CompareTo(SingleRecipe other)
    {
        int modelCmp = this.model.CompareTo(other.model);
        int gearCmp = this.gears.CompareTo(other.gears);
        int numCmp = this.nums.CompareTo(other.nums);
        if (modelCmp == 0 && gearCmp == 0 & numCmp == 0)
        {
            return 0;
        }
        else
        {
            if (modelCmp == 0)
            {
                return gearCmp == 0 ? numCmp : gearCmp;
            }
            else
            {
                return modelCmp;
            }
        }
    }
}
class MotorRecipe
{   
    private const string connectionString = "Data Source=motorRecipe.db;Version=3";
    private SQLiteConnection connection;
    private GearManagement gearHub;
    private HashSet<SingleRecipe> recipe = new();
    public MotorRecipe(GearManagement gearHub)
    {
        //recipe逐个添加
        this.gearHub = gearHub;
        string databasePath = "motorRecipe.db";
        if (!File.Exists(databasePath))
        {
            CreateDatabase(databasePath);
        }
        connection = new SQLiteConnection(connectionString);
        InitHashSet();
    }

    private void CreateDatabase(string databasePath)
    {
        try
        {
            SQLiteConnection.CreateFile(databasePath);
            using (SQLiteConnection tempConnection = new($"Data Source={databasePath}"))
            {
                tempConnection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS MotorRecipe(ID INTEGER PRIMARY KEY AUTOINCREMENT, Model TEXT, Gears TEXT, Quantity TEXT)";
                using (SQLiteCommand command = new(createTableQuery, tempConnection))
                {
                    command.ExecuteNonQuery();
                    Debug.WriteLine($"Database created at: {databasePath}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating database: {ex.Message}");
        }
    }
    
    
    
    public void addRecipe(string model, ref List<GearInfo> gearInfos)
    {
        //添加配方
        //检查配方冗余
        //connection.close需要修改
        connection.Open();
        string gears = "";
        string nums = "";
        if (checkDuplicate(gearInfos))
        {
            Debug.WriteLine("失败：齿轮型号输入重复");
            return;
        }
        if (gearInfos != null)
        {
            gearInfos.Sort();
            foreach (GearInfo gearInfo in gearInfos)
            {
                gears += gearInfo.model + " ";
                nums += gearInfo.quantity + " ";
            }
        }

        if (!recipe.Add(new SingleRecipe(model, gears, nums)))
        {
            Console.WriteLine("配方已存在！");
            connection.Close();
            return;
        }
        string updateQuery = $"INSERT INTO MotorRecipe (Model, Gears, Quantity) VALUES ('{model}', '{gears}', '{nums}')";
        
        SQLiteCommand command = new(updateQuery, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }

    public void updateMotor_useModel(string model, ref List<GearInfo> gearInfos)
    {
        //通过模型号更新配方，该方法需配方唯一
        //淘汰方法
        connection.Open();
        string query = $"SELECT COUNT(*) FROM motorRecipe WHERE model = '{model}'";
        SQLiteCommand command = new(query, connection);
        object count = command.ExecuteScalar();
        string gears = "";
        string nums = "";
        if (gearInfos != null)
        {
            foreach (GearInfo gearInfo in gearInfos)
            {
                gears += gearInfo.model + " ";
                nums += gearInfo.quantity + " ";
            }
        }
        string updateQuery;
        if ((System.Int64)count == 0)
        {
            updateQuery = $"INSERT INTO MotorRecipe (Model, Gears, Quantity) VALUES ('{model}', '{gears}', '{nums}')";
        }
        else
        {
            updateQuery = $"UPDATE MotorRecipe SET Gears = '{gears}', Quantity = '{nums}' WHERE Model = '{model}'";
        }
        command = new(updateQuery, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }

    public void updateSingleRecipe(int id, ref List<GearInfo> gearInfos)
    {
        //通过id更新配方
        //判断是否包含重复并sort
        int state = 0;
        connection.Open();
        string query = $"SELECT Model FROM motorRecipe WHERE ID = '{id}'";
        SQLiteCommand command = new(query, connection);
        object model = command.ExecuteScalar();
        string gears = "";
        string nums = "";
        if (gearInfos != null)
        {
            gearInfos.Sort();
            foreach (GearInfo gearInfo in gearInfos)
            {
                gears += gearInfo.model + " ";
                nums += gearInfo.quantity + " ";
            }
        }
        string updateQuery;
        if (model == null)
        {
            updateQuery = $"INSERT INTO MotorRecipe (Model, Gears, Quantity) VALUES ('{model}', '{gears}', '{nums}')";
        }
        else
        {
            updateQuery = $"UPDATE MotorRecipe SET Gears = '{gears}', Quantity = '{nums}' WHERE ID = '{id}'";
        }
        command = new(updateQuery, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }

    public int DeleteItem_useModel(string model)     //删除Motor型号，返回：0正常，-1异常
    {
        //淘汰方法
        int state = 0;
        try
        {
            connection.Open();
            string deleteQuery = $"DELETE FROM MotorRecipe WHERE Model = '{model}'";
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

    
    public int DeleteItem(int id)     //删除Motor型号，返回：0正常，-1异常
    {
        //！！当前方法存在问题，结束后再执行ExecuteNonQuery会出现问题
        int state = 0;
        try
        {
            connection.Open();
            string query = $"SELECT Model, Gears, Quantity FROM MotorRecipe WHERE ID = '{id}'";
            using (SQLiteCommand command = new(query, connection))
            {
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string model = reader.GetString(0);
                    string gears = reader.GetString(1);
                    string nums = reader.GetString(2);
                }
            }
            string deleteQuery = $"DELETE FROM MotorRecipe WHERE ID = '{id}'";
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
    
    public int DeleteItemAdo(int id)
    {
        string connectionString = $"Data Source=motorRecipe.db";
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                @"SELECT Model, Gears, Quantity FROM MotorRecipe WHERE ID = $id";
            command.Parameters.AddWithValue("$id", id);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var gears = reader.GetString(1);
                    var nums = reader.GetString(2);
                    recipe.Remove(new SingleRecipe(name, gears, nums));
                }
            }
        }
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"DELETE FROM MotorRecipe WHERE ID = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        return 0;
    }
    public void DisplayRecipe()      //打印配方
    {
        //打印现有配方
        //HashSet当前在此添加
        try
        {
            connection.Open();
            string query = "SELECT ID, Model, Gears, Quantity FROM MotorRecipe";
            SQLiteCommand command = new(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            //Console.WriteLine("Model\t\tGear\t\tQuantity");
            Console.WriteLine();
            Console.WriteLine("Recipe:");
            string horizontalLine = new('-', 50);
            Console.WriteLine(horizontalLine);
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string model = reader.GetString(1);
                string gears = reader.GetString(2);
                string quantity = reader.GetString(3);
                //recipe.Add(new SingleRecipe(model, gears, quantity));
                Console.WriteLine($"{model}: -> {id}\n{gears}\n{quantity}\n");
            }
            Console.WriteLine();
            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying recipe: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }

    public bool Manufact(int id, int num)
    {
        //生产消耗齿轮
        bool enough = IsEnough(id, num);
        if (!enough)
        {
            return false;
        }
        try
        {
            connection.Open();
            string query = $"SELECT Gears, Quantity From motorRecipe WHERE ID = '{id}'";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            string gears = "";
            string quantity = "";
            while (reader.Read())
            {
                gears = reader.GetString(0);
                quantity = reader.GetString(1);
            }
            gears = gears.TrimEnd();
            quantity = quantity.TrimEnd();
            string[] gearA = gears.Split(" ");
            string[] quantityA = quantity.Split(" ");
            int[] quantityInt = new int[quantityA.Length];
            for (int i = 0; i < quantityA.Length; i++)
            {
                quantityInt[i] = int.Parse(quantityA[i]);
            }
            
            for (int i = 0; i < quantityA.Length; i++)
            {
                gearHub.RemoveGear(gearA[i], quantityInt[i] * num);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Manufact error: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }     
        return true;
    }
    private bool IsEnough(int id, int num)
    {
        //添加齿轮是否都存在检查
        try
        {
            connection.Open();
            string query = $"SELECT Gears, Quantity From MotorRecipe WHERE ID = '{id}'";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            string gears = "";
            string quantity = "";
            while (reader.Read())
            {
                gears = reader.GetString(0);
                quantity = reader.GetString(1);
            }
            gears = gears.TrimEnd(' ');
            quantity = quantity.TrimEnd(' ');
            string[] gearA = gears.Split(" ");
            string[] quantityA = quantity.Split(" ");
            int[] quantityInt = new int[quantityA.Length];
            for (int i = 0; i < quantityA.Length; i++)
            {
                quantityInt[i] = int.Parse(quantityA[i]);
            }

            bool enough = true;
            for (int i = 0; i < quantityA.Length; i++)
            {
                if (gearHub.GetGearQuantity(gearA[i]) < quantityInt[i] * num)
                {
                    enough = false;
                    break;
                }
            }
            return enough;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Check Inv Error: {ex.Message}");
            return false;
        }
        finally
        {
            connection.Close();
        }
    }

    private void sortStruct(ref List<GearInfo> gearInfos)
    {
        gearInfos.Sort();
    }

    private bool checkDuplicate(List<GearInfo> gearInfos)
    {   
        //检查是否包含重复项，true包含，false不含
        HashSet<string> set = new();
        foreach(var item in gearInfos)
        {
            if(!set.Add(item.model))
            {
                return true;
            }
        }
        return false;
    }

    private void InitHashSet()
    {
        try
        {
            connection.Open();
            string query = "SELECT Model, Gears, Quantity FROM MotorRecipe";
            SQLiteCommand command = new(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            //Console.WriteLine("Model\t\tGear\t\tQuantity");
            Console.WriteLine();
            Console.WriteLine("Recipe:");
            string horizontalLine = new('-', 50);
            Console.WriteLine(horizontalLine);
            while (reader.Read())
            {
                string model = reader.GetString(0);
                string gears = reader.GetString(1);
                string quantity = reader.GetString(2);
                recipe.Add(new SingleRecipe(model, gears, quantity));
            }
            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error InitHashSet: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }
}

