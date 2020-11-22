using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
namespace lab3forms
{
    public partial class Form1 : Form
    {
        TreeView prevTree;
        TreeView newTree;
        static string IP;
        string IP01;
        string path01;
        private XmlNode validationEventHandler;

        public Form1()
        {
            InitializeComponent();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            
            Start();
        }
        private async void Start()
        {
            while (true)
            {
                List<int> monthSizesForDownload = new List<int>();
                List<int> daySizesForDownload = new List<int>();
                List<int> files = new List<int>();
                int yearSize = 0;
                List<int> monthSizes = new List<int>();
                List<int> monthSizesSave = new List<int>();
                List<int> daySizes = new List<int>();



                LoadConfigs();


                IP = textBox1.Text + ":" + textBox2.Text;           
                IP = "ftp://" + IP + IP01;



                FtpWebRequest request = null;
                FtpWebResponse response = null;
                Stream responseStream = null;
                StreamReader reader = null;
                try
                {
                    request = (FtpWebRequest)WebRequest.Create(IP);
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    response = (FtpWebResponse)request.GetResponse();
                    responseStream = response.GetResponseStream();
                    reader = new StreamReader(responseStream);
                }
                catch
                {
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    label3.Text = "Возникла ошибка";
                    return;
                }
                
               
                
               

                while (!reader.EndOfStream)
                {
                    yearSize++;
                    treeView1.Nodes.Add(new TreeNode(reader.ReadLine()));
                }
                for (int i = 0; i < yearSize; i++)
                {
                    int size = 0;
                    TreeNode treeNode = treeView1.Nodes[i];
                    string IP0 = IP + treeNode.Text + "/";
                    FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(IP0);
                    request1.Method = WebRequestMethods.Ftp.ListDirectory;
                    FtpWebResponse response1 = (FtpWebResponse)request1.GetResponse();
                    Stream responseStream1 = response1.GetResponseStream();
                    StreamReader reader1 = new StreamReader(responseStream1);
                    while (!reader1.EndOfStream)
                    {
                        treeNode.Nodes.Add(new TreeNode(reader1.ReadLine()));
                        size++;
                    }
                    monthSizes.Add(size);
                }
                monthSizesForDownload = CopyList(monthSizes);
                monthSizesSave = CopyList(monthSizes);
                for (int i = 0; i < yearSize; i++)
                {
                    for (int j = 0; j < monthSizes.First(); j++)
                    {
                        int size = 0;
                        TreeNode treeNode = treeView1.Nodes[i];
                        string IP0 = IP + treeNode.Text + "/";
                        treeNode = treeNode.Nodes[j];
                        IP0 = IP0 + treeNode.Text + "/";
                        FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(IP0);
                        request1.Method = WebRequestMethods.Ftp.ListDirectory;
                        FtpWebResponse response1 = (FtpWebResponse)request1.GetResponse();
                        Stream responseStream1 = response1.GetResponseStream();
                        StreamReader reader1 = new StreamReader(responseStream1);
                        while (!reader1.EndOfStream)
                        {
                            treeNode.Nodes.Add(new TreeNode(reader1.ReadLine()));
                            size++;
                        }
                        daySizes.Add(size);
                    }
                    monthSizes.Remove(monthSizes.First());
                }
                daySizesForDownload = CopyList(daySizes);
                
                for (int i = 0; i < yearSize; i++)
                {
                    for (int j = 0; j < monthSizesSave.First(); j++)
                    {
                        for (int k = 0; k < daySizes.First(); k++)
                        {
                            int size = 0;
                            TreeNode treeNode = treeView1.Nodes[i];
                            string IP0 = IP + treeNode.Text + "/";
                            treeNode = treeNode.Nodes[j];
                            IP0 = IP0 + treeNode.Text + "/";
                            treeNode = treeNode.Nodes[k];
                            IP0 = IP0 + treeNode.Text + "/";
                            FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(IP0);
                            request1.Method = WebRequestMethods.Ftp.ListDirectory;
                            FtpWebResponse response1 = (FtpWebResponse)request1.GetResponse();
                            Stream responseStream1 = response1.GetResponseStream();
                            StreamReader reader1 = new StreamReader(responseStream1);
                            while (!reader1.EndOfStream)
                            {
                                treeNode.Nodes.Add(new TreeNode(reader1.ReadLine()));
                                size++;
                            }
                            files.Add(size);
                        }
                        daySizes.Remove(daySizes.First());
                    }
                    monthSizesSave.Remove(monthSizesSave.First());
                }
                newTree = treeView1;
                if(newTree!=prevTree)
                {
                    prevTree = newTree;
                    MakeDirs(monthSizesForDownload, daySizesForDownload, yearSize);
                    DownloadFolder(IP, monthSizesForDownload, daySizesForDownload, yearSize, files);
                    if (Directory.Exists("C://lab2/downloads/lab2"))
                    {
                        Directory.Delete("C://lab2/downloads/lab2");
                    }
                        
                }
             

                await Task.Delay(10000);
                treeView1.Nodes.Clear();
            }
        }
        public void LoadConfigs()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, "val.xsd");
            xDoc.Load("config.xml");

           
            XmlElement xroot = xDoc.DocumentElement;

