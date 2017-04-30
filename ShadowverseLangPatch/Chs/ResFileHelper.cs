using Cute;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Galstars.Extensions
{
	class ResFileHelper
	{
		readonly string jsonFolder = "";
		readonly string masterFolder = "";
		readonly string scenarioFolder = "";
		readonly string lngFile = "";
		JsonData lngJson = null;

		Regex uReg = new Regex(@"\[u\]\[ffcd45\](.*?)\[\-\]\[\/u\]");

		static ResFileHelper _Instance = null;

		ResFileHelper()
		{
			const string CONFIG_FILE = "ResFileHelper";
			var fn = Path.Combine(
				Application.streamingAssetsPath,
				CONFIG_FILE);

			if (!File.Exists(fn))
			{ return; }

			Regex reg = new Regex("(\\w+)\\s*=\\s*\"?(.+)\"?");
			var lst = new List<string>();

			foreach (var ln in File.ReadAllLines(fn))
			{
				var line = ln.Trim();
				if (line[0] == ';') continue;

				var m = reg.Match(line);
				if (!m.Success) continue;

				var option = m.Groups[1].Value.ToLower();
				var value = m.Groups[2].Value.Trim();
				if (value == "") continue;

				switch (option)
				{
					case "jsonfolder":
						if (Directory.Exists(value))
							jsonFolder = value;
						break;
					case "masterfolder":
						if (Directory.Exists(value))
							masterFolder = value;
						break;
					case "scenariofolder":
						if (Directory.Exists(value))
							scenarioFolder = value;
						break;
					case "lngfile":
						if (File.Exists(value))
						{
							lngFile = value;
							lngJson = JsonMapper.ToObject(File.ReadAllText(value));
						}
						break;
				}
			}
		}

		Dictionary<string, string> Get_MasterDict(string assetName)
		{
			var result = new Dictionary<string, string>();
			//从Chs.lng文件中加载
			if (lngJson != null)
			{
				IDictionary jsn = lngJson[assetName];
				if (jsn != null && jsn.Count > 0)
				{
					foreach (string k in jsn.Keys)
					{
						result[k] = jsn[k].ToString();
					}
				}
			}

			string content = "";

			//从master_chs文件夹中加载
			var fn = Path.Combine(masterFolder, assetName + ".txt");
			if (File.Exists(fn))
			{
				content = File.ReadAllText(fn);
			}

			//从程序集中加载
			if (String.IsNullOrEmpty(content))
			{
				Resource1.ResourceManager.GetString(assetName);
			}

			if (!String.IsNullOrEmpty(content))
			{
				//将下划线转换为加粗
				if (CustomPreference._localePref == "Eng")
				{
					content = uReg.Replace(content, "[ffcd45][b]$1[/b][-]");
				}

				var dict = JsonMapper.ToObject<Dictionary<string, string>>(content);

				foreach (string k in dict.Keys)
				{
					if (!result.ContainsKey(k))
						result[k] = dict[k];
				}
			}

			return result.Count > 0 ? result : null;
		}

		string Get_SystemText(string resName)
		{
			string content = "";

			//从master_chs文件夹中加载
			var fn = Path.Combine(jsonFolder, resName + ".txt");
			if (File.Exists(fn))
			{
				content = File.ReadAllText(fn);
			}

			//从程序集中加载
			if (String.IsNullOrEmpty(content))
			{
				Resource1.ResourceManager.GetString(resName);
			}

			return content;
		}

		string Get_ScenarioText(string scenaName)
		{
			string content = "";

			//从master_chs文件夹中加载
			var fn = Path.Combine(scenarioFolder, scenaName + ".txt");
			if (File.Exists(fn))
			{
				content = File.ReadAllText(fn);
			}

			//从程序集中加载
			if (String.IsNullOrEmpty(content))
			{
				content = Resource2.ResourceManager.GetString(scenaName);
			}

			return content;
		}

		static public Dictionary<string, string> GetMasterDict(string assetName)
		{
			_Instance = _Instance ?? new ResFileHelper();
			return _Instance.Get_MasterDict(assetName);
		}

		static public string GetSystemText(string resName)
		{
			_Instance = _Instance ?? new ResFileHelper();
			return _Instance.Get_SystemText(resName);
		}

		static public string GetScenarioText(string scenaName)
		{
			_Instance = _Instance ?? new ResFileHelper();
			return _Instance.Get_ScenarioText(scenaName);
		}
	}

}
