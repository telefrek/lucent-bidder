using System;
using System.Linq;
using System.Security;
using System.Xml;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Formatters
{
    /// <summary>
    /// 
    /// </summary>
    public static class VastFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string ToVast(this BidMatch match)
        {
            // Format the video in VAST protocol
            if (match.Impression.Video != null)
            {
                var proto = match.Impression.Video.Protocols ?? new VideoProtocol[] { match.Impression.Video.Protocol };
                if (proto.Contains(VideoProtocol.VAST_4))
                    return match.ToVast4();
                else if (proto.Contains(VideoProtocol.VAST_3))
                    return match.ToVast3();
                else if (proto.Contains(VideoProtocol.VAST_2))
                    return match.ToVast2();

                return null;
            }

            return null;
        }

        /// <summary>
        /// Format as VAST 4
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string ToVast4(this BidMatch match)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "4.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, match.Creative);
            inline.AddDescription(xDoc, match.Creative);
            inline.AddErrorUri(xDoc, match.Creative, match.RawBid);
            inline.AddImpression(xDoc, match.Creative, match.RawBid);
            inline.AddAdvertiser(xDoc, match.Campaign);
            inline.AddPricing(xDoc, match.RawBid);

            inline.AppendChild(xDoc.CreateElement("Extensions"));
            inline.AddViewableImpression(xDoc, match.Creative, match.RawBid);

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(match.Creative);

            var uId = xDoc.CreateElement("UniversalAdId");
            uId.Attributes.Append(xDoc.CreateVastAttribute("idRegistry", "lucentbid.com"));
            uId.Attributes.Append(xDoc.CreateVastAttribute("idValue", match.Creative.Id));
            uId.InnerText = match.Creative.Id;
            xCreative.AppendChild(uId);

            var linear = xDoc.CreateLinear(match.Content);
            linear.AddDuration(xDoc, match.Content);

            var mediaFiles = xDoc.CreateMediaFiles(match.Content, 4);

            var mez = xDoc.CreateElement("Mezzanine");
            mez.AppendChild(xDoc.CreateCDataSection(match.Content.RawUri));
            mediaFiles.AppendChild(mez);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, match.Campaign, match.Creative, match.RawBid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }

        /// <summary>
        /// Format as VAST 3
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string ToVast3(this BidMatch match)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "3.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, match.Creative);
            inline.AddDescription(xDoc, match.Creative);
            inline.AddErrorUri(xDoc, match.Creative, match.RawBid);
            inline.AddImpression(xDoc, match.Creative, match.RawBid);
            inline.AddAdvertiser(xDoc, match.Campaign);
            inline.AddPricing(xDoc, match.RawBid);

            inline.AppendChild(xDoc.CreateElement("Extensions"));

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(match.Creative);

            var uId = xDoc.CreateElement("UniversalAdId");
            uId.Attributes.Append(xDoc.CreateVastAttribute("idRegistry", "lucentbid.com"));
            uId.Attributes.Append(xDoc.CreateVastAttribute("idValue", match.Creative.Id));
            uId.InnerText = match.Creative.Id;
            xCreative.AppendChild(uId);

            var linear = xDoc.CreateLinear(match.Content);
            linear.AddDuration(xDoc, match.Content);

            var mediaFiles = xDoc.CreateMediaFiles(match.Content, 4);

            var mez = xDoc.CreateElement("Mezzanine");
            mez.AppendChild(xDoc.CreateCDataSection(match.Content.RawUri));
            mediaFiles.AppendChild(mez);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, match.Campaign, match.Creative, match.RawBid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }

        /// <summary>
        /// Format as VAST 2
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string ToVast2(this BidMatch match)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "2.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, match.Creative);
            inline.AddDescription(xDoc, match.Creative);
            inline.AddErrorUri(xDoc, match.Creative, match.RawBid);
            inline.AddImpression(xDoc, match.Creative, match.RawBid);
            inline.AppendChild(xDoc.CreateElement("Extensions"));

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(match.Creative);

            var linear = xDoc.CreateLinear(match.Content);
            linear.AddDuration(xDoc, match.Content);

            var mediaFiles = xDoc.CreateMediaFiles(match.Content, 2);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, match.Campaign, match.Creative, match.RawBid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }
    }

    /// <summary>
    /// XML Extensions for VAST formatting
    /// </summary>
    public static class VastXmlExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        public static void AddAdSystem(this XmlElement element, XmlDocument xDoc)
        {
            var adSystem = xDoc.CreateElement("AdSystem");
            adSystem.InnerText = "lucentbid";
            adSystem.Attributes.Append(xDoc.CreateVastAttribute("version", "1.0"));
            element.AppendChild(adSystem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        public static void AddAdTitle(this XmlElement element, XmlDocument xDoc, Creative creative)
        {
            var adTitle = xDoc.CreateElement("AdTitle");
            adTitle.InnerText = creative.Title;
            element.AppendChild(adTitle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        /// <param name="bid"></param>
        public static void AddImpression(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var impression = xDoc.CreateElement("Impression");
            impression.AppendChild(xDoc.CreateCDataSection(GeneratePostback(BidOperation.Impression, bid)));
            impression.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            element.AppendChild(impression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        public static void AddDescription(this XmlElement element, XmlDocument xDoc, Creative creative)
        {
            var desc = xDoc.CreateElement("Description");
            desc.InnerText = creative.Description;
            element.AppendChild(desc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="campaign"></param>
        public static void AddAdvertiser(this XmlElement element, XmlDocument xDoc, Campaign campaign)
        {
            var adv = xDoc.CreateElement("Advertiser");
            adv.InnerText = campaign.AdDomains.FirstOrDefault();
            element.AppendChild(adv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="bid"></param>
        public static void AddPricing(this XmlElement element, XmlDocument xDoc, Bid bid)
        {
            var price = xDoc.CreateElement("Pricing");
            price.AppendChild(xDoc.CreateCDataSection(Math.Round(bid.CPM, 2, MidpointRounding.AwayFromZero).ToString()));
            price.Attributes.Append(xDoc.CreateVastAttribute("model", "CPM"));
            price.Attributes.Append(xDoc.CreateVastAttribute("currency", "USD"));
            element.AppendChild(price);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        /// <param name="bid"></param>
        public static void AddErrorUri(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var err = xDoc.CreateElement("Error");
            err.AppendChild(xDoc.CreateCDataSection(GeneratePostback(BidOperation.Error, bid)));
            element.AppendChild(err);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        /// <param name="bid"></param>
        public static void AddViewableImpression(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var viewImp = xDoc.CreateElement("ViewableImpression");
            var viewable = xDoc.CreateElement("Viewable");
            viewable.AppendChild(xDoc.CreateCDataSection(GeneratePostback(BidOperation.Viewed, bid)));
            var notviewable = xDoc.CreateElement("NotViewable");
            notviewable.AppendChild(xDoc.CreateCDataSection(GeneratePostback(BidOperation.NotViewed, bid)));
            viewImp.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            viewImp.AppendChild(viewable);
            viewImp.AppendChild(notviewable);
            element.AppendChild(viewImp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="creative"></param>
        /// <returns></returns>
        public static XmlElement CreateCreative(this XmlDocument xDoc, Creative creative)
        {
            var xCreative = xDoc.CreateElement("Creative");
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("adId", creative.Id));
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("sequence", "1"));

            return xCreative;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static XmlElement CreateLinear(this XmlDocument xDoc, CreativeContent content)
        {
            var linear = xDoc.CreateElement("Linear");
            if (content.Offset > 0)
                linear.Attributes.Append(xDoc.CreateVastAttribute("skipoffset", TimeSpan.FromSeconds(content.Offset).ToString()));

            return linear;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="content"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static XmlElement CreateMediaFiles(this XmlDocument xDoc, CreativeContent content, int version = 4)
        {
            var mediaFiles = xDoc.CreateElement("MediaFiles");
            mediaFiles.AppendChild(xDoc.CreateMediaFile(content, content.CreativeUri, version));

            return mediaFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="content"></param>
        public static void AddDuration(this XmlElement element, XmlDocument xDoc, CreativeContent content)
        {
            var duration = xDoc.CreateElement("Duration");
            duration.InnerText = TimeSpan.FromSeconds(content.Duration).ToString();
            element.AppendChild(duration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="xDoc"></param>
        /// <param name="campaign"></param>
        /// <param name="creative"></param>
        /// <param name="bid"></param>
        public static void AddVideoClicks(this XmlElement element, XmlDocument xDoc, Campaign campaign, Creative creative, Bid bid)
        {
            var clicks = xDoc.CreateElement("VideoClicks");

            var clickThrough = xDoc.CreateElement("ClickThrough");
            clickThrough.AppendChild(xDoc.CreateCDataSection(campaign.ReplaceMacros(creative, bid)));
            clickThrough.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            clicks.AppendChild(clickThrough);

            var clickTrack = xDoc.CreateElement("ClickTracking");
            clickTrack.AppendChild(xDoc.CreateCDataSection(GeneratePostback(BidOperation.Clicked, bid)));
            clickTrack.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            clicks.AppendChild(clickTrack);

            element.AppendChild(clicks);
        }

        /// <summary>
        /// Generate an appropriate postback link
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        public static string GeneratePostback(BidOperation operation, Bid bid) => new Uri(bid.BidContext.BaseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bid.BidContext.GetOperationString(operation)).AbsoluteUri;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="content"></param>
        /// <param name="creativeUri"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static XmlElement CreateMediaFile(this XmlDocument xDoc, CreativeContent content, string creativeUri, int version = 4)
        {
            var media = xDoc.CreateElement("MediaFile");
            media.AppendChild(xDoc.CreateCDataSection(creativeUri));
            media.Attributes.Append(xDoc.CreateVastAttribute("delivery", "progressive"));
            media.Attributes.Append(xDoc.CreateVastAttribute("type", content.MimeType));
            media.Attributes.Append(xDoc.CreateVastAttribute("width", content.W.ToString()));
            media.Attributes.Append(xDoc.CreateVastAttribute("heigt", content.H.ToString()));
            media.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            media.Attributes.Append(xDoc.CreateVastAttribute("scalable", content.CanScale ? "1" : "0"));
            media.Attributes.Append(xDoc.CreateVastAttribute("bitrate", content.BitRate.ToString()));
            media.Attributes.Append(xDoc.CreateVastAttribute("maintainAspectRatio", content.PreserveAspect ? "1" : "0"));

            if (version >= 3)
                if (!string.IsNullOrWhiteSpace(content.Codec))
                    media.Attributes.Append(xDoc.CreateVastAttribute("codec", content.Codec));

            return media;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XmlAttribute CreateVastAttribute(this XmlDocument xDoc, string name, string value)
        {
            var attr = xDoc.CreateAttribute(name);
            attr.Value = value;

            return attr;
        }
    }
}