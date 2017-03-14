using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Update : Structure
    {
        private ushort mType;
        private UInt16 mWithdrawnRoutesLength;
        private string mWithdrawnRoutes;
        private ushort mIpPrefixLength;
        private string mIpPrefix;
        private ushort mTotalPathAttributeLength;
        private string mAttributePath;
        private UInt64 mAttributeType;
        private UInt32 mAttributeLength;
        private string mAttribute;
        private UInt32 mAttrFlags;
        private ushort mTypeCode;
        private ushort mOrigin;
        private string mAsPath;
        private ushort mPathSegmentType;
        private ushort mPathSegmentLength;
        private string mPathSegmentValue;
        private IPAddress mNextHop;
        private string mMultiExitDisc;
        private UInt64 mLocalPref;
        private ushort mAutomaticAggregate;
        private string mAggregator;
        private string mNetworkLayerreachability;
        private ushort mNlrLength;
        private string mNlrPrefix;

        public Update(UInt16 withdrawRouteLength, string withdrawalRoute, ushort ipPrefixLength, string ipPrefix, ushort totalPathAttributeLength, UInt32 attributeLength, UInt32 attrFlags, ushort typeCode, string attribute, ushort pathSegmentType, ushort pathSegmentLength, string pathSegmentValue, ushort nlrLength, string nlrPrefix)
            : base((ushort)(38 + 2 + withdrawalRoute.Length + 4 + 2 + ipPrefix.Length + 4 + 4 + 4 + 4 + 4 + attribute.Length + 4 + 4 + pathSegmentValue.Length + 4 + nlrPrefix.Length), 184)
        {
            Type = 2;
            WithdrawRouteLength = withdrawRouteLength;
            WithdrawRoutes = withdrawalRoute;
            IpPrefixLength = ipPrefixLength;
            IpPrefix = ipPrefix;
            TotalPathAttributeLength = totalPathAttributeLength;
            AttrFlags = attrFlags;
            TypeCode = typeCode;
            AttributeLength = attributeLength;
            Attribute = attribute;
            PathSegmentType = pathSegmentType;
            PathSegmentLength = pathSegmentLength;
            PathSegmentValue = pathSegmentValue;
            NlrLength = nlrLength;
            NlrPrefix = nlrPrefix;
        }
        public ushort Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
                writeType(value, 38);
            }
        }
        public UInt16 WithdrawRouteLength
        {
            get
            {
                return mWithdrawnRoutesLength;
            }
            set
            {
                mWithdrawnRoutesLength = value;
                writeWithdrawRoutesLn(value, 40);
            }
        }
        public string WithdrawRoutes
        {
            get
            {
                return mWithdrawnRoutes;
            }
            set
            {
                mWithdrawnRoutes = value;
                writeWithdrawalRoutes(value, 42);
            }
        }
        public ushort IpPrefixLength
        {
            get
            {
                return mIpPrefixLength;
            }
            set
            {
                mIpPrefixLength = value;
                writeIpPrefixLength(value, 51);
            }
        }
        public string IpPrefix
        {
            get
            {
                return mIpPrefix;
            }
            set
            {
                mIpPrefix = value;
                writeIpPrefix(value, 53);
            }
        }
        public ushort TotalPathAttributeLength
        {
            get
            {
                return mTotalPathAttributeLength;
            }
            set
            {
                mTotalPathAttributeLength = value;
                writeTotalPath(value, 62);
            }
        }
        public UInt32 AttributeLength
        {
            get
            {
                return mAttributeLength;
            }
            set
            {
                mAttributeLength = value;
                writeAttributeLength(value, 64);
            }
        }
        public string Attribute
        {
            get
            {
                return mAttribute;
            }
            set
            {
                mAttribute = value;
                writeAttribute(value, 66);
            }
        }
        public UInt32 AttrFlags
        {
            get
            {
                return mAttrFlags;
            }
            set
            {
                mAttrFlags = value;
                writeAttributeFlags(value, 75);
            }
        }
        public ushort TypeCode
        {
            get
            {
                return mTypeCode;
            }
            set
            {
                mTypeCode = value;
                // HUOMIO HUOMIO, if not working, the issue is here (wrong object)
                writeCodeType(value, 77);
            }
        }
        public ushort PathSegmentType
        {
            get
            {
                return mPathSegmentType;
            }
            set
            {
                mPathSegmentType = value;
                writePathSegmentType(value, 79);
            }
        }
        public ushort PathSegmentLength
        {
            get
            {
                return mPathSegmentLength;
            }
            set
            {
                mPathSegmentLength = value;
                writePathSegmentLength(value, 81);
            }
        }
        public string PathSegmentValue
        {
            get
            {
                return mPathSegmentValue;
            }
            set
            {
                mPathSegmentValue = value;
                writePathSegmentValue(value, 83);
            }
        }
        public ushort NlrLength
        {
            get
            {
                return mNlrLength;
            }
            set
            {
                mNlrLength = value;
                writeNlrLength(value, 85);
            }
        }
        public string NlrPrefix
        {
            get
            {
                return mNlrPrefix;
            }
            set
            {
                mNlrPrefix = value;
                writeNlrPrefix(value, 87);
            }
        }
    }
}
