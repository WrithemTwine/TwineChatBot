﻿using System;
using System.IO;
using System.Xml.Linq;

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

                        line = starttag + ParseTime.ToString(Format) + endtag;
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