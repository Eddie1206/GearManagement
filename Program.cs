//#define ENGLISH     //注释该行以加载中文模式

// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Configuration;
using System.Drawing;
using System.Formats.Tar;
using gear;

#if ENGLISH
    const string inputHint = "Input parametes: [args]int string int";
    const string arg0 = "arg0[int]:\t[-1]Exit\n\t\t[0]ShowEnventory\n\t\t[1]AddEventory\t\tneed to serve more info\n\t\t[2]RemoveEnventory\tneed to serve more info";
    const string arg1 = "arg1[string]:\tnameOfModel\n\narg2[int]:\tQuantity\n";
    const string inputHint2 = "\\/COMMAND INPUT HERE\\/";
    const string ErrInput = "Error parameter! Please reinput!\n";
    const string showInv = "Inventory Details: \n";
    const string addedOK = "Succeed added ";
    const string removedOK = "Succeed remove ";
    const string opFailed = "Operation Failed";
#else
    const string inputHint = "请输入参数: [args]int string int";
    const string arg0 = "参数0[int]:\t[-1]退出\n\t\t[0]显示库存\n\t\t[1]增加库存\t需要提供[参数2:数量]\n\t\t[2]移除库存\t需要提供[参数2:数量]";
    const string arg1 = "参数1[string]:\t型号\n\n参数2[int]:\t数量\t<-参数0为1、2时需要提供此参数\n";
    const string inputHint2 = "\\/在此输入指令\\/";
    const string ErrInput = "参数错误! 请重新输入!\n";
    const string showInv = "库存详情: \n";
    const string addedOK = "成功添加 ";
    const string removedOK = "成功移除 ";
    const string opFailed = "操作失败";
#endif


void ColorfulWrite(string str, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(str);
    Console.ResetColor();
}
GearManagement gm = new();
while (true) {
    Console.WriteLine();

    ColorfulWrite(inputHint, ConsoleColor.Blue);
    
    Console.WriteLine();
    Console.WriteLine(arg0);
    //Console.WriteLine("if arg0 is 1 or 2, need to serve more info");
    Console.WriteLine();
    Console.WriteLine(arg1);
    ColorfulWrite(inputHint2, ConsoleColor.Blue);
    string? read = Console.ReadLine();
    string[] arg = read.Split(" ");
    Console.WriteLine();
    //Console.WriteLine(arg.Count());

    if (arg.Length != 3 && arg.Length != 1)
    {
        ColorfulWrite(ErrInput, ConsoleColor.Red);
        continue;
    }
    bool checkArg = true;
    int cmd, quantity = 0;
    checkArg = int.TryParse(arg[0], out cmd);
    if (checkArg && (cmd < -1 || cmd > 4))      //输入检查，由于添加DeleteItem此处接受cmd == 3。使用cmd == 3时需额外提供一参数以逃逸检查。
    {                                           //该使用方式仅为测试暂时增添
        checkArg = false;
    }
    if (checkArg && arg.Length > 1)
    {
        checkArg = int.TryParse(arg[2], out quantity);
        if (quantity < 0)
        {
            checkArg = false;
        }
    }
    if (checkArg && cmd > 0 && arg.Length != 3)
    {
        checkArg = false;
    }

    if (!checkArg)
    {
        ColorfulWrite(ErrInput, ConsoleColor.Red);
        continue;
    }

    if (cmd == -1)
    {
        break;
    }else if(cmd == 0){
        ColorfulWrite(showInv, ConsoleColor.Green);
        //gm.DisplayInventory();
        List<InventoryItem> inventory = gm.GetInventory();
        Console.WriteLine("Model\t\tQuantity");
        string horizontalLine = new('-', 50);
        Console.WriteLine(horizontalLine);
        foreach (var item in inventory)
        {
            Console.WriteLine($"{item.model}\t\t{item.quantity}");
        }
    }else if(cmd == 1){
        int act = gm.AddGear(arg[1], quantity);
        if (act != -1)
        {
            ColorfulWrite(addedOK + $"{quantity} {arg[1]}!", ConsoleColor.Green);
        }
        else
        {
            ColorfulWrite(opFailed, ConsoleColor.Red);
        }
    }else if(cmd == 2){
        int act = gm.RemoveGear(arg[1], quantity);
        if (act != -1) 
        {
            ColorfulWrite(removedOK + $"{quantity} {arg[1]}!", ConsoleColor.Green);
        }
        else
        {
            ColorfulWrite(opFailed, ConsoleColor.Red);
        }
    }else if(cmd == 3){
        int act = gm.DeleteItem(arg[1]);
        if (act == 0)
        {
            ColorfulWrite($"No Model found: {arg[1]}", ConsoleColor.Red);
        }
        else if (act == -1)
        {
            ColorfulWrite("Operation Error!", ConsoleColor.Red);
        }
        else if (act > 0)
        {
            ColorfulWrite($"Successfully deleted {arg[1]}", ConsoleColor.Green);
        }
    }else if (cmd == 4){
        int act = gm.AddItem(arg[1]);
        if (act == 0)
        {
            ColorfulWrite($"Successfully added {arg[1]}", ConsoleColor.Green);
        }
        else if (act == 1)
        {
            ColorfulWrite($"Already exist {arg[1]}", ConsoleColor.Red);
        }
        else
        {
            ColorfulWrite(opFailed, ConsoleColor.Red);
        }
    }

}
