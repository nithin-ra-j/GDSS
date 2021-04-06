using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GDSS.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TableDependency.SqlClient.Base.EventArgs;

namespace GDSS.Handlers
{
    [HubName("MyHub")]
    public class MyHub : Hub
    {
        [HubMethodName("GetTool")]
        public static void GetTool(Poll poll, Forum forum)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            if (poll != null)
                context.Clients.All.updatePoll();
            else if (forum != null)
                context.Clients.All.updateForum();
        }
    }
}