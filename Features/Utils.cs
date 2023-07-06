﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using BanLogger.Features.Discord;
using BanLogger.Features.Enums;
using BanLogger.Features.Structs;
using Exiled.API.Features;
using MEC;

namespace BanLogger.Features
{
    public static class Utils
    {
        public static void CreateEmbed(BanInfo banInfo, MessageType messageType, WebhookType webhookType)
        {
            var embed = new Embed
            {
                title = Plugin.Instance.Config.Title,
                color = ConfigColor,
                description = CreateEmbedDescription(banInfo, messageType, webhookType),
                author = new EmbedAuthor(Plugin.Instance.Config.AuthorName, null, Plugin.Instance.Config.ServerImgUrl),
                image = new EmbedImage(Plugin.Instance.Config.ImageUrl),
                footer = new EmbedFooter(Plugin.Instance.Config.FooterTxt, Plugin.Instance.Config.FooterIconUrl)
            };

            var webhook = webhookType == WebhookType.Public ? Plugin.Instance.Config.PublicWebhooks[messageType] : Plugin.Instance.Config.PrivateWebhooks[messageType];
                
            if (DiscordHandler.Queue.ContainsKey(webhook))
                DiscordHandler.Queue[webhook].Add(embed);
            else
                DiscordHandler.Queue.Add(webhook, new List<Embed> { embed });

            if (!DiscordHandler.CoroutineHandle.IsRunning)
                DiscordHandler.CoroutineHandle = Timing.RunCoroutine(DiscordHandler.HandleQueue());
        }

        public static string CreateEmbedDescription(BanInfo banInfo, MessageType messageType, WebhookType webhookType)
        {
            var fields = new List<Field>();
            
            switch (messageType)
            {
                case MessageType.Kick when webhookType == WebhookType.Public:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, "Kick"),
                    };
                    break;
                case MessageType.Kick when webhookType == WebhookType.Private:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, "Kick"),
                    };
                    break;
                case MessageType.Ban when webhookType == WebhookType.Public:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, banInfo.ReadableDuration),
                    };
                    break;
                case MessageType.Ban when webhookType == WebhookType.Private:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, banInfo.ReadableDuration),
                    };
                    break;
                case MessageType.OBan when webhookType == WebhookType.Public:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PublicInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, banInfo.ReadableDuration),
                    };
                    break;
                case MessageType.OBan when webhookType == WebhookType.Private:
                    fields = new List<Field>
                    {
                        new Field(Plugin.Instance.Config.UserBannedText, banInfo.BannedUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.IssuingStaffText, banInfo.IssuerUserInfo.PrivateInfo),
                        new Field(Plugin.Instance.Config.ReasonText, banInfo.Reason),
                        new Field(Plugin.Instance.Config.TimeBannedText, banInfo.ReadableDuration),
                    };
                    break;
            }

            return fields.Join();
        }

        public static string GetUserName(string userid)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest) WebRequest.Create($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={Plugin.Instance.Config.SteamApiKey}&steamids={userid}");
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return Regex.Match(result, @"\x22personaname\x22:\x22(.+?)\x22").Groups[1].Value;
                }
            }
            catch (Exception)
            {
                Log.Error("An error has occured while contacting steam servers (Are they down? Invalid API key?)");
            }

            return "Unknown";
        }
        
        public static string Join(this List<Field> fields)
        {
            return fields.Aggregate("", (current, field) => current + field);
        }

        public static string TimeFormatter(long duration)
        {
            var timespan = new TimeSpan(0, 0, (int)duration);
            var finalFormat = "";

            if (timespan.TotalDays >= 365)
                finalFormat += $" {timespan.TotalDays / 365}y";
            else if (timespan.TotalDays >= 30)
                finalFormat += $" {timespan.TotalDays / 30}mon";
            else if (timespan.TotalDays >= 1)
                finalFormat += $" {timespan.TotalDays}d";
            else if (timespan.Hours > 0)
                finalFormat += $" {timespan.Hours}h";
            if (timespan.Minutes > 0)
                finalFormat += $" {timespan.Minutes}min";
            if (timespan.Seconds > 0)
                finalFormat += $" {timespan.Seconds}s";

            return finalFormat;
        }

        public static int ConfigColor => int.Parse(Plugin.Instance.Config.HexColor.Replace("#", ""), NumberStyles.HexNumber);
    }
}