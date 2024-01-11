using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KOAStudio.Core.Helpers
{
    public record GithubTagInfo(string html_url, string tag_name, string name, string published_at, string body);
    public static class GithubVersion
    {
        public static Task<IList<GithubTagInfo>?> GetRepoTagInfos(string Username, string Repository)
        {
            // 깃헙 릴리즈 태그에서 가져오기
            HttpClient client = new();
            var pih = ProductInfoHeaderValue.Parse(Repository);
            client.DefaultRequestHeaders.UserAgent.Add(pih);
            return client.GetFromJsonAsync<IList<GithubTagInfo>>($"https://api.github.com/repos/{Username}/{Repository}/releases");
        }
    }
}
