using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Github;

public static class GithubIntegration
{
	private static HttpClient client => new HttpClient
	{
		DefaultRequestHeaders = { { "User-Agent", "request" } }
	};

	public static async Task<string> Get(string Url)
	{
		return (await client.GetAsync(Url)).Content.ReadAsStringAsync().Result;
	}

	public static async Task<(string, string)> GetLatestVersion()
	{
		List<Dictionary<string, object>>? list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await Get("https://api.github.com/repos/edgarcantuco/BOTW.Release/releases"));
		string LatestRelease = "0.0.0";
		string url = "";
		foreach (Dictionary<string, object> item in list)
		{
			if (CompareVersion(LatestRelease, (string)item["tag_name"]))
			{
				LatestRelease = (string)item["tag_name"];
				url = (string)item["assets_url"];
			}
		}
		List<Dictionary<string, object>> list2 = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await Get(url));
		return (LatestRelease, (string)list2[0]["url"]);
	}

	public static bool CompareVersion(string oldVersion, string newVersion)
	{
		List<string> list = oldVersion.Split(".").ToList();
		List<string> list2 = newVersion.Split(".").ToList();
		for (int i = 0; i < 3; i++)
		{
			if (short.Parse(list[i]) < short.Parse(list2[i]))
			{
				return true;
			}
			if (short.Parse(list[i]) > short.Parse(list2[i]))
			{
				return false;
			}
		}
		return false;
	}

	public static async Task<string> DownloadZip(string Url)
	{
		string text = Guid.NewGuid().ToString();
		WebClient webClient = new WebClient();
		try
		{
			webClient.Headers.Add(HttpRequestHeader.UserAgent, "request");
			webClient.Headers.Add(HttpRequestHeader.Accept, "application/octet-stream");
			webClient.DownloadFile(new Uri(Url), text + ".zip");
			return text;
		}
		finally
		{
			((IDisposable)webClient)?.Dispose();
		}
	}
}
