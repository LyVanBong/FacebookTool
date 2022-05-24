using FacebookTool.Models;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacebookTool.Helpers;

public static class FacebookHelper
{
    public static async Task<bool> SendMessage(Facebooks facebook)
    {
        try
        {
            var client = new RestClient("https://d.facebook.com/messages/send/?icm=1&refid=12");
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddHeader("Cookie", facebook.Cookie);
            request.AddParameter("body", facebook.body);
            request.AddParameter("fb_dtsg", facebook.fb_dtsg);
            request.AddParameter("jazoest", facebook.jazoest);
            if (!string.IsNullOrEmpty(facebook?.tids))
                request.AddParameter("tids", facebook?.tids);
            if (!string.IsNullOrEmpty(facebook?.wwwupp))
                request.AddParameter("wwwupp", facebook?.wwwupp);
            if (!string.IsNullOrEmpty(facebook?.cver))
                request.AddParameter("cver", facebook?.cver);
            if (!string.IsNullOrEmpty(facebook?.csid))
                request.AddParameter("csid", facebook?.csid);
            if (!string.IsNullOrEmpty(facebook?.id))
                request.AddParameter($"ids[{facebook?.id}]", facebook?.id);

            var response = await client.ExecuteAsync(request);
            var html = response?.Content;
            if (html != null && html.Contains("mbasic_logout_button"))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }

        return false;
    }

    public static async Task<Facebooks> GetParaFacebook(Facebooks facebook)
    {
        var client = new RestClient("https://d.facebook.com/messages/read/?fbid=" + facebook.id);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Cookie", facebook.Cookie);
        var response = await client.ExecuteAsync(request);

        var data = response?.Content;
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }
        // fb_dtsg
        Regex regex_fb_dtsg = new Regex(@"name=""fb_dtsg"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        MatchCollection collection_fb_dtsg = regex_fb_dtsg.Matches(data);
        if (collection_fb_dtsg.Any())
        {
            var fb_dtsg = collection_fb_dtsg[0]?.Groups[1]?.Value;
            if (!string.IsNullOrEmpty(fb_dtsg))
            {
                facebook.fb_dtsg = fb_dtsg;
            }
        }

        // jazoest
        Regex regex_jazoest = new Regex(@"name=""jazoest"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        MatchCollection collection_jazoest = regex_jazoest.Matches(data);
        if (collection_jazoest.Any())
        {
            var jazoest = collection_jazoest[0]?.Groups[1]?.Value;
            if (!string.IsNullOrEmpty(jazoest))
            {
                facebook.jazoest = jazoest;
            }
        }

        // tids
        Regex regex_tids = new Regex(@"name=""tids"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        var match_tids = regex_tids.Match(data);
        if (match_tids.Success)
        {
            var tids = match_tids.Groups[1].Value;
            if (!string.IsNullOrEmpty(tids))
            {
                facebook.tids = tids;
            }
        }

        // wwwupp
        Regex regex_wwwupp = new Regex(@"name=""wwwupp"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        var match_wwwupp = regex_wwwupp.Match(data);
        if (match_wwwupp.Success)
        {
            var wwwupp = match_wwwupp.Groups[1].Value;
            if (!string.IsNullOrEmpty(wwwupp))
            {
                facebook.wwwupp = wwwupp;
            }
        }

        // cver
        Regex regex_cver = new Regex(@"name=""cver"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        var match_cver = regex_cver.Match(data);
        if (match_cver.Success)
        {
            var cver = match_cver.Groups[1].Value;
            if (!string.IsNullOrEmpty(cver))
            {
                facebook.csid = cver;
            }
        }
        // csid
        Regex regex_csid = new Regex(@"name=""csid"" value=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        var match_csid = regex_csid.Match(data);
        if (match_csid.Success)
        {
            var csid = match_csid.Groups[1].Value;
            if (!string.IsNullOrEmpty(csid))
            {
                facebook.csid = csid;
            }
        }

        if (string.IsNullOrEmpty(facebook.fb_dtsg) || string.IsNullOrEmpty(facebook.jazoest))
        {
            return null;
        }

        return facebook;
    }
}