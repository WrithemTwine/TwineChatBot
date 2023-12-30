using System;
using System.IO;

namespace ConvertDataSourceFile
{
    public class Program
    {
        private static readonly string DataFileXML = "ChatDataStore.xml";

        static void Main(string[] args)
        {
            if (File.Exists(DataFileXML))
            {
                ConvertDataSource_v_0_1_12_0();
                ConvertDataSource_v_0_1_12_4();
                ConvertDataSource_v_0_2_5_1();
            }
        }

        private static void ConvertDataSource_v_0_1_12_0()
        {
            /*

             <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
                <DataSource xmlns="http://tempuri.org/DataSource.xsd">
                  <ChannelEvents diffgr:id="ChannelEvents1" msdata:rowOrder="0">
                   <Id>1</Id>
                   <Name>BeingHosted</Name>
                   <RepeatMsg>0</RepeatMsg>
                   <AddMe>false</AddMe>
                   <IsEnabled>true</IsEnabled>
                   <MsgStr>Thanks #user for #autohost this channel!</MsgStr>
                   <Commands>#user,#autohost,#viewers</Commands>
                 </ChannelEvents>
                </DataSource>
            </diffgr:diffgram>

                <Clips diffgr:id="Clips1" msdata:rowOrder="0">
                  <Id>DependableInexpensiveGarageTooSpicy-hTDYSGRHyQ3_kllT</Id>
                  <CreatedAt>2021-08-21T22:28:14-04:00</CreatedAt>
                  <Title>Wait - what wall?</Title>
                  <GameId>497118</GameId>
                  <Language>en</Language>
                  <Duration>10.8</Duration>
                  <Url>https://clips.twitch.tv/DependableInexpensiveGarageTooSpicy-hTDYSGRHyQ3_kllT</Url>
                </Clips>

             */

            File.Move(DataFileXML, "temp_" + DataFileXML);
            string tempfile = "temp_" + DataFileXML;

            StreamWriter output = new(DataFileXML);

            string starttag = "<CreatedAt>";
            string endtag = "</CreatedAt>";
            const string Format = "yyyy-MM-ddTHH:mm:sszzz";

            using (StreamReader sr = new(tempfile))
            {
                do
                {
                    string line = sr.ReadLine();

                    if (line.Contains("MsgStr"))
                    {
                        line = line.Replace("MsgStr", "Message");
                    }

                    if (line.Contains("CreatedAt"))
                    {
                        DateTime ParseTime = DateTime.Parse(line.Replace(starttag, "").Replace(endtag, ""));

                        line = $"{starttag}{ParseTime.ToString(Format)}{endtag}";
                    }

                    output.WriteLine(line);
                }
                while (!sr.EndOfStream);

                sr.Close();
            }

            output.Close();
            File.Delete(tempfile);
        }

        private static void ConvertDataSource_v_0_1_12_4()
        {
            /*

            <CategoryList diffgr:id="CategoryList2" msdata:rowOrder="1">
                  <Id>1</Id>
                  <CategoryId>517330</CategoryId>
                  <Category>Assassin's Creed Valhalla</Category>
                  <StreamCount>0</StreamCount>
            </CategoryList> 

             * */

            File.Move(DataFileXML, "temp_" + DataFileXML);
            string tempfile = "temp_" + DataFileXML;

            StreamWriter output = new(DataFileXML);

            bool Found = false;

            using (StreamReader sr = new(tempfile))
            {
                do
                {
                    string line = sr.ReadLine();

                    if (line.Contains("StreamCount"))
                    {
                        Found = true;
                    }

                    if (line.Contains("</CategoryList>"))
                    {
                        if (!Found)
                        {
                            output.WriteLine("<StreamCount>0</StreamCount>");
                        }

                        Found = false;
                    }

                    output.WriteLine(line);
                }
                while (!sr.EndOfStream);

                sr.Close();
            }

            output.Close();
            File.Delete(tempfile);
        }

        private static void ConvertDataSource_v_0_2_5_1()
        {
            /*
             * 
                  <Commands diffgr:id="Commands22" msdata:rowOrder="21">
                      <Id>22</Id>
                      <CmdName>uptime</CmdName>
                      <AddMe>false</AddMe>
                      <Permission>Viewer</Permission>
                      <IsEnabled>false</IsEnabled>
                      <Message>#user has been streaming for #uptime.</Message>
                      <RepeatTimer>0</RepeatTimer>
                      <SendMsgCount>0</SendMsgCount>
                      <Category>All</Category>
                      <AllowParam>false</AllowParam>
                      <Usage>!uptime</Usage>
                      <lookupdata>false</lookupdata>
                      <table />
                      <key_field />
                      <data_field />
                      <currency_field />
                      <unit />
                      <action>Get</action>
                      <top>0</top>
                      <sort>ASC</sort>
                  </Commands>
             *
             */

            File.Move(DataFileXML, "temp_" + DataFileXML);
            string tempfile = "temp_" + DataFileXML;

            StreamWriter output = new(DataFileXML);

            bool Found = false;

            using (StreamReader sr = new(tempfile))
            {
                do
                {
                    string line = sr.ReadLine();

                    if (line.Contains("CmdName") && line.Contains("uptime"))
                    {
                        Found = true;
                    }

                    if (line.Contains("AllowParam"))
                    {
                        if (!Found)
                        {
                            output.WriteLine("<AllowParam>true</AllowParam>");
                        }

                        Found = false;
                    }

                    output.WriteLine(line);
                }
                while (!sr.EndOfStream);

                sr.Close();
            }

            output.Close();
            File.Delete(tempfile);

        }
    }
}
