using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Utils.Rlp;
using Newtonsoft.Json.Linq;
using System;

namespace Bryllite.Core
{
    // 트랜잭션 클래스
    public class Tx : ICloneable
    {
        // max tx size
        public static readonly int MAX_SIZE = 32 * 1024;

        // network id
        public const byte BrylliteNet = 0x00;
        public const byte CyprusNet = 0x80;

        // cyprus tx opcode
        public const byte Transfer = 0x80;
        public const byte Withdraw = 0x81;
        public const byte Deposit = 0x82;

        // network id
        // bryllite.mainnet < 0x80
        // bryllite.cyprus >= 0x80
        protected Hex chain = BrylliteNet;
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

        // recipient address
        protected Address to = null;
        public Address To
        {
            get { return to; }
            set { to = value; }
        }

        // value
        protected Hex value = 0x00;
        public ulong Value
        {
            get { return value; }
            set { this.value = value; }
        }

        // gas
        protected Hex gas = 0x00;
        public ulong Gas
        {
            get { return gas; }
            set { gas = value; }
        }

        // nonce
        protected Hex nonce = 0x00;
        public ulong Nonce
        {
            get { return nonce; }
            set { nonce = value; }
        }

        // data ( for contract )
        protected Hex data = null;
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        // extra data 
        protected Hex extra = null;
        public byte[] Extra
        {
            get { return extra; }
            set { extra = value; }
        }

        // seal
        protected Hex seal = null;
        public Signature Seal
        {
            get { return (byte[])seal; }
            set { seal = (byte[])value; }
        }

        public byte[] R => Seal.R;
        public byte[] S => Seal.S;
        public byte V => Seal.V;

        // meta data ( not affected to txid, hash )
        protected Hex metadata = null;
        public byte[] Metadata
        {
            get { return metadata; }
            set { metadata = value; }
        }

        // txid
        public H256 Txid => ToTxid();

        // sig hash
        public H256 SigHash => ToHash();

        // sender address
        public Address From => Sender();

        // cost
        public ulong Cost => Value + Gas;

        // rlp stream
        public byte[] Rlp => ToRlp();

        // tx size
        public int Size => Rlp.Length;

        // cyprus net?
        public bool IsCyprusChain => Chain >= CyprusNet;

        // mainnet tx
        public Tx()
        {
        }

        protected Tx(byte[] rlp)
        {
            var decoder = new RlpDecoder(rlp);

            chain = decoder.Next();
            version = decoder.Next();
            to = decoder.Next();
            value = decoder.Next();
            gas = decoder.Next();
            nonce = decoder.Next();
            data = decoder.Next();
            extra = decoder.Next();

            // signature rsv
            seal = decoder.Next();

            // metadata
            metadata = decoder.Next();
        }

        protected Tx(string rlp) : this(Hex.ToByteArray(rlp))
        {
        }

        protected virtual H256 ToHash()
        {
            return RlpEncoder.EncodeList(chain, version, to, value, gas, nonce, data, extra).Hash256();
        }

        protected virtual byte[] ToRlp()
        {
            return RlpEncoder.EncodeList(chain, version, to, value, gas, nonce, data, extra, seal, metadata);
        }

        protected virtual H256 ToTxid()
        {
            return RlpEncoder.EncodeList(chain, version, to, value, gas, nonce, data, extra, seal).Hash256();
        }

        public JObject ToJObject()
        {
            var json = new JObject();

            json.Put<string>("hash", Txid);
            json.Put<string>("chain", Hex.ToString(Chain, true));
            json.Put<string>("version", Hex.ToString(Version, true));
            json.Put<string>("from", From);
            json.Put<string>("to", To);
            json.Put<string>("value", Hex.ToString(Value, true));
            json.Put<string>("gas", Hex.ToString(Gas, true));
            json.Put<string>("nonce", Hex.ToString(Nonce, true));
            json.Put<string>("input", Hex.ToString(Data));
            json.Put<string>("extra", Hex.ToString(Extra));
            json.Put<string>("metadata", Hex.ToString(Metadata));
            json.Put<string>("v", Hex.ToString(V));
            json.Put<string>("r", Hex.ToString(R));
            json.Put<string>("s", Hex.ToString(S));
            json.Put<string>("size", Hex.ToString(Rlp.Length, true));

            return json;
        }


        // sender address
        public Address Sender()
        {
            try
            {
                return Seal.GetPublicKey(SigHash).Address;
            }
            catch
            {
                return null;
            }
        }

        public Tx Sign(PrivateKey signer)
        {
            seal = (byte[])signer.Sign(SigHash);
            return this;
        }

        public override string ToString()
        {
            return Txid;
        }

        public override int GetHashCode()
        {
            return Rlp.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;

            return obj is Tx tx ? Txid.Equals(tx.Txid) : false;
        }

        public static bool operator ==(Tx left, Tx right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Tx left, Tx right)
        {
            return !(left == right);
        }

        public Tx Clone()
        {
            return new Tx(Rlp);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public static Tx Parse(byte[] rlp)
        {
            return new Tx(rlp);
        }

        public static Tx Parse(string rlp)
        {
            return new Tx(rlp);
        }

        public static bool TryParse(byte[] rlp, out Tx tx)
        {
            try
            {
                tx = Parse(rlp);
                return !ReferenceEquals(tx, null);
            }
            catch
            {
                tx = null;
                return false;
            }
        }

        public static bool TryParse(string rlp, out Tx tx)
        {
            try
            {
                tx = Parse(rlp);
                return true;
            }
            catch
            {
                tx = null;
                return false;
            }
        }

        public static implicit operator H256(Tx tx)
        {
            return tx?.Txid;
        }
    }
}
