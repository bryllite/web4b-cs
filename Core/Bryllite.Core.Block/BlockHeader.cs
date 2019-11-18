using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Utils.Rlp;
using Newtonsoft.Json.Linq;
using System;

namespace Bryllite.Core
{
    public class BlockHeader : ICloneable
    {
        // network id
        // bryllite.mainnet = 0x00
        // bryllite.cyprus = 0x80
        protected Hex chain = Tx.MainNet;
        public byte Chain
        {
            get { return chain; }
            set { chain = value; }
        }

        // version
        protected Hex version = 0x00;
        public byte Version
        {
            get { return version; }
            set { version = value; }
        }

        // reserved
        protected Hex reserved = null;
        public Hex Reserved
        {
            get { return reserved; }
            set { reserved = value; }
        }

        // timestamp
        protected Hex timestamp = null;
        public int Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        // block number
        protected Hex number = -1;
        public long Number
        {
            get { return number; }
            set { number = value; }
        }

        // coinbase
        protected Hex coinbase = null;
        public Address Coinbase
        {
            get { return coinbase; }
            set { coinbase = value; }
        }

        // witness
        protected Hex witness = null;
        public Address Witness
        {
            get { return witness; }
            set { witness = value; }
        }

        // nonce
        protected Hex nonce = null;
        public byte[] Nonce
        {
            get { return nonce; }
            set { nonce = value; }
        }

        // difficulty
        protected Hex difficulty = null;
        public byte[] Difficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }

        // parent block hash
        protected Hex parent = null;
        public H256 ParentHash
        {
            get { return (byte[])parent; }
            set { parent = (byte[])value; }
        }

        // transaction root hash
        protected Hex txroot;
        public H256 TransactionRoot
        {
            get { return (byte[])txroot; }
            set { txroot = (byte[])value; }
        }

        // state root hash
        protected Hex state;
        public H256 StateRoot
        {
            get { return (byte[])state; }
            set { state = (byte[])value; }
        }

        // data
        protected Hex data;
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        // extra data
        protected Hex extra;
        public byte[] Extra
        {
            get { return extra; }
            set { extra = value; }
        }

        // rlp
        public byte[] Rlp => ToRlp();

        // block header hash
        public H256 Hash => ToHash();


        public BlockHeader()
        {
        }

        protected BlockHeader(byte[] rlp)
        {
            var decoder = new RlpDecoder(rlp);

            chain = decoder.Next();
            version = decoder.Next();
            reserved = decoder.Next();
            timestamp = decoder.Next();
            number = decoder.Next();
            coinbase = decoder.Next();
            witness = decoder.Next();
            nonce = decoder.Next();
            difficulty = decoder.Next();
            parent = decoder.Next();
            txroot = decoder.Next();
            state = decoder.Next();
            data = decoder.Next();
            extra = decoder.Next();
        }

        protected BlockHeader(string rlp) : this(Hex.ToByteArray(rlp))
        {
        }

        protected virtual H256 ToHash()
        {
            return Rlp.Hash256();
        }

        protected virtual byte[] ToRlp()
        {
            return RlpEncoder.EncodeList(chain, version, reserved, timestamp, number, coinbase, witness, nonce, difficulty, parent, txroot, state, data, extra);
        }

        public JObject ToJObject()
        {
            var json = new JObject();

            json.Put("hash", Hash);
            json.Put("number", Hex.ToString(Number, true));
            json.Put("chain", Hex.ToString(Chain, true));
            json.Put("version", Hex.ToString(Version, true));
            json.Put("timestamp", Hex.ToString(Timestamp, true));
            json.Put<string>("coinbase", Coinbase);
            json.Put<string>("witness", Witness);
            json.Put("parentHash", ParentHash);
            json.Put("transactionRoot", TransactionRoot);
            json.Put("stateRoot", StateRoot);
            json.Put("nonce", Hex.ToString(Nonce));
            json.Put("difficulty", Hex.ToString(Difficulty));
            json.Put("data", Hex.ToString(Data));
            json.Put("extra", Hex.ToString(Extra));

            return json;
        }


        public BlockHeader Clone()
        {
            return new BlockHeader(Rlp);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override string ToString()
        {
            return Hash;
        }

        public override int GetHashCode()
        {
            return Rlp.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;

            return obj is BlockHeader header ? Hash.Equals(header.Hash) : false;
        }

        public static bool operator ==(BlockHeader left, BlockHeader right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(BlockHeader left, BlockHeader right)
        {
            return !(left == right);
        }

        public static BlockHeader Parse(byte[] rlp)
        {
            return new BlockHeader(rlp);
        }

        public static BlockHeader Parse(string rlp)
        {
            return new BlockHeader(rlp);
        }

        public static bool TryParse(byte[] rlp, out BlockHeader header)
        {
            try
            {
                header = Parse(rlp);
                return true;
            }
            catch
            {
                header = null;
                return false;
            }
        }

        public static bool TryParse(string rlp, out BlockHeader header)
        {
            try
            {
                header = Parse(rlp);
                return true;
            }
            catch
            {
                header = null;
                return false;
            }
        }
    }
}
