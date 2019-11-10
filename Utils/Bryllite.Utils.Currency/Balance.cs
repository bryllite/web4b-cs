using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Currency
{
    public class Balance
    {
        public static readonly Balance MaxValue = new Balance(ulong.MaxValue);
        public static readonly Balance MinValue = new Balance(ulong.MinValue);
        public static readonly Balance Zero = new Balance(0);
        public static readonly Balance One = new Balance(1);

        // Beryl 단위의 잔고
        private ulong balance;

        // byte[] for serialize
        public byte[] Bytes => Hex.ToByteArray(balance);

        public ulong Beryl => balance;
        public decimal Goshenite => balance / Coin.Goshenite;
        public decimal Aquamarine => balance / Coin.Aquamarine;
        public decimal Pezzottaite => balance / Coin.Pezzottaite;
        public decimal Emerald => balance / Coin.Emerald;
        public decimal Heliodor => balance / Coin.Heliodor;
        public decimal Alexandrite => balance / Coin.Alexandrite;
        public decimal Morganite => balance / Coin.Morganite;
        public decimal Bryllite => balance / Coin.Bryllite;
        public decimal Bixbite => balance / Coin.Bixbite;

        public Balance()
        {
        }

        public Balance(ulong balance)
        {
            this.balance = balance;
        }

        public Balance(decimal balance)
        {
            this.balance = Coin.ToBeryl(balance);
        }

        public Balance(byte[] bytes)
        {
            balance = Hex.ToNumber<ulong>(bytes);
        }

        public string ToString(CoinUnit unit, string format = "#,#.########")
        {
            if (balance == 0) return balance.ToString();

            switch (unit)
            {
                case CoinUnit.Beryl:
                    return Beryl.ToString(format);
                case CoinUnit.Goshenite:
                    return Goshenite.ToString(format);
                case CoinUnit.Aquamarine:
                    return Aquamarine.ToString(format);
                case CoinUnit.Pezzottaite:
                    return Pezzottaite.ToString(format);
                case CoinUnit.Emerald:
                    return Emerald.ToString(format);
                case CoinUnit.Heliodor:
                    return Heliodor.ToString(format);
                case CoinUnit.Alexandrite:
                    return Alexandrite.ToString(format);
                case CoinUnit.Morganite:
                    return Morganite.ToString(format);
                case CoinUnit.Bryllite:
                    return Bryllite.ToString(format);
                case CoinUnit.Bixbite:
                    return Bixbite.ToString(format);
                default: break;
            }

            return ToString();
        }

        public override string ToString()
        {
            return ToString(CoinUnit.Bryllite);
        }

        public override int GetHashCode()
        {
            return balance.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Balance;
            return !ReferenceEquals(o, null) && balance == o.balance;
        }

        public static bool operator ==(Balance left, Balance right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Balance left, Balance right)
        {
            return !(left == right);
        }

        public static bool operator >(Balance left, Balance right)
        {
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return true;
            return left.balance > right.balance;
        }

        public static bool operator <(Balance left, Balance right)
        {
            if (ReferenceEquals(right, null)) return false;
            if (ReferenceEquals(left, null)) return true;
            return left.balance < right.balance;
        }

        public static bool operator >=(Balance left, Balance right)
        {
            return (left > right || left == right);
        }

        public static bool operator <=(Balance left, Balance right)
        {
            return (left < right || left == right);
        }

        public static Balance operator +(Balance left, Balance right)
        {
            ulong b1 = left?.balance ?? 0;
            ulong b2 = right?.balance ?? 0;
            return new Balance(b1 + b2);
        }

        public static Balance operator -(Balance left, Balance right)
        {
            ulong b1 = left?.balance ?? 0;
            ulong b2 = right?.balance ?? 0;
            return new Balance(b1 - b2);
        }


        public static implicit operator Balance(ulong beryl)
        {
            return new Balance(beryl);
        }

        public static implicit operator ulong(Balance balance)
        {
            return balance.balance;
        }

        public static implicit operator byte[] (Balance balance)
        {
            return balance.Bytes;
        }
    }
}
