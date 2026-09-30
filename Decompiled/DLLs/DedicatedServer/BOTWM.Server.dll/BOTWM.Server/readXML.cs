using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BOTWM.Server;

public static class readXML
{
	public static Dictionary<string, Dictionary<string, string>> readAnimationFile()
	{
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
		XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(Resources.animationHashes));
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		string[] source = new string[4] { "Hash", "Schedule", "Animation", "Name" };
		string text = "";
		string key = "";
		dictionary2.Add("Schedule", "");
		dictionary2.Add("Animation", "");
		dictionary2.Add("Name", "");
		while (xmlTextReader.Read())
		{
			switch (xmlTextReader.NodeType)
			{
			case XmlNodeType.Element:
				if (source.Contains(xmlTextReader.Name))
				{
					text = xmlTextReader.Name;
				}
				break;
			case XmlNodeType.Text:
				if (!(text != ""))
				{
					break;
				}
				if (text == "Hash")
				{
					key = xmlTextReader.Value;
					break;
				}
				dictionary2[text] = xmlTextReader.Value;
				if (text == "Animation")
				{
					dictionary3 = new Dictionary<string, string>(dictionary2);
					dictionary.Add(key, dictionary3);
					text = "";
				}
				break;
			}
		}
		xmlTextReader.Close();
		return dictionary;
	}
}
