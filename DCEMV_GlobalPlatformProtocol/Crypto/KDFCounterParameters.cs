/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using System;

namespace DCEMV.GlobalPlatformProtocol
{
    public class KDFCounterParameters : IDerivationParameters
    {
        private byte[] ki;
        private byte[] fixedInputDataCounterPrefix;
        private byte[] fixedInputDataCounterSuffix;
        private int r;

        public KDFCounterParameters(byte[] var1, byte[] var2, int var3)
            : this(var1, (byte[])null, var2, var3)
        {

        }

        public KDFCounterParameters(byte[] var1, byte[] var2, byte[] var3, int var4)
        {
            if (var1 == null)
            {
                throw new Exception("A KDF requires Ki (a seed) as input");
            }
            else
            {
                this.ki = Arrays.Clone(var1);
                if (var2 == null)
                {
                    this.fixedInputDataCounterPrefix = new byte[0];
                }
                else
                {
                    this.fixedInputDataCounterPrefix = Arrays.Clone(var2);
                }

                if (var3 == null)
                {
                    this.fixedInputDataCounterSuffix = new byte[0];
                }
                else
                {
                    this.fixedInputDataCounterSuffix = Arrays.Clone(var3);
                }

                if (var4 != 8 && var4 != 16 && var4 != 24 && var4 != 32)
                {
                    throw new Exception("Length of counter should be 8, 16, 24 or 32");
                }
                else
                {
                    this.r = var4;
                }
            }
        }

        public byte[] GetKI()
        {
            return this.ki;
        }

        public byte[] GetFixedInputData()
        {
            return Arrays.Clone(this.fixedInputDataCounterSuffix);
        }

        public byte[] GetFixedInputDataCounterPrefix()
        {
            return Arrays.Clone(this.fixedInputDataCounterPrefix);
        }

        public byte[] GetFixedInputDataCounterSuffix()
        {
            return Arrays.Clone(this.fixedInputDataCounterSuffix);
        }

        public int GetR()
        {
            return this.r;
        }
    }

    public class KDFCounterBytesGenerator : IMacDerivationFunction
    {
        private static BigInteger INTEGER_MAX = BigInteger.ValueOf(2147483647L);
        private static BigInteger TWO = BigInteger.ValueOf(2L);
        private IMac prf;
        private int h;
        private byte[] fixedInputDataCtrPrefix;
        private byte[] fixedInputData_afterCtr;
        private int maxSizeExcl;
        private byte[] ios;
        private int generatedBytes;
        private byte[] k;

        public IDigest Digest => throw new NotImplementedException();

        public KDFCounterBytesGenerator(IMac var1)
        {
            this.prf = var1;
            this.h = var1.GetMacSize();
            this.k = new byte[this.h];
        }

        public void Init(IDerivationParameters var1)
        {
            if (!(var1 is KDFCounterParameters))
            {
                throw new Exception("Wrong type of arguments given");
            }
            else
            {
                KDFCounterParameters var2 = (KDFCounterParameters)var1;
                this.prf.Init(new KeyParameter(var2.GetKI()));
                this.fixedInputDataCtrPrefix = var2.GetFixedInputDataCounterPrefix();
                this.fixedInputData_afterCtr = var2.GetFixedInputDataCounterSuffix();
                int var3 = var2.GetR();
                this.ios = new byte[var3 / 8];
                BigInteger var4 = TWO.Pow(var3).Multiply(BigInteger.ValueOf((long)this.h));
                this.maxSizeExcl = var4.CompareTo(INTEGER_MAX) == 1 ? 2147483647 : var4.IntValue;
                this.generatedBytes = 0;
            }
        }

        public IMac GetMac()
        {
            return this.prf;
        }

        public int GenerateBytes(byte[] var1, int var2, int var3)
        {
            int var4 = this.generatedBytes + var3;
            if (var4 >= 0 && var4 < this.maxSizeExcl)
            {
                if (this.generatedBytes % this.h == 0)
                {
                    this.GenerateNext();
                }

                int var6 = this.generatedBytes % this.h;
                int var7 = this.h - this.generatedBytes % this.h;
                int var8 = Math.Min(var7, var3);
                Array.Copy(this.k, var6, var1, var2, var8);
                this.generatedBytes += var8;
                int var5 = var3 - var8;

                for (var2 += var8; var5 > 0; var2 += var8)
                {
                    this.GenerateNext();
                    var8 = Math.Min(this.h, var5);
                    Array.Copy(this.k, 0, var1, var2, var8);
                    this.generatedBytes += var8;
                    var5 -= var8;
                }

                return var3;
            }
            else
            {
                throw new DataLengthException("Current KDFCTR may only be used for " + this.maxSizeExcl + " bytes");
            }
        }

        private void GenerateNext()
        {
            int var1 = this.generatedBytes / this.h + 1;
            switch (this.ios.Length)
            {
                case 4:
                    this.ios[0] = (byte)TripleShift(var1, 24);
                    goto case 3;
                case 3:
                    this.ios[this.ios.Length - 3] = (byte)TripleShift(var1, 16);
                    goto case 2;
                case 2:
                    this.ios[this.ios.Length - 2] = (byte)TripleShift(var1, 8);
                    goto case 1;
                case 1:
                    this.ios[this.ios.Length - 1] = (byte)var1;
                    this.prf.BlockUpdate(this.fixedInputDataCtrPrefix, 0, this.fixedInputDataCtrPrefix.Length);
                    this.prf.BlockUpdate(this.ios, 0, this.ios.Length);
                    this.prf.BlockUpdate(this.fixedInputData_afterCtr, 0, this.fixedInputData_afterCtr.Length);
                    this.prf.DoFinal(this.k, 0);
                    return;
                default:
                    throw new Exception("Unsupported size of counter i");
            }
        }

        private static int TripleShift(int n, int s)
        {
            if (n >= 0)
                return n >> s;
            return (n >> s) + (2 << ~s);
        }
    }

    public interface IMacDerivationFunction : IDerivationFunction
    {
        IMac GetMac();
    }
}
