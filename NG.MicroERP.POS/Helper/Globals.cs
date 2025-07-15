using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.POS.Helper;

public class Globals
{
    public static string FullName = "";
    public static string UserName = "";
    public static string ComputerAlias = "";
    public static string LocationName = "";
    public static long LocationID = 0;
    public static string MachineID = "";
    public static long ComputerID = 0;
    public static string ComanyName = "";
    public static int GroupID = 0;
    public static int UserID = 0;
    public static bool bNew = false;
    public static bool bPin = false;
    public static string sSQL = "";
    public static int SubCategoryLevel = 0;
    public static string Theme = "Pumpkin";
    public static byte[] MyImage;
    public static bool bExpandCollapse = true;
    public static int POSFormNumber = 0;
    public static long ID = 0;
    public static string DefaultFilePath = FileSystem.CurDir();
    public static long TileID = 0;
    public static string POSPrinterName = "";
    public static string NormalPrinterName = "";
    public static string COMPort = "";
    public static string[] arraylist = null;
    public static int totaltax = 0;
    public static DateTime Expiry;
}
