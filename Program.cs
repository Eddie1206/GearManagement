// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Formats.Tar;
using gear;
//Console.WriteLine("Hello, World!");
// motor motor1 = new motor("A1");
// motor1.add_gear(1);
// motor1.add_gear(3);
// //motor1.show_gear();
// ArrayList gear = motor1.get_gear();


// for (int i = 0; i < gear.Count; i++)
// {
//     System.Console.WriteLine("gear: " + gear[i]);
// }
// System.Console.WriteLine(motor1.getName());

GearManagement gm = new();
while (true) {
    Console.WriteLine("Input parametes: [args]int string int");
    Console.WriteLine();
    Console.WriteLine("arg0[int]:\t[-1]Exit\n\t\t[0]ShowEnventory\n\t\t[1]AddEventory\t\tneed to serve more info\n\t\t[2]RemoveEnventory\tneed to serve more info");
    //Console.WriteLine("if arg0 is 1 or 2, need to serve more info");
    Console.WriteLine();
    Console.WriteLine("arg1[sting]:\tnameOfModel\n\narg2[int]:\tQuantity\n");
    string? read = Console.ReadLine();
    string[] arg = read.Split(" ");
    //Console.WriteLine(arg.Count());

    if (arg.Length != 3 && arg.Length != 1)
    {
        Console.WriteLine("Error parameter! Please reinput!\n");
        continue;
    }
    bool checkArg = true;
    int cmd, quantity = 0;
    checkArg = int.TryParse(arg[0], out cmd);
    if (checkArg && (cmd < -1 || cmd > 2))
    {
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
        Console.WriteLine("Error parameter! Please reinput!\n");
        continue;
    }

    if (cmd == -1)
    {
        break;
    }else if(cmd == 0){
        Console.WriteLine("Inventory Details: \n");
        gm.DisplayInventory();
    }else if(cmd == 1){
        gm.AddGear(arg[1], quantity);
        Console.WriteLine($"Succeed added {quantity} {arg[1]}!");
    }else if(cmd == 2){
        int act = gm.RemoveGear(arg[1], quantity);
        if (act != -1) 
        {
            Console.WriteLine($"Succeed remove {quantity} {arg[1]}!");
        }
        else
        {
            Console.WriteLine("Operation Failed!");
        }
    }

}



// gm.AddOrUpdateGear("Model A", 50);
// gm.AddOrUpdateGear("Model B", 100);
// gm.RemoveGear("Model A", 30);
// gm.DisplayInventory();