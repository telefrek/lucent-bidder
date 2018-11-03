using System;
using System.IO;
using System.Text;
using Lucent.Common.Protobuf;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Represents the context for a given bid
    /// </summary>
    public class BidContext
    {
        /// <summary>
        /// Parse a bid context from a given string
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static BidContext Parse(string encoded)
        {
            // Decode the string
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded.SafeBase64Decode()));

            // Read the raw objects
            var context = new BidContext();

            // Read the encoded GUIDs
            context.ExchangeId = decoded.Substring(0, 22).DecodeGuid();
            context.CampaignId = decoded.Substring(22, 22).DecodeGuid();
            context.BidId = decoded.Substring(44, 22).DecodeGuid();

            // Get the other packed values as a protobuf stream
            var packed = Convert.FromBase64String(decoded.Substring(66));
            using (var protoReader = new ProtobufReader(new MemoryStream(packed)))
            {
                context.CPM = protoReader.ReadDouble();
                context.BidDate = DateTime.FromFileTimeUtc(protoReader.ReadInt64());
                context.Operation = (BidOperation)protoReader.ReadInt32();
            }

            return context;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid ExchangeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid BidId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime BidDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double CPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BidOperation Operation { get; set; }

        /// <summary>
        /// Get the operation parameter
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public string GetOperationString(BidOperation operation)
        {
            this.Operation = operation;
            return this.ToString();
        }

        /// <summary>
        /// Override the default ToString method to get encoded
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // Encode the guids
            sb.Append(ExchangeId.EncodeGuid());
            sb.Append(CampaignId.EncodeGuid());
            sb.Append(BidId.EncodeGuid());

            // Wre the additional values using protobuf
            using (var ms = new MemoryStream())
            {
                using (var protoWriter = new ProtobufWriter(ms, true))
                {
                    protoWriter.Write(CPM);
                    protoWriter.Write(BidDate.ToFileTimeUtc());
                    protoWriter.Write((int)Operation);
                    protoWriter.Flush();
                }

                sb.Append(Convert.ToBase64String(ms.ToArray()));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())).SafeBase64Encode();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BidOperation
    {
        /// <value></value>
        Unknown = 0,
        /// <value></value>
        Win,
        /// <value></value>
        Loss,
        /// <value></value>
        Impression,
        /// <value></value>
        Clicked,
        /// <value></value>
        Action,
    }
}