            foreach (XmlElement xnode in xroot)
            {
                XmlNode attr = xnode.Attributes.GetNamedItem("prog");
                foreach (XmlNode xmlNode in xnode.ChildNodes)
                {
                    if (xmlNode.Name == "IP")
                    {
                        IP01 = xmlNode.InnerText;
                    }
                    else
                    {
                        path01 = xmlNode.InnerText;
                    }
                }
            }
            return;
        }
        public void MakeDirs(List<int> months, List<int> days, int years)
        {
            List<int> m = CopyList(months);
            List<int> d = CopyList(days);
            
            
            string path = path01;
            string path1;
            string path2;
            string path3;
            for (int i = 0; i < years; i++)
            {
                TreeNode treenode = treeView1.Nodes[i];
                path1 = path + treenode.Text + '/';
                if(!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }

                for(int j = 0; j < m.First(); j++)
                {
                    TreeNode treenode1 = treeView1.Nodes[i].Nodes[j];
                    path2 = path1 + treenode1.Text + '/';
                    if (!Directory.Exists(path2))
                    {
                        Directory.CreateDirectory(path2);
                    }
                    for(int k = 0; k < d.First(); k++)
                    {
                        TreeNode treenode2 = treeView1.Nodes[i].Nodes[j].Nodes[k];
                        path3 = path2 + treenode2.Text + '/';
                        if (!Directory.Exists(path3))
                        {
                            Directory.CreateDirectory(path3);
                        }
                    }
                    d.Remove(d.First());
                }
                m.Remove(m.First());
            }
            return;
        }
        public void DownloadFolder(string IP, List<int> months, List<int> days, int years, List<int> files)
        {
            List<int> f = CopyList(files);
            List<int> d = CopyList(days);
            List<int> m = CopyList(months);
            string path = "C://lab2/";
            string path1;
            string path2;
            string IP1;
            string IP2;
            for(int i = 0; i < years; i++)
            {
                for(int j = 0; j < m.First(); j++)
                {
                    for(int k = 0; k < d.First(); k++)
                    {
                        TreeNode treenode = treeView1.Nodes[i];
                        path1 = path + treenode.Text + "/";
                        IP1 = IP + treenode.Text + "/";
                        treenode = treenode.Nodes[j];
                        path1 = path1 + treenode.Text + "/";
                        IP1 = IP1 + treenode.Text + "/";
                        treenode = treenode.Nodes[k];
                        path1 = path1 + treenode.Text + "/";
                        IP1 = IP1 + treenode.Text + "/";
                        for (int l = 0; l < f.First(); l++)
                        {
                            treenode = treenode.Nodes[l];
                            path2 = path1 + treenode.Text;
                            IP2 = IP1  + treenode.Text;
                            
                            if(!File.Exists(path2))
                            {
                                DownloadFile(IP2, path2);
                            }
                        }
                        f.Remove(f.First());
                    }
                    d.Remove(d.First());
                }
                m.Remove(m.First());
            }
        }
        public void DownloadFile(string IP, string PATH)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(IP);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            //string path = PATH + "/folder";
           
            string path1 = PATH;
            string path2 = PATH;
            
            string filename = PATH.Substring(PATH.Length - 14, 14);
            path2 = path2.Substring(0, path2.Length - 14);
            path2 += "folder";
            Directory.CreateDirectory(path2);
            path1 = path1.Substring(0, path1.Length - 4);
            path1 += ".zip";
            FileStream fs = new FileStream(path2 + '/' + filename, FileMode.Create);
           
            byte[] buffer = new byte[64];
            int size = 0;

            while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, size);

            }
            fs.Close();
            response.Close();
           
            if (!File.Exists(path1))
            {
                ZipFile.CreateFromDirectory(path2, path1);
            }
            File.Delete(path2 + '/' + filename);
            Directory.Delete(path2);

            return; 
        }
        public List<int> CopyList(List<int> inp)
        {
            List<int> output = new List<int>();
            foreach(int z in inp)
            {
                output.Add(z);
            }
            return output;
        }
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if(Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                if (type == XmlSeverityType.Error)
                    throw new Exception(e.Message);
            }
        }

  
    }
}
