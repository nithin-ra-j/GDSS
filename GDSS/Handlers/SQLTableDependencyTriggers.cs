using GDSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.EventArgs;

namespace GDSS.Handlers
{
    public class SQLTableDependencyTriggers
    {
        private static SqlTableDependency<Poll> pollDependency;
        private static SqlTableDependency<Forum> forumDependency;

        public SQLTableDependencyTriggers()
        {
            string connectionString = "data source=(LocalDB)\\MSSQLLocalDB;attachdbfilename=|DataDirectory|\\GDSS_DB.mdf;integrated security=True;connect timeout=30;MultipleActiveResultSets=True;App=EntityFramework";
            UpdateOfModel<Poll> updateOFPoll = new UpdateOfModel<Poll>();
            updateOFPoll.Add(x => x.Status);
            pollDependency = new SqlTableDependency<Poll>(connectionString, updateOf: updateOFPoll);
            pollDependency.OnChanged += Poll_OnChanged;
            pollDependency.Start();

            UpdateOfModel<Forum> updateOfForum = new UpdateOfModel<Forum>();
            updateOfForum.Add(x => x.Status);
            forumDependency = new SqlTableDependency<Forum>(connectionString, updateOf: updateOfForum);
            forumDependency.OnChanged += Forum_OnChanged;
            forumDependency.Start();
        }

        public static void Poll_OnChanged(object sender, RecordChangedEventArgs<Poll> e)
        {
            Poll poll = e.Entity;
            if (poll.Status == "Active")
                MyHub.GetTool(poll,null);
        }

        public static void Forum_OnChanged(object sender, RecordChangedEventArgs<Forum> e)
        {
            Forum forum = e.Entity;
            if (forum.Status == "Active")
                MyHub.GetTool(null, forum);
        }

        ~SQLTableDependencyTriggers()
        {
            pollDependency.Stop();
            forumDependency.Stop();
        }

    }
}