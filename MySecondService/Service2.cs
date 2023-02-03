using MSMQ.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MySecondService
{
    public partial class Service2 : ServiceBase
    {
        Timer timer = new Timer();
        public static string path = @".\Private$\MyFirstQueue";
        MessageQueue queue;
        public static string checkPath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile";
        public static string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

        public Service2()
        {
            InitializeComponent();
            queue = new MessageQueue(path);
            queue.MessageReadPropertyFilter.SetAll();

        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 10000; //time interval in milliseconds (10Sec) 
            timer.Enabled = true;
        }

        protected override void OnStop()
        {

        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //WriteTextToFile("Service recalled at " + DateTime.Now);
            File.WriteAllText(filepath,String.Empty);
            GetAllMessagesFromQueue(queue);
        }

        public static void GetAllMessagesFromQueue(MessageQueue queue)
        {
            queue.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            Message[] msgs = queue.GetAllMessages();
            foreach (Message msg in msgs)
            {
                Person p = JsonConvert.DeserializeObject<Person>(msg.Body.ToString());
                WriteTextToFile(p.FirstName + "----"+p.LastName+"----"+p.Occupation);
            }
            queue.Close();
        }

        public static void WriteTextToFile(string Message)
        {
            //string checkPath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile";
            if (!Directory.Exists(checkPath))
            {
                Directory.CreateDirectory(checkPath);
            }
            //string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                //Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
