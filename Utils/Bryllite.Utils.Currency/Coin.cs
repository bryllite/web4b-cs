using System;

namespace Bryllite.Utils.Currency
{
    // 코인 단위
    public enum CoinUnit
    {
        Beryl,
        ColorlessBeryl,
        BlueBeryl,
        RaspberryBeryl,
        GreenBeryl,
        GoldenBeryl,
        ChrysoBeryl,
        PinkBeryl,
        OrangeBeryl,
        RedBeryl,

        Goshenite = ColorlessBeryl,
        Aquamarine = BlueBeryl,
        Pezzottaite = RaspberryBeryl,
        Emerald = GreenBeryl,
        Heliodor = GoldenBeryl,
        Alexandrite = ChrysoBeryl,
        Morganite = PinkBeryl,
        Bryllite = OrangeBeryl,
        Bixbite = RedBeryl
    }

    public class Coin
    {
        public static readonly string SYMBOL = "BRC";
        public static readonly int PRECISION = 8;
        public static readonly int BLOCK_REWARD = 100;
        public static readonly long HALVING_BLOCK = 1051200;
        public static readonly int BONUS_BLOCK = 360;
        public static readonly decimal BONUS_RATE = 12.5m;

        // beryl per 1 coin
        public static readonly decimal DECIMALS = new decimal(Math.Pow(10, PRECISION));

        // 1유닛당 Beryl
        internal const decimal Beryl = 1;
        internal const decimal Goshenite = 10;
        internal const decimal Aquamarine = 100;
        internal const decimal Pezzottaite = 1000;
        internal const decimal Emerald = 10000;
        internal const decimal Heliodor = 100000;
        internal const decimal Alexandrite = 1000000;
        internal const decimal Morganite = 10000000;
        internal const decimal Bryllite = 100000000;
        internal const decimal Bixbite = 1000000000;

        // 블록 넘버에 해당하는 블록 리워드를 구한다.
        public static ulong GetBlockReward(long number)
        {
            Guard.Assert(number > 0);

            decimal halved = (BLOCK_REWARD / 10) * Math.Min(9, number / HALVING_BLOCK);
            return ToBeryl(BLOCK_REWARD - halved);
        }

        // 블록 넘버에 해당하는 블록 보너스를 구한다.
        public static ulong GetBlockBonus(long number)
        {
            Guard.Assert(number > 0);
            if (number % BONUS_BLOCK != 0) return 0;

            return (ulong)(GetBlockReward(number) * BONUS_RATE);
        }

        // decimal은 눈으로 보여지는 단위( 예: 10.15 BRC, 0.001 BRC )
        // ulong은 내부적으로 처리하는 단위( 예: 110,000,000 Beryl = 1.1 BRC )

        // brc -> beryl
        public static ulong ToBeryl(decimal coin)
        {
            return (ulong)decimal.Multiply(coin, DECIMALS);
        }

        // beryl -> brc
        public static decimal ToCoin(ulong beryl)
        {
            return decimal.Divide(beryl, DECIMALS);
        }

        // 블록 넘버에 해당하는 누적 발행 금액을 구한다
        public static ulong GetCumulativeIssuance(long number)
        {
            ulong issue = 0;

            for (long i = 1; i <= number; i++)
                issue += GetBlockReward(i);

            return issue;
        }
    }
}
