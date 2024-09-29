using System.Collections;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using System.Data.SQLite;
using System.Data.SqlClient;
namespace gear;
class GearManagement
{
    private Dictionary<string, int> gearInventory = new();      
    private string connectionString = "Data Source=gearManagement.db;Version=3";
    private SQLiteConnection connection;

    public GearManagement()
    {
        connection = new SQLiteConnection(connectionString);
    } 
    public int AddGear(string model, int quantity)      //添加齿轮，成功返回最新数量，错误返回-1错误码
    {
        if (quantity >= 0)
        {
            if (gearInventory.ContainsKey(model))
            {
                gearInventory[model] += quantity;
            }else{
                gearInventory[model] = quantity;
            }
            return gearInventory[model];
        }
        else
        {
            return -1;
        }
        
    }

    public int RemoveGear(string model, int quantity)      //如果可行减去齿轮数量，否则返回-1错误码
    {
        if (gearInventory.ContainsKey(model) && gearInventory[model] >= quantity && quantity >= 0)
        {
            gearInventory[model] -= quantity;

            return quantity;
        }
        else
        {
            return -1;
        }
    }

    public int GetGearQuantity(string model)        //通过型号获得数量
    {
        return gearInventory.ContainsKey(model) ? gearInventory[model] : 0;
    }

    public void DisplayInventory()      //打印库存
    {
        Console.WriteLine("Model\t\tQuantity");
        string horizontalLine = new('-', 50);
        Console.WriteLine(horizontalLine);
        foreach (var gear in gearInventory)
        {
            Console.WriteLine($"{gear.Key}\t\t{gear.Value}");
        }
        Console.WriteLine();
    }
}