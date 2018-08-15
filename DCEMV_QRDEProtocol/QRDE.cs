
using DataFormatters;
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEMV_QRDEProtocol
{
    public enum TagId
    {
        _00,
        _01,
        _02,
        _03,
        _04,
        _05,
        _06,
        _07,
        _08,
        _09,
        _10,
        _11,
        _12,
        _13,
        _14,
        _15,
        _16,
        _17,
        _18,
        _19,
        _20,
        _21,
        _22,
        _23,
        _24,
        _25,
        _26,
        _27,
        _28,
        _29,
        _30,
        _31,
        _32,
        _33,
        _34,
        _35,
        _36,
        _37,
        _38,
        _39,
        _40,
        _41,
        _42,
        _43,
        _44,
        _45,
        _46,
        _47,
        _48,
        _49,
        _50,
        _51,
        _52,
        _53,
        _54,
        _55,
        _56,
        _57,
        _58,
        _59,
        _60,
        _61,
        _62,
        _63,
        _64,
        _65,
        _66,
        _67,
        _68,
        _69,
        _70,
        _71,
        _72,
        _73,
        _74,
        _75,
        _76,
        _77,
        _78,
        _79,
        _80,
        _81,
        _82,
        _83,
        _84,
        _85,
        _86,
        _87,
        _88,
        _89,
        _90,
        _91,
        _92,
        _93,
        _94,
        _95,
        _96,
        _97,
        _98,
        _99,
        None
    }
    public interface IQRMetaDataSource
    {
        DataFormatterBase GetFormatter(TagId tagLabel, TagId tagLabelParent);
        string GetName(TagId tagLabel, TagId tagLabelParent);
        bool IsTemplate(TagId tagLabel, TagId tagLabelParent);
    }
    public sealed class QRMetaDataSourceSingleton
    {
        public static readonly QRMetaDataSourceSingleton Instance = new QRMetaDataSourceSingleton();

        public IQRMetaDataSource DataSource { get; set; }

        static QRMetaDataSourceSingleton()
        {

        }
        private QRMetaDataSourceSingleton()
        {

        }
    }
    public abstract class QRDataElementBase
    {
        public abstract int Deserialize(string rawQR, int pos);
        public abstract string Serialize();
        public static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
    public class QRDEList
    {
        protected List<QRDE> listToManage;
        public QRDEList()
        {
            listToManage = new List<QRDE>();
        }

        public void Initialize()
        {
            listToManage.Clear();
        }

        public string Serialize()
        {
            List<string> result = new List<string>();
            foreach (QRDE de in listToManage)
                result.Add(de.Serialize());
            return new string(result.SelectMany(a => a).ToArray());
        }

        public void Deserialize(string rawTLV)
        {
            for (int i = 0; i < rawTLV.Length;)
                AddToList(QRDE.Create(rawTLV, ref i));
        }

        public IEnumerator<QRDE> GetEnumerator()
        {
            return listToManage.GetEnumerator();
        }

        public virtual void AddToList(QRDE tlv)
        {
            List<QRDE> result = listToManage.Where(x => x.Tag == tlv.Tag).ToList();
            if (result.Count != 0)
            {
                QRDE tlvFound = result[0];
                tlvFound.Value = tlv.Value;
            }
            else
                listToManage.Add(tlv);
        }

        public QRDE Get(TagId tag)
        {
            List<QRDE> result = listToManage.Where(x => x.Tag == tag).ToList();
            if (result.Count == 0)
                return null;
            if (result.Count > 1)
                throw new Exception("TLVList:GetTLV Duplicate tag found:" + result[0].Tag);
            return result[0];
        }

        public string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbTab = new StringBuilder();
            for (int j = 0; j <= depth; j++)
                sbTab.Append("\t");

            for (int i = 0; i < listToManage.Count; i++)
                if (i == listToManage.Count - 1)
                    sb.Append(sbTab.ToString() + listToManage[i].ToPrintString(ref depth));
                else
                    sb.AppendLine(sbTab.ToString() + listToManage[i].ToPrintString(ref depth));

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listToManage.Count; i++)
                sb.Append(listToManage[i].ToString());
            return sb.ToString();
        }

        public int Count { get { return listToManage.Count; } }
    }
    public class QRDE : QRDataElementBase
    {
        private QRDEList children;
        public QRDEList Children
        {
            get
            {
                if (IsTemplate)
                    return children;
                else
                    throw new Exception("Cannot add child to non constructed element");
            }
            protected set
            {
                children = value;
            }
        }
        public bool IsTemplate { get; set; }
        public int Length { get; set; }
        public TagId Tag { get; set; }
        public TagId TagParent { get; set; }
        public string Value { get; set; }

        public QRDE()
        {
            Tag = TagId.None;
            TagParent = TagId.None;
            children = new QRDEList();
        }

        public override int Deserialize(string rawTlv, int pos)
        {
            if (rawTlv.Length == 0)
                return 0;

            children = new QRDEList();

            Tag = (TagId)GetEnum(typeof(TagId), Convert.ToInt16(rawTlv.Substring(pos, 2)));
            pos = pos + 2;
            Length = Convert.ToInt16(rawTlv.Substring(pos, 2));
            pos = pos + 2;
            Value = rawTlv.Substring(pos, Length);
            pos = pos + Length;
            IsTemplate = QRMetaDataSourceSingleton.Instance.DataSource.IsTemplate(Tag, TagParent);
            //DataFormatterBase formatter = QRMetaDataSourceSingleton.Instance.DataSource.GetFormatter(Tag, TagParent);
            if (IsTemplate)
            {
                for (int i = 0; i < Value.Length;)
                {
                    QRDE child = new QRDE
                    {
                        TagParent = Tag
                    };
                    i = child.Deserialize(Value, i);
                    Children.AddToList(child);
                }
            }
            return pos;
        }
        public override string Serialize()
        {
            List<string> result = new List<string>();
            int length = 0;

            result.Add(String.Format("{0:00}", (int)Tag));
            if (children.Count > 0)
            {
                List<string> resultChildren = new List<string>();
                foreach (QRDE tlv in children)
                {
                    string tlvSer = tlv.Serialize();
                    resultChildren.Add(tlvSer);
                    length = length + tlvSer.Length;
                }
                Length = length;
                result.Add(String.Format("{0:00}", Length));
                result.Add(new string(resultChildren.SelectMany(a => a).ToArray()));
            }
            else
            {
                result.Add(String.Format("{0:00}", Length) + Value);
            }
            return new string(result.SelectMany(a => a).ToArray());
        }
        public static QRDE Create(string rawTLV, ref int pos)
        {
            QRDE tlv = new QRDE();
            pos = tlv.Deserialize(rawTLV, pos);
            return tlv;
        }
        public static QRDE Create(EMVQRTagMeta tagMeta, QRDE parent = null)
        {
            return Create(tagMeta, "", parent);
        }
        public static QRDE Create(EMVQRTagMeta tagMeta, string value, QRDE parent = null)
        {
            if (!tagMeta.DataFormatter.Validate(Formatting.ASCIIStringToByteArray(value)))
                throw new Exception("Invalid data value for tag:" + tagMeta.Name);

            QRDE qde = new QRDE() { Tag = tagMeta.Tag, Length = value.Length, Value = value };
            qde.IsTemplate = QRMetaDataSourceSingleton.Instance.DataSource.IsTemplate(qde.Tag, qde.TagParent);

            if (parent != null)
            {
                if (!parent.IsTemplate)
                    throw new Exception("Cannot add child to non template QDE");

                qde.TagParent = parent.Tag;
                parent.Children.AddToList(qde);
            }
            else
                qde.TagParent = TagId.None;

            return qde;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string tagName = QRMetaDataSourceSingleton.Instance.DataSource.GetName(Tag, TagParent);

            sb.Append((int)Tag);
            sb.Append(" (" + tagName + ") ");
            //sb.Append(Length.ToString());

            if (children.Count == 0)
                sb.Append(" " + Value);
            else
                sb.Append(" V:[" + children.ToString() + "]");

            return sb.ToString();
        }
        public virtual string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = QRMetaDataSourceSingleton.Instance.DataSource.GetName(Tag, TagParent);

            //string formatter = "{0,-75}";

            sb.Append((
                "T:[" + String.Format("{0:00}", (int)Tag) + "] L:[" +
                String.Format("{0:00}", Length) + "] " + tagName).PadRight(55, '.'));

            if (children.Count == 0)
            {
                sb.Append(" " + Value);
            }
            else
            {
                depth++;
                sb.AppendLine(" V:[");
                sb.Append(children.ToPrintString(ref depth));
                sb.Append("]");
                depth--;
            }

            return sb.ToString();
        }
    }

    public class EMVQRTagMeta
    {
        public TagId Tag { get; }
        public DataFormatterBase DataFormatter { get; }
        public List<EMVQRTagMeta> TagParents { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsTemplate { get; }

        public EMVQRTagMeta(TagId tagId, string name, List<EMVQRTagMeta> tagParents, DataFormatterBase dataFormatter, string description, bool isTemplate = false)
        {
            Tag = tagId;
            Name = name;
            TagParents = tagParents;
            DataFormatter = dataFormatter;
            Description = description;
            IsTemplate = isTemplate;
        }
    }
}
