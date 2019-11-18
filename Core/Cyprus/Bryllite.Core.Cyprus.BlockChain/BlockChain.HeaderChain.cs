using Bryllite.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Cyprus
{
    public partial class BlockChain
    {
        public static readonly byte[] HEADER_PREFIX = Encoding.UTF8.GetBytes("h");
        public static readonly byte[] NUMBER_PREFIX = Encoding.UTF8.GetBytes("n");
        public static readonly byte[] LATEST_KEY = Encoding.UTF8.GetBytes("latest");

        // 가장 최근 블록 해시
        private H256 latest;

        // 가장 최근 블록 헤더
        public BlockHeader Latest => GetHeader(latest);


        // header key
        private byte[] ToHeaderKey(H256 hash)
        {
            return HEADER_PREFIX.Append(hash?.Value);
        }

        // number key
        private byte[] ToNumberKey(long number)
        {
            return NUMBER_PREFIX.Append(Hex.ToByteArray(number, true));
        }

        // block number to block hash
        public H256 ToBlockHash(long number)
        {
            return chaindb.Get(ToNumberKey(number));
        }

        // 블록 해시로 블록 헤더를 얻는다
        public BlockHeader GetHeader(H256 hash)
        {
            return BlockHeader.TryParse(chaindb.Get(ToHeaderKey(hash)), out BlockHeader header) ? header : null;
        }

        public bool TryGetHeader(H256 hash, out BlockHeader header)
        {
            try
            {
                header = GetHeader(hash);
                return !ReferenceEquals(header, null);
            }
            catch
            {
                header = null;
                return false;
            }
        }

        // 블록 넘버로 블록 헤더를 얻는다
        public BlockHeader GetHeader(long number)
        {
            return GetHeader(ToBlockHash(number));
        }

        public bool TryGetHeader(long number, out BlockHeader header)
        {
            try
            {
                header = GetHeader(number);
                return !ReferenceEquals(header, null);
            }
            catch
            {
                header = null;
                return false;
            }
        }

        // 헤더를 체인에 기록한다
        private void Write(BlockHeader header)
        {
            var hash = header.Hash;
            var number = header.Number;

            // 헤더 저장
            chaindb.Put(ToHeaderKey(hash), header.Rlp);

            // number -> hash lookup
            chaindb.Put(ToNumberKey(number), hash);

            // 가장 최신 헤더
            chaindb.Put(LATEST_KEY, (latest = hash));
        }
    }
}
