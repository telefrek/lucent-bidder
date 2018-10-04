using System;
using System.Linq;
using System.Security;
using System.Xml;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Formatters
{
    public class VastFormatter
    {
        public static string ToVast4(Campaign campaign, Creative creative, CreativeContent content, Bid bid)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "4.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, creative);
            inline.AddDescription(xDoc, creative);
            inline.AddErrorUri(xDoc, creative, bid);
            inline.AddImpression(xDoc, creative, bid);
            inline.AddAdvertiser(xDoc, campaign);
            inline.AddPricing(xDoc, bid);

            inline.AppendChild(xDoc.CreateElement("Extensions"));
            inline.AddViewableImpression(xDoc, creative, bid);

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(creative);

            var uId = xDoc.CreateElement("UniversalAdId");
            uId.Attributes.Append(xDoc.CreateVastAttribute("idRegistry", "lucentbid.com"));
            uId.Attributes.Append(xDoc.CreateVastAttribute("idValue", creative.Id));
            uId.InnerText = creative.Id;
            xCreative.AppendChild(uId);

            var linear = xDoc.CreateLinear(content);
            linear.AddDuration(xDoc, content);

            var mediaFiles = xDoc.CreateMediaFiles(content, 4);

            var mez = xDoc.CreateElement("Mezzanine");
            mez.AppendChild(xDoc.CreateCDataSection(content.RawUri));
            mediaFiles.AppendChild(mez);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, campaign, creative, bid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }

        public static string ToVast3(Campaign campaign, Creative creative, CreativeContent content, Bid bid)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "3.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, creative);
            inline.AddDescription(xDoc, creative);
            inline.AddErrorUri(xDoc, creative, bid);
            inline.AddImpression(xDoc, creative, bid);
            inline.AddAdvertiser(xDoc, campaign);
            inline.AddPricing(xDoc, bid);

            inline.AppendChild(xDoc.CreateElement("Extensions"));

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(creative);

            var uId = xDoc.CreateElement("UniversalAdId");
            uId.Attributes.Append(xDoc.CreateVastAttribute("idRegistry", "lucentbid.com"));
            uId.Attributes.Append(xDoc.CreateVastAttribute("idValue", creative.Id));
            uId.InnerText = creative.Id;
            xCreative.AppendChild(uId);

            var linear = xDoc.CreateLinear(content);
            linear.AddDuration(xDoc, content);

            var mediaFiles = xDoc.CreateMediaFiles(content, 4);

            var mez = xDoc.CreateElement("Mezzanine");
            mez.AppendChild(xDoc.CreateCDataSection(content.RawUri));
            mediaFiles.AppendChild(mez);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, campaign, creative, bid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }

        public static string ToVast2(Campaign campaign, Creative creative, CreativeContent content, Bid bid)
        {
            var xDoc = new XmlDocument();

            var vastRoot = xDoc.CreateElement("VAST", "http://www.iab.com/VAST");
            vastRoot.Attributes.Append(xDoc.CreateVastAttribute("version", "2.0"));

            var ad = xDoc.CreateElement("Ad");
            ad.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));

            var inline = xDoc.CreateElement("InLine");
            inline.AddAdSystem(xDoc);
            inline.AddAdTitle(xDoc, creative);
            inline.AddDescription(xDoc, creative);
            inline.AddErrorUri(xDoc, creative, bid);
            inline.AddImpression(xDoc, creative, bid);
            inline.AppendChild(xDoc.CreateElement("Extensions"));

            var creatives = xDoc.CreateElement("Creatives");
            var xCreative = xDoc.CreateCreative(creative);

            var linear = xDoc.CreateLinear(content);
            linear.AddDuration(xDoc, content);

            var mediaFiles = xDoc.CreateMediaFiles(content, 2);

            linear.AppendChild(mediaFiles);
            linear.AddVideoClicks(xDoc, campaign, creative, bid);

            xCreative.AppendChild(linear);

            creatives.AppendChild(xCreative);
            inline.AppendChild(creatives);

            ad.AppendChild(inline);

            vastRoot.AppendChild(ad);

            return vastRoot.OuterXml;
        }
    }

    public static class VastXmlExtensions
    {
        public static string POSTBACK_URI = "";

        public static void AddAdSystem(this XmlElement element, XmlDocument xDoc)
        {
            var adSystem = xDoc.CreateElement("AdSystem");
            adSystem.InnerText = "lucentbid";
            adSystem.Attributes.Append(xDoc.CreateVastAttribute("version", "1.0"));
            element.AppendChild(adSystem);
        }

        public static void AddAdTitle(this XmlElement element, XmlDocument xDoc, Creative creative)
        {
            var adTitle = xDoc.CreateElement("AdTitle");
            adTitle.InnerText = creative.Title;
            element.AppendChild(adTitle);
        }

        public static void AddImpression(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var impression = xDoc.CreateElement("Impression");
            impression.AppendChild(xDoc.CreateCDataSection(POSTBACK_URI + "&a=imp&lctx=" + bid.Id.SafeBase64Encode()));
            impression.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            element.AppendChild(impression);
        }

        public static void AddDescription(this XmlElement element, XmlDocument xDoc, Creative creative)
        {
            var desc = xDoc.CreateElement("Description");
            desc.InnerText = creative.Description;
            element.AppendChild(desc);
        }

        public static void AddAdvertiser(this XmlElement element, XmlDocument xDoc, Campaign campaign)
        {
            var adv = xDoc.CreateElement("Advertiser");
            adv.InnerText = campaign.AdDomains.FirstOrDefault();
            element.AppendChild(adv);
        }

        public static void AddPricing(this XmlElement element, XmlDocument xDoc, Bid bid)
        {
            var price = xDoc.CreateElement("Pricing");
            price.AppendChild(xDoc.CreateCDataSection(Math.Round(bid.CPM, 2, MidpointRounding.AwayFromZero).ToString()));
            price.Attributes.Append(xDoc.CreateVastAttribute("model", "CPM"));
            price.Attributes.Append(xDoc.CreateVastAttribute("currency", "USD"));
            element.AppendChild(price);
        }

        public static void AddErrorUri(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var err = xDoc.CreateElement("Error");
            err.AppendChild(xDoc.CreateCDataSection(POSTBACK_URI + "&a=err&lctx=" + bid.Id.SafeBase64Encode()));
            element.AppendChild(err);
        }

        public static void AddViewableImpression(this XmlElement element, XmlDocument xDoc, Creative creative, Bid bid)
        {
            var viewImp = xDoc.CreateElement("ViewableImpression");
            var viewable = xDoc.CreateElement("Viewable");
            viewable.AppendChild(xDoc.CreateCDataSection(POSTBACK_URI + "&a=view&lctx=" + bid.Id.SafeBase64Encode()));
            var notviewable = xDoc.CreateElement("NotViewable");
            notviewable.AppendChild(xDoc.CreateCDataSection(POSTBACK_URI + "&a=noview&lctx=" + bid.Id.SafeBase64Encode()));
            viewImp.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            viewImp.AppendChild(viewable);
            viewImp.AppendChild(notviewable);
            element.AppendChild(viewImp);
        }

        public static XmlElement CreateCreative(this XmlDocument xDoc, Creative creative)
        {
            var xCreative = xDoc.CreateElement("Creative");
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("adId", creative.Id));
            xCreative.Attributes.Append(xDoc.CreateVastAttribute("sequence", "1"));

            return xCreative;
        }

        public static XmlElement CreateLinear(this XmlDocument xDoc, CreativeContent content)
        {
            var linear = xDoc.CreateElement("Linear");
            if (content.Offset > 0)
                linear.Attributes.Append(xDoc.CreateVastAttribute("skipoffset", TimeSpan.FromSeconds(content.Offset).ToString()));

            return linear;
        }

        public static XmlElement CreateMediaFiles(this XmlDocument xDoc, CreativeContent content, int version = 4)
        {
            var mediaFiles = xDoc.CreateElement("MediaFiles");
            mediaFiles.AppendChild(xDoc.CreateMediaFile(content, content.CreativeUri, version));

            return mediaFiles;
        }

        public static void AddDuration(this XmlElement element, XmlDocument xDoc, CreativeContent content)
        {
            var duration = xDoc.CreateElement("Duration");
            duration.InnerText = TimeSpan.FromSeconds(content.Duration).ToString();
            element.AppendChild(duration);
        }

        public static void AddVideoClicks(this XmlElement element, XmlDocument xDoc, Campaign campaign, Creative creative, Bid bid)
        {
            var clicks = xDoc.CreateElement("VideoClicks");

            var clickThrough = xDoc.CreateElement("ClickThrough");
            clickThrough.AppendChild(xDoc.CreateCDataSection(campaign.LandingPage.Replace("{lctx}", bid.Id.SafeBase64Encode())));
            clickThrough.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            clicks.AppendChild(clickThrough);

            var clickTrack = xDoc.CreateElement("ClickTracking");
            clickTrack.AppendChild(xDoc.CreateCDataSection(POSTBACK_URI + "&a=click&lctx=" + bid.Id.SafeBase64Encode()));
            clickTrack.Attributes.Append(xDoc.CreateVastAttribute("id", SequentialGuid.NextGuid().ToString()));
            clicks.AppendChild(clickTrack);

            element.AppendChild(clicks);
        }

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

        public static XmlAttribute CreateVastAttribute(this XmlDocument xDoc, string name, string value)
        {
            var attr = xDoc.CreateAttribute(name);
            attr.Value = value;

            return attr;
        }
    }
}