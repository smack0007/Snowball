using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PixelEngineDotNet.Images
{
    public static class PNG
    {
        public static readonly byte[] HeaderBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private enum ChunkType
        {
            Other = -1,
            IHDR,
            PLTE,
            IDAT,
            IEND
        }

        public static bool IsPNGImage(Stream stream)
        {
            var position = stream.Position;
            bool result = true;

            try
            {
                var header = new byte[HeaderBytes.Length];
                stream.Read(header, 0, header.Length);

                for (int i = 0; i < HeaderBytes.Length; i++)
                {
                    if (header[i] != HeaderBytes[i])
                    {
                        result = false;
                        break;
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                stream.Position = position;
            }

            return result;
        }

        public static ImageData Load(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return Load(file);
            }
        }

        public static ImageData Load(Stream stream)
        {
            if (!IsPNGImage(stream))
            {
                throw new PixelEngineDotNetException("PNG header incorrect.");
            }

            stream.Position += HeaderBytes.Length;

            uint width = 0;
            uint height = 0;
            byte bitDepth = 0;
            byte colorType = 0;
            byte compressionMethod = 0;
            byte filterMethod = 0;
            byte interlaceMethod = 0;

            int bytesPerPixel = 0;
            byte[] rawPixels = Array.Empty<Byte>();
            byte[]? idat = null;

            while (true)
            {
                var chunkHeader = new byte[8];
                stream.Read(chunkHeader, 0, chunkHeader.Length);

                var chunkSize = BinaryHelper.ReadBigEndianUInt32(chunkHeader, 0);
                var chunkType = ReadChunkType(chunkHeader, 4);

                if (chunkType == ChunkType.IEND)
                {
                    break;
                }

                var chunkData = new byte[chunkSize];
                stream.Read(chunkData, 0, (int)chunkSize);

                switch (chunkType)
                {
                    case PNG.ChunkType.IHDR:
                        width = BinaryHelper.ReadBigEndianUInt32(chunkData, 0);
                        height = BinaryHelper.ReadBigEndianUInt32(chunkData, 4);
                        bitDepth = chunkData[8];
                        colorType = chunkData[9];
                        compressionMethod = chunkData[10];
                        filterMethod = chunkData[11];
                        interlaceMethod = chunkData[12];

                        switch (colorType)
                        {
                            case 0: // GRAY8
                                bytesPerPixel = 1;
                                break;

                            case 2: // RGB
                                bytesPerPixel = 3;
                                break;

                            case 6: // RGBA
                                bytesPerPixel = 4;
                                break;
                        }

                        rawPixels = new byte[width * height * bytesPerPixel];
                        break;

                    case ChunkType.PLTE:
                        break;

                    case ChunkType.IDAT:
                        if (idat == null)
                        {
                            idat = chunkData;
                        }
                        else
                        {
                            var newIdat = new byte[idat.Length + chunkData.Length];

                            Buffer.BlockCopy(idat, 0, newIdat, 0, idat.Length);
                            Buffer.BlockCopy(chunkData, 0, newIdat, idat.Length, chunkData.Length);

                            idat = newIdat;
                        }
                        break;
                }

                var chunkCrc = new byte[4];
                stream.Read(chunkCrc, 0, 4);

                var crc = BinaryHelper.ReadBigEndianUInt32(chunkCrc, 0);
            }

            if (idat == null)
                throw new PixelEngineDotNetException("No IDAT chunk found.");

            using (MemoryStream chunkDataStream = new MemoryStream(idat))
            {
                // Read past the first two bytes of the zlib header
                chunkDataStream.Seek(2, SeekOrigin.Begin);

                int pixelsOffset = 0;
                byte[] scanline = new byte[width * bytesPerPixel];
                byte[] previousScanline = new byte[scanline.Length];

                using (var deflate = new DeflateStream(chunkDataStream, CompressionMode.Decompress))
                {
                    for (int i = 0; i < height; i++)
                    {
                        var scanlineFilterAlgorithm = deflate.ReadByte();
                        deflate.Read(scanline, 0, scanline.Length);

                        if (scanlineFilterAlgorithm != 0)
                        {
                            for (int x = 0; x < scanline.Length; x++)
                            {
                                switch (scanlineFilterAlgorithm)
                                {
                                    case 1: // Sub
                                        throw new PixelEngineDotNetException("Sub filter algorithm in PNG not implemented.");

                                    case 2: // Up
                                        scanline[x] = (byte)(scanline[x] + previousScanline[x]);
                                        break;

                                    case 3: // Average
                                        throw new PixelEngineDotNetException("Average filter algorithm in PNG not implemented.");

                                    case 4: // Paeth
                                        throw new PixelEngineDotNetException("Paeth filter algorithm in PNG not implemented.");
                                }
                            }
                        }

                        Buffer.BlockCopy(scanline, 0, rawPixels, pixelsOffset, scanline.Length);
                        pixelsOffset += scanline.Length;

                        Buffer.BlockCopy(scanline, 0, previousScanline, 0, scanline.Length);
                    }
                }
            }

            var pixels = new Pixel[width * height];
            int rawOffset;

            for (int i = 0; i < pixels.Length; i++)
            {
                switch (bytesPerPixel)
                {
                    case 1:
                        //pixels[i] = ((int)width, (int)height, PixelHelper.ToPixelArray<Gray8>(rawPixels));

                    case 3:
                        rawOffset = i * 3;
                        pixels[i].R = rawPixels[rawOffset];
                        pixels[i].G = rawPixels[rawOffset + 1];
                        pixels[i].B = rawPixels[rawOffset + 2];
                        break;

                    case 4:
                        rawOffset = i * 4;
                        pixels[i].R = rawPixels[rawOffset];
                        pixels[i].G = rawPixels[rawOffset + 1];
                        pixels[i].B = rawPixels[rawOffset + 2];
                        pixels[i].A = rawPixels[rawOffset + 3];
                        break;

                    default:
                        throw new PixelEngineDotNetException($"PNG loading not implemented for images with {bytesPerPixel} bytes per pixel.");
                }
            }

            return new ImageData((int)width, (int)height, pixels);
        }

        private static ChunkType ReadChunkType(byte[] data, int offset)
        {
            if (data[offset] == 'I')
            {
                if (data[offset + 1] == 'D' && data[offset + 2] == 'A' && data[offset + 3] == 'T')
                {
                    return ChunkType.IDAT;
                }
                else if (data[offset + 1] == 'E' && data[offset + 2] == 'N' && data[offset + 3] == 'D')
                {
                    return ChunkType.IEND;
                }
                else if (data[offset + 1] == 'H' && data[offset + 2] == 'D' && data[offset + 3] == 'R')
                {
                    return ChunkType.IHDR;
                }
            }
            else if (data[offset] == 'P' && data[offset + 1] == 'L' && data[offset + 2] == 'T' && data[offset + 3] == 'E')
            {
                return ChunkType.PLTE;
            }

            return ChunkType.Other;
        }
    }
}
