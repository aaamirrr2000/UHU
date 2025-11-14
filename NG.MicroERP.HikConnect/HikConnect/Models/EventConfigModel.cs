using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikConnect.Models;

public class EventConfigModel
{
    public int Id { get; set; } = 0;
    public int Timer_Seconds { get; set; } = 60;
    public int StartStop { get; set; }
    public int Email_Log { get; set; }
    public int Historical_Days { get; set; } = 30;
}
