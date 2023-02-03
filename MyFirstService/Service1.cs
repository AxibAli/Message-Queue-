using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Messaging;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MyFirstService
{
    public partial class Service1 : ServiceBase
    {

        Timer timer = new Timer();
        public static string path = @".\Private$\MyFirstQueue";
        MessageQueue queue;
        
        public Service1()
        {
            InitializeComponent();
            queue = new MessageQueue(path);
            queue.MessageReadPropertyFilter.SetAll();
        }

        protected override void OnStart(string[] args)
        {
            //WriteTextToFile("Service started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 10000; //time interval in milliseconds (10Sec) 
            timer.Enabled = true;
            //ParseCsv();
        }

        protected override void OnStop()
        {
            //WriteTextToFile("Service stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //WriteTextToFile("Service recalled at " + DateTime.Now);
            DeleteAllTheMessagesFromQueue(queue);
            ParseCsv();            
        }

        //public void WriteTextToFile(string Message)
        //{
        //    string checkPath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile";
        //    if (!Directory.Exists(checkPath))
        //    {
        //        Directory.CreateDirectory(checkPath);
        //    }
        //    string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
        //    if (!File.Exists(filepath))
        //    {
        //        //Create a file to write to.   
        //        using (StreamWriter sw = File.CreateText(filepath))
        //        {
        //            sw.WriteLine(Message);
        //        }
        //    }
        //    else
        //    {
        //        using (StreamWriter sw = File.AppendText(filepath))
        //        {
        //            sw.WriteLine(Message);
        //        }
        //    }
        //}

        public void ParseCsv()
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + "\\filePersons.csv";
            using (var reader = new StreamReader(url))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Person>();
                foreach (var item in records)
                {
                    //WriteTextToFile(item.FirstName+"----"+item.LastName+"----"+item.Occupation);
                    string data = JsonConvert.SerializeObject(item);
                    SendMessageToMessageQueue(data);
                }
            }
        }

        public void SendMessageToMessageQueue(string Message)
        {
            CheckIfQueueExsits(path);
            SendAMessage(Message, queue);
        }

        public static void CheckIfQueueExsits(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                MessageQueue.Create(path);
            }
        }

        public static void SendAMessage(string msg, MessageQueue queue)
        {
            queue.Send(msg, msg);
            queue.Close();
        }

        public static void DeleteAllTheMessagesFromQueue(MessageQueue queue)
        {
            queue.Purge();
            queue.Close();
        }

    }
}