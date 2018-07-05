﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace LiveSplit.LiveSplitClientPlugin
{
	public class LSClientInstance
	{
		DispatcherTimer t;
		DateTime start;
		TcpClient client;

		public static string AppDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\LiveSplitClient";
		public static string ConfigurationFile = AppDataRoamingPath + @"\Config.xml";

		public string IPAddress { get; set; }
		public int Port { get; set; }
		public int IGT
		{
			get
			{
				return TimeStringToMili(serverReturnData);
			}
		}
		public string serverReturnData { get; set; }

		public LSClientInstance()
		{
			t = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 10), DispatcherPriority.Background, t_Tick, Dispatcher.CurrentDispatcher);
			CreateConfiguration();
			ReadConfiguration();
			ConnectToServer();
			start = DateTime.Now;
		}

		private void t_Tick(object sender, EventArgs e)
		{
			try
			{
				StreamWriter sw = new StreamWriter(client.GetStream());
				StreamReader sr = new StreamReader(client.GetStream());
				sw.WriteLine("getcurrenttime");
				sw.Flush();
				serverReturnData = sr.ReadLine();
			}
			catch
			{

			}
		}

		public int TimeStringToMili(string time)
		{
			if(time == null)
			{
				return 0;
			}

			if (Regex.Matches(time, @"[a-zA-Z]").Count > 0)
			{
				return 0;
			}

			int milliseconds = 0;
			int seconds = 0;
			int minutes = 0;
			int hours = 0;

			switch(time.Count(f => f == ':'))
			{
				case 0:
					seconds = int.Parse(time.Substring(0, time.IndexOf('.')));
					time = time.Substring(time.IndexOf('.') + 1);
					milliseconds = 10 * int.Parse(time);
					break;
				case 1:
					minutes = int.Parse(time.Substring(0, time.IndexOf(':')));
					time = time.Substring(time.IndexOf(':') + 1);
					seconds = int.Parse(time.Substring(0, time.IndexOf('.')));
					time = time.Substring(time.IndexOf('.') + 1);
					milliseconds = 10 * int.Parse(time);
					break;
				case 2:
					hours = int.Parse(time.Substring(0, time.IndexOf(':')));
					time = time.Substring(time.IndexOf(':') + 1);
					minutes = int.Parse(time.Substring(0, time.IndexOf(':')));
					time = time.Substring(time.IndexOf(':') + 1);
					seconds = int.Parse(time.Substring(0, time.IndexOf('.')));
					time = time.Substring(time.IndexOf('.') + 1);
					milliseconds = 10 * int.Parse(time);
					break;
			}

			milliseconds = milliseconds +(hours * 60 * 60 * 1000) + (minutes * 60 * 1000) + (seconds * 1000);

			return milliseconds;
		}

		public void CreateConfiguration()
		{
			if (!File.Exists(ConfigurationFile))
			{
				Directory.CreateDirectory(AppDataRoamingPath);
				XmlTextWriter Writer = new XmlTextWriter(ConfigurationFile, Encoding.UTF8);
				Writer.Formatting = Formatting.Indented;
				Writer.WriteStartElement("Configs");

				Writer.WriteStartElement("IPAddress");
				Writer.WriteString("0.0.0.0");
				Writer.WriteEndElement();

				Writer.WriteStartElement("Port");
				Writer.WriteString("16834");
				Writer.WriteEndElement();

				Writer.WriteEndElement();
				Writer.Close();
			}
		}

		public void ReadConfiguration()
		{
			if (File.Exists(ConfigurationFile))
			{
				XDocument Xml = XDocument.Load(ConfigurationFile);
				XElement ipElement = Xml.Element("Configs").Element("IPAddress");
				IPAddress = ipElement.Value;
				XElement portElement = Xml.Element("Configs").Element("Port");
				Port = int.Parse(portElement.Value);
			}
		}

		public void SaveConfiguration(string ip, string port)
		{
			XDocument Xml = XDocument.Load(ConfigurationFile);
			Xml.Element("Configs").Element("IPAddress").Value = ip;
			Xml.Element("Configs").Element("Port").Value = port.ToString();
			Xml.Save(ConfigurationFile);
			ReadConfiguration();
		}

		public void ConnectToServer()
		{
			try
			{
				if(client != null)
				{
					client.Close();
				}
				client = new TcpClient(IPAddress, Port);
				serverReturnData = "Connected";
				t.IsEnabled = true;
			}
			catch
			{
				serverReturnData = "Connection error, please restart livesplit server";
				t.IsEnabled = false;
			}
		}

		public void Disconnect()
		{

		}
	}
